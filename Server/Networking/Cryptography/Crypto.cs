namespace MagnumOpus.Networking.Cryptography;

public class Crypto
{
    /// <summary>
    /// Integer constant used to generate the initialization vector.
    /// </summary>
    static readonly uint P = 0x13FA0F9D;
    /// <summary>
    /// Integer constant used to generate the initialization vector.
    /// </summary>
    static readonly uint G = 0x6D5C7962;

    /// <summary>
    /// The key size in bytes.
    /// </summary>
    private const int KEY_SIZE = 512;

    /// <summary>
    /// The initial key.
    /// </summary>
    readonly byte[] mIV = new byte[KEY_SIZE];
    /// <summary>
    /// The alternate key (to be used for decryption).
    /// </summary>
    readonly byte[] mAltKey = new byte[KEY_SIZE];
    /// <summary>
    /// Whether or not the alternate key is used for decryption.
    /// </summary>
    bool mUsingAltKey = false;

    /// <summary>
    /// The encryption counter.
    /// </summary>
    ushort mEnCounter = 0;
    /// <summary>
    /// The decryption counter.
    /// </summary>
    ushort mDeCounter = 0;

    /// <summary>
    /// Create a new cipher instance. The key will be generated
    /// using the P and G constants.
    /// </summary>
    public unsafe Crypto()
    {
        const int K = KEY_SIZE / 2;

        fixed (uint* _p = &P, _g = &G)
        {
            var p = (byte*)_p;
            var g = (byte*)_g;

            for (var i = 0; i < K; ++i)
            {
                mIV[i + 0] = p[0];
                mIV[i + K] = g[0];
                p[0] = (byte)((p[1] + (byte)(p[0] * p[2])) * p[0] + p[3]);
                g[0] = (byte)((g[1] - (byte)(g[0] * g[2])) * g[0] + g[3]);
            }
        }
    }

    /// <summary>
    /// Generates an alternate key to use for the algorithm and reset
    /// the encryption counter.
    ///
    /// In Conquer Online: A = Token, B = AccountUID
    /// </summary>
    public unsafe void GenerateAltKey(int A, int B)
    {
        const int K = KEY_SIZE / 2;

        var tmp1 = (uint)((A + B) ^ 0x4321 ^ A);
        var tmp2 = tmp1 * tmp1;

        var tmpKey1 = (byte*)&tmp1;
        var tmpKey2 = (byte*)&tmp2;
        for (var i = 0; i < K; ++i)
        {
            mAltKey[i + 0] = (byte)(mIV[i + 0] ^ tmpKey1[i % 4]);
            mAltKey[i + K] = (byte)(mIV[i + K] ^ tmpKey2[i % 4]);
        }
        mUsingAltKey = true;
        mEnCounter = 0;
    }

    /// <summary>
    /// Encrypts data with the algorithm.
    /// </summary>
    public void Encrypt(Span<byte> aBuf, int aLength)
    {
        const int K = KEY_SIZE / 2;

        for (var i = 0; i < aLength; ++i)
        {
            aBuf[i] ^= 0xAB;
            aBuf[i] = (byte)(aBuf[i] >> 4 | aBuf[i] << 4);
            aBuf[i] ^= mIV[(byte)(mEnCounter & 0xFF) + 0];
            aBuf[i] ^= mIV[(byte)(mEnCounter >> 8) + K];
            ++mEnCounter;
        }
    }

    /// <summary>
    /// Decrypts data with the algorithm.
    /// </summary>
    public void Decrypt(Span<byte> aBuf, int aLength)
    {
        const int K = KEY_SIZE / 2;

        var key = mUsingAltKey ? mAltKey : mIV;
        for (var i = 0; i < aLength; ++i)
        {
            aBuf[i] ^= 0xAB;
            aBuf[i] = (byte)(aBuf[i] >> 4 | aBuf[i] << 4);
            aBuf[i] ^= key[(byte)(mDeCounter & 0xFF) + 0];
            aBuf[i] ^= key[(byte)(mDeCounter >> 8) + K];
            ++mDeCounter;
        }
    }

    /// <summary>
    /// Resets the decrypt and the encrypt counters.
    /// </summary>
    public void ResetCounters() { mDeCounter = 0; mEnCounter = 0; }
}
