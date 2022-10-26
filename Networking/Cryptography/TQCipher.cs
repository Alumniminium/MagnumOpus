namespace MagnumOpus.Networking.Cryptography
{
    public sealed class TQCipher
    {
        // Static fields and properties
        private static readonly Memory<byte> KInit = new byte[0x200];

        // Local fields and properties
        private Memory<byte> K;
        private readonly Memory<byte> K1 = new byte[0x200];
        private readonly Memory<byte> K2 = new byte[0x200];
        private ushort DecryptCounter, EncryptCounter;

        /// <summary>
        /// Add defines how the cipher increments counters. By default, counters are 
        /// incremented without thread-safety for synchronous reads and writes.
        /// </summary>
        public Increment Add;

        /// <summary>
        /// Initializes static variables for <see cref="TQCipher"/>. Generates the static
        /// IV using a static, default seed for Conquer Online. Since the seed never
        /// changes across clients or instantiations, only needs to be computed once.
        /// </summary>
        static TQCipher()
        {
            var seed = new byte[] { 0x9D, 0x0F, 0xFA, 0x13, 0x62, 0x79, 0x5C, 0x6D };
            for (int i = 0; i < 0x100; i++)
            {
                KInit.Span[i] = seed[0];
                KInit.Span[i + 0x100] = seed[4];
                seed[0] = (byte)((seed[1] + (seed[0] * seed[2])) * seed[0] + seed[3]);
                seed[4] = (byte)((seed[5] - (seed[4] * seed[6])) * seed[4] + seed[7]);
            }
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="TQCipher"/> using pregenerated
        /// IVs for initializing the cipher's keystreams. Initialized on each server
        /// to start communication. The game server will also require that keys are
        /// regenerated using key derivations from the client's first packet. Increments
        /// counters without thread-safety for synchronous reads and writes. 
        /// </summary>
        public TQCipher()
        {
            Add = DefaultIncrement;
            KInit.CopyTo(K1);
            KInit.CopyTo(K2);
            K = K1;
        }

        /// <summary>
        /// Generates keys for the game server using the player's server access token 
        /// as a key derivation variable. Invoked after the first packet is received on
        /// the game server.
        /// </summary>
        /// <param name="seeds">Array of seeds for generating keys</param>
        public void GenerateKeys(ulong[] seeds)
        {
            var seed = seeds[0];
            var a = (uint)(seed >> 32);
            var b = (uint)seed;
            var c = (a + b) ^ 0x4321 ^ a;
            var d = c * c;

            var temp1 = BitConverter.GetBytes(c);
            var temp2 = BitConverter.GetBytes(d);
            for (int i = 0; i < 0x100; i++)
            {
                K2.Span[i] = (byte)(K1.Span[i] ^ temp1[i % 4]);
                K2.Span[i + 0x100] = (byte)(K1.Span[i + 0x100] ^ temp2[i % 4]);
            }

            K = K2;
            EncryptCounter = 0;
        }

        /// <summary>
        /// Decrypts the specified span by XORing the source span with the cipher's 
        /// keystream. The source and destination may be the same slice, but otherwise
        /// should not overlap.
        /// </summary>
        /// <param name="src">Source span that requires decrypting</param>
        /// <param name="dst">Destination span to contain the decrypted result</param>
        public void Decrypt(Span<byte> src, Span<byte> dst) => XOR(src, dst, K, ref DecryptCounter);

        /// <summary>
        /// Encrypt the specified span by XORing the source span with the cipher's 
        /// keystream. The source and destination may be the same slice, but otherwise
        /// should not overlap.
        /// </summary>
        /// <param name="src">Source span that requires encrypting</param>
        /// <param name="dst">Destination span to contain the encrypted result</param>
        public void Encrypt(Span<byte> src) => XOR(src, src, K1, ref EncryptCounter);

        /// <summary>
        /// XOR sets the destination span of bytes with the result of XORing the source
        /// span with the cipher's keystreams. The source and destination may be the same
        /// slice, but otherwise should not overlap.
        /// </summary>
        /// <param name="src">Source span that requires decrypting</param>
        /// <param name="dst">Destination span to contain the decrypted result</param>
        /// <param name="k">Keystream to be used for XORing data</param>
        /// <param name="c">Counter for the direction of the cipher operation</param>
        private void XOR(Span<byte> src, Span<byte> dst, Memory<byte> k, ref ushort c)
        {
            var x = Add(ref c, src.Length);
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = (byte)(src[i] ^ 0xAB);
                dst[i] = (byte)(dst[i] >> 4 | dst[i] << 4);
                dst[i] = (byte)(dst[i] ^ k.Span[x & 0xff]);
                dst[i] = (byte)(dst[i] ^ k.Span[(x >> 8) + 0x100]);
                x++;
            }
        }

        /// <summary>
        /// Increments the counter used for decryption or encryption using the keystream.
        /// Allows the server to specify thread safety for parallel reads and writes, or
        /// use the default (non-thread safe) increment for synchronized reads and writes.
        /// </summary>
        /// <param name="x">Value to be incremented</param>
        /// <param name="n">Amount to increment by</param>
        /// <returns>Returns the previous value.</returns>
        public delegate ushort Increment(ref ushort x, int n);

        /// <summary>
        /// Increments without thread-safety for <see cref="Increment"/>
        /// </summary>
        /// <param name="x">Value to be incremented</param>
        /// <param name="n">Amount to increment by</param>
        /// <returns>Returns the previous value.</returns>
        public static ushort DefaultIncrement(ref ushort x, int n) => (ushort)((x = (ushort)(x + n)) - n);
    }
}