namespace MagnumOpus.Networking.Cryptography
{
    public sealed unsafe class TQCipher
    {
        // Static fields and properties
        private static readonly Memory<byte> KInit = new byte[0x200];

        // Local fields and properties
        private readonly Memory<byte> K;
        private readonly Memory<byte> K1 = new byte[0x200];
        private readonly Memory<byte> K2 = new byte[0x200];
        private ushort DecryptCounter, EncryptCounter;
        private static readonly byte[] seed = new byte[] { 0x9D, 0x0F, 0xFA, 0x13, 0x62, 0x79, 0x5C, 0x6D };

        static TQCipher()
        {
            for (var i = 0; i < 0x100; i++)
            {
                KInit.Span[i] = seed[0];
                KInit.Span[i + 0x100] = seed[4];
                seed[0] = (byte)(((seed[1] + (seed[0] * seed[2])) * seed[0]) + seed[3]);
                seed[4] = (byte)(((seed[5] - (seed[4] * seed[6])) * seed[4]) + seed[7]);
            }
        }

        public TQCipher()
        {
            KInit.CopyTo(K1);
            KInit.CopyTo(K2);
            K = K1;
        }

        public void Decrypt(Span<byte> src, Span<byte> dst) => XOR(src, dst, K, ref DecryptCounter);
        public void Encrypt(Span<byte> src) => XOR(src, src, K1, ref EncryptCounter);
        private void XOR(Span<byte> src, Span<byte> dst, Memory<byte> k, ref ushort c)
        {
            byte* pSrc = null, pDst = null, pK = null;
            var delta = (ushort)src.Length;
            c ^= delta;
            var x = (ushort)(c - delta);

            fixed (byte* pTmp = src, pOut = dst, pKey = k.Span)
            {
                pSrc = pTmp;
                pDst = pOut;
                pK = pKey;

                for (var i = 0; i < src.Length; i++)
                {
                    pDst[i] = (byte)(*pSrc++ ^ 0xAB);
                    pDst[i] = (byte)((pDst[i] >> 4) | (pDst[i] << 4));
                    pDst[i] = (byte)(pDst[i] ^ pK[x & 0xff]);
                    pDst[i] = (byte)(pDst[i] ^ pK[(x >> 8) + 0x100]);
                    x++;
                }
            }
        }
    }
}