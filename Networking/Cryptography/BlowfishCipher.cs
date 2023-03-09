// * Originally created by CptSky (February 15th, 2012)
// * Copyright (C) 2012 CptSky

using System.Runtime.CompilerServices;
using System.Text;

namespace MagnumOpus.Networking.Cryptography
{
    public sealed class BlowfishCipher
    {
        // Constants and static properties
        public static readonly BlowfishCipher Default;
        public const int BlockSize = 8;
        private const int KeySize = 72;
        private const int Rounds = 16;
        private const string DefaultSeed = "DR654dt34trg4UI6";

        // Local fields and properties
        public Memory<byte> DecryptionIV { get; private set; }
        public Memory<byte> EncryptionIV { get; private set; }
        public uint[]? P { get; private set; }
        public uint[]? S { get; private set; }
        public int DecryptCount, EncryptCount;

        /// <summary>Create a default instance of Blowfish to copy from.</summary>
        static BlowfishCipher()
        {
            Default = new BlowfishCipher();
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="BlowfishCipher"/> and generates 
        /// keys using the client's shared secret seed. Keys should be regenerated after
        /// the DH Key Exchange has established a new shared secret.
        /// </summary>
        /// <param name="seed">Shared secret seed for generating keys</param>
        public BlowfishCipher(string seed = DefaultSeed)
        {
            DecryptionIV = new byte[BlockSize];
            EncryptionIV = new byte[BlockSize];
            DecryptCount = 0;
            EncryptCount = 0;

            GenerateKeys(Encoding.ASCII.GetBytes(seed));
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="BlowfishCipher"/> without 
        /// generating new keys. Instead, keys will be copied from the specified cipher
        /// instance which has already generated keys based on a shared secret.
        /// </summary>
        /// <param name="copy">The cipher to copy keys from</param>
        public BlowfishCipher(BlowfishCipher copy)
        {
            DecryptionIV = new byte[BlockSize];
            EncryptionIV = new byte[BlockSize];

            copy.DecryptionIV.CopyTo(DecryptionIV);
            copy.EncryptionIV.CopyTo(EncryptionIV);
            DecryptCount = copy.DecryptCount;
            EncryptCount = copy.EncryptCount;

            if (copy.S != null && copy.P != null)
            {
                S = new uint[copy.S.Length];
                P = new uint[copy.P.Length];

                copy.S.CopyTo(S, 0);
                copy.P.CopyTo(P, 0);
            }
        }

        /// <summary>
        /// The key schedule for Blowfish can be time consuming to generate. The server
        /// should optimize around calls to generate keys. When providing an array of
        /// seeds, only the first seed will be used to generate keys.
        /// </summary>
        /// <param name="seeds">An array of seeds used to generate keys</param>
        public void GenerateKeys(byte[] seedBuffer)
        {
            // Initialize key buffers
            if (seedBuffer.Length > KeySize)
                Array.Resize(ref seedBuffer, KeySize);

            P = (uint[])Keys.PInit.Clone();
            S = (uint[])Keys.SInit.Clone();

            // Generate keys
            for (uint i = 0, x = 0; i < P.Length; i++)
            {
                uint rv = seedBuffer[x]; x = (uint)((x + 1) % seedBuffer.Length);
                rv = (rv << 8) | seedBuffer[x]; x = (uint)((x + 1) % seedBuffer.Length);
                rv = (rv << 8) | seedBuffer[x]; x = (uint)((x + 1) % seedBuffer.Length);
                rv = (rv << 8) | seedBuffer[x]; x = (uint)((x + 1) % seedBuffer.Length);
                P[i] ^= rv;
            }

            var block = new uint[BlockSize / sizeof(uint)];
            for (var i = 0; i < P.Length;)
            {
                EncipherBlock(block);
                P[i++] = block[0];
                P[i++] = block[1];
            }

            for (var i = 0; i < S.Length;)
            {
                EncipherBlock(block);
                S[i++] = block[0];
                S[i++] = block[1];
            }
        }

        /// <summary>Sets the IVs of the cipher.</summary>
        /// <param name="decryptionIV">Decryption IV from client key exchange</param>
        /// <param name="encryptionIV">Encryption IV from client key exchange</param>
        public void SetIVs(in Memory<byte> decryptionIV, Memory<byte> encryptionIV)
        {
            decryptionIV.CopyTo(DecryptionIV);
            encryptionIV.CopyTo(EncryptionIV);
            DecryptCount = 0;
            EncryptCount = 0;
        }

        /// <summary>
        /// Decrypts bytes using cipher feedback mode. The source and destination may be
        /// the same slice, but otherwise should not overlap.
        /// </summary>
        /// <param name="src">Source span that requires decrypting</param>
        /// <param name="dst">Destination span to contain the decrypted result</param>
        public void Decrypt(Span<byte> src)
        {
            var block = new uint[2];
            for (var i = 0; i < src.Length; i++)
            {
                if (DecryptCount == 0)
                {
                    block[0] = N21(DecryptionIV, 0);
                    block[1] = N21(DecryptionIV, 4);
                    EncipherBlock(block);
                    N12(DecryptionIV, 0, block[0]);
                    N12(DecryptionIV, 4, block[1]);
                }

                var tmp = DecryptionIV.Span[DecryptCount];
                DecryptionIV.Span[DecryptCount] = src[i];
                src[i] = (byte)(src[i] ^ tmp);
                DecryptCount = (DecryptCount + 1) & (BlockSize - 1);
            }
        }

        /// <summary>
        /// Encrypts bytes using cipher feedback mode. The source and destination may be
        /// the same slice, but otherwise should not overlap.
        /// </summary>
        /// <param name="src">Source span that requires encrypting</param>
        /// <param name="dst">Destination span to contain the encrypted result</param>
        public void Encrypt(Span<byte> src)
        {
            var block = new uint[2];
            for (var i = 0; i < src.Length; i++)
            {
                if (EncryptCount == 0)
                {
                    block[0] = N21(EncryptionIV, 0);
                    block[1] = N21(EncryptionIV, 4);
                    EncipherBlock(block);
                    N12(EncryptionIV, 0, block[0]);
                    N12(EncryptionIV, 4, block[1]);
                }

                src[i] = (byte)(src[i] ^ EncryptionIV.Span[EncryptCount]);
                EncryptionIV.Span[EncryptCount] = src[i];
                EncryptCount = (EncryptCount + 1) & (BlockSize - 1);
            }
        }

        /// <summary>
        /// Function F is Blowfish's one-way function for achieving non-linearity with 
        /// its substitution boxes without requiring a massive lookup array. Each input
        /// is broken up into 4 bytes which is used as indexes to fetch 32-bit numbers
        /// from the different S boxes.
        /// </summary>
        /// <param name="x">Input from the encipher round</param>
        /// <returns>((Sa + Sb) ^ Sc) + Sd, S being the substitution per byte.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint F(uint x)
        {
            return S == null
                ? throw new InvalidOperationException("Cipher must be initialized with a shared secret!")
                : (((S[(x >> 24) & 0xFF] + S[0x100 + ((x >> 16) & 0xFF)]) ^ S[0x200 + ((x >> 8) & 0xFF)]) + S[0x300 + ((x) & 0xFF)]) & 0xFFFFFFFF;
        }

        /// <summary>
        /// Swaps the endianness and converts data types for block operations.
        /// </summary>
        /// <param name="iv">IV depending on the direction of the cipher</param>
        /// <param name="x">Index used to read from the IV buffer</param>
        /// <returns>An unsigned integer representing a side of the block.</returns>
        private static uint N21(in Memory<byte> iv, int x)
        {
            var l = (uint)(iv.Span[x] << 24);
            l |= (uint)(iv.Span[x + 1] << 16);
            l |= (uint)(iv.Span[x + 2] << 8);
            l |= iv.Span[x + 3];
            return l;
        }

        /// <summary>
        /// Converts on half of the results of a block operation back to bytes. 
        /// </summary>
        /// <param name="iv">IV depending on the direction of the cipher</param>
        /// <param name="x">Index used to write to the IV buffer</param>
        /// <param name="v">Value from the block operation results.</param>
        private static void N12(in Memory<byte> iv, int x, uint v)
        {
            iv.Span[x] = (byte)((v >> 24) & 0xFF);
            iv.Span[x + 1] = (byte)((v >> 16) & 0xFF);
            iv.Span[x + 2] = (byte)((v >> 8) & 0xFF);
            iv.Span[x + 3] = (byte)((v) & 0xFF);
        }

        /// <summary>
        /// Enciphers a block of plaintext into ciphertext. If the cipher is used in 
        /// cipher feedback mode, then this method is also utilized for decrypting
        /// ciphertext into plaintext.
        /// </summary>
        /// <param name="block">A block of two 32-bit unsigned integers</param>
        private void EncipherBlock(uint[] block)
        {
            if (P == null)
                throw new InvalidOperationException("Cipher must be initialized with a shared secret!");
            var lv = block[0];
            var rv = block[1];

            lv ^= P[0];
            for (uint i = 1; i <= Rounds; i++)
            {
                rv ^= P[i];
                rv ^= F(lv);
                (rv, lv) = (lv, rv);
            }

            rv ^= P[Rounds + 1];
            block[0] = rv & 0xFFFFFFFF;
            block[1] = lv & 0xFFFFFFFF;
        }
    }
}
