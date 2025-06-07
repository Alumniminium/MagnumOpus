namespace MagnumOpus.Networking.Cryptography
{
    /// <summary>
    /// RC5 cipher implementation for legacy password encryption in Conquer Online protocol.
    /// Uses a fixed 16-byte key with 12 rounds of encryption for backward compatibility with client systems.
    /// Provides block cipher operations on 64-bit data blocks with unsafe memory access for performance.
    /// </summary>
    public static unsafe class RivestCipher5
    {
        public const uint RC5PW32 = 0xB7E15163;
        public const uint RC5QW32 = 0x61C88647;

        private const int RC5_32 = 32;
        private const int RC5_12 = 12;
        private const int RC5_SUB = (RC5_12 * 2) + 2;
        private const int RC5_16 = 16;
        private const int RC5_KEY = RC5_16 / 4;

        private static readonly uint[] mKey = new uint[RC5_KEY];
        private static readonly uint[] mSub = new uint[RC5_SUB];

        static RivestCipher5()
        {
            var aSeed = new byte[RC5_16] { 0x3C, 0xDC, 0xFE, 0xE8, 0xC4, 0x54, 0xD6, 0x7E, 0x16, 0xA6, 0xF8, 0x1A, 0xE8, 0xD0, 0x38, 0xBE };
            fixed (byte* seed = aSeed)
            {
                for (var z = 0; z < RC5_KEY; ++z)
                    mKey[z] = ((uint*)seed)[z];
            }

            mSub[0] = RC5PW32;
            int i, j;
            for (i = 1; i < RC5_SUB; ++i)
                mSub[i] = mSub[i - 1] - RC5QW32;

            uint x, y;
            i = j = 0;
            x = y = 0;
            for (int k = 0, count = 3 * Math.Max(RC5_KEY, RC5_SUB); k < count; ++k)
            {
                mSub[i] = RotL(mSub[i] + x + y, 3);
                x = mSub[i];
                i = (i + 1) % RC5_SUB;
                mKey[j] = RotL(mKey[j] + x + y, x + y);
                y = mKey[j];
                j = (j + 1) % RC5_KEY;
            }
        }

        /// <summary>
        /// Encrypts data using RC5 algorithm with fixed key and 12 rounds.
        /// Data length must be a multiple of 64 bits (8 bytes) for proper block operation.
        /// </summary>
        /// <param name="aBuf">Memory buffer containing data to encrypt in-place</param>
        /// <param name="aLength">Length of data to encrypt (must be multiple of 8)</param>
        public static void Encrypt(ref Memory<byte> aBuf, int aLength)
        {
            if (aLength % 8 != 0)
                throw new ArgumentException("Length of the buffer must be a multiple of 64 bits.", nameof(aLength));

            fixed (byte* buf = aBuf.Span)
            {
                var data = (uint*)buf;
                for (int k = 0, len = aLength / 8; k < len; ++k)
                {
                    var lv = data[2 * k] + mSub[0];
                    var rv = data[(2 * k) + 1] + mSub[1];
                    for (var i = 1; i <= RC5_12; ++i)
                    {
                        lv = RotL(lv ^ rv, rv) + mSub[2 * i];
                        rv = RotL(rv ^ lv, lv) + mSub[(2 * i) + 1];
                    }

                    data[2 * k] = lv;
                    data[(2 * k) + 1] = rv;
                }
            }
        }

        /// <summary>
        /// Decrypts data using RC5 algorithm with fixed key and 12 rounds.
        /// Data length must be a multiple of 64 bits (8 bytes) for proper block operation.
        /// </summary>
        /// <param name="buf">Pointer to buffer containing encrypted data to decrypt in-place</param>
        /// <param name="aLength">Length of data to decrypt (must be multiple of 8)</param>
        public static void Decrypt(byte* buf, int aLength)
        {
            if ((aLength & 7) != 0)
                throw new ArgumentException("Length of the buffer must be a multiple of 64 bits.", nameof(aLength));

            var data = (uint*)buf;

            fixed (uint* sub = mSub)
            {

                for (int k = 0, len = aLength >> 3; k < len; ++k)
                {
                    var lv = data[k << 1];
                    var rv = data[(k << 1) + 1];

                    for (var i = RC5_12; i >= 1; --i)
                    {
                        rv = RotR(rv - sub[(i << 1) + 1], lv) ^ lv;
                        lv = RotR(lv - sub[i << 1], rv) ^ rv;
                    }

                    data[k << 1] = lv - sub[0];
                    data[(k << 1) + 1] = rv - sub[1];
                }
            }
        }

        private static uint RotL(uint aValue, uint aCount) => (aValue << (int)aCount) | (aValue >> (32 - (int)aCount));

        private static uint RotR(uint aValue, uint aCount) => (aValue >> (int)aCount) | (aValue << (32 - (int)aCount));

    }
}