using Org.BouncyCastle.Math;

namespace MagnumOpus.Networking.Cryptography
{
    /// <summary>
    /// DiffieHellman key exchange implementation for establishing shared secrets with Conquer Online clients.
    /// Uses BouncyCastle's BigInteger implementation for cryptographic operations with predefined parameters.
    /// Enables secure session key establishment without transmitting private keys over the network.
    /// </summary>
    public sealed class DiffieHellman
    {
        private const string DefaultGenerator = "05";
        private const string DefaultPrimativeRoot = "E7A69EBDF105F2A6BBDEAD7E798F76A209AD73FB466431E2E7352ED262F8C558F10BEFEA977DE9E21DCEE9B04D245F300ECCBBA03E72630556D011023F9E857F";
        internal static string G => DefaultGenerator;
        internal static string P => DefaultPrimativeRoot;

        // Key exchange Properties
        public BigInteger PrimeRoot { get; set; }
        public BigInteger Generator { get; set; }
        public BigInteger Modulus { get; set; }
        public BigInteger PublicKey { get; private set; }
        public BigInteger PrivateKey { get; private set; }

        /// <summary>
        /// Instantiates a new instance of the <see cref="DiffieHellman"/> key exchange.
        /// If no prime root or generator is specified, then defaults for remaining W
        /// interoperable with the Conquer Online game client will be used. 
        /// </summary>
        /// <param name="p">Prime root to modulo with the generated probable prime.</param>
        /// <param name="g">Generator used to seed the modulo of primes.</param>
        public DiffieHellman(string p = DefaultPrimativeRoot, string g = DefaultGenerator)
        {
            PrimeRoot = new BigInteger(p, 16);
            Generator = new BigInteger(g, 16);
            Modulus = new BigInteger(P, 16);
            PrivateKey = new BigInteger(PrimeRoot.BitLength, new Random());
            PublicKey = Generator.ModPow(PrivateKey, Modulus);
        }

        /// <summary>
        /// Computes the public key for transmission to the client during key exchange.
        /// Generates a new probable prime and calculates the public key using modular exponentiation.
        /// </summary>
        public void ComputePublicKey()
        {
            Modulus = BigInteger.ProbablePrime(256, new Random());
            PublicKey = Generator.ModPow(Modulus, PrimeRoot);
        }

        /// <summary>
        /// Computes the shared private key using the client's public key from the exchange.
        /// The resulting private key becomes the shared secret for symmetric encryption.
        /// </summary>
        /// <param name="clientKeyString">Hexadecimal string representation of client's public key</param>
        public void ComputePrivateKey(string clientKeyString)
        {
            BigInteger clientKey = new(clientKeyString, 16);
            PrivateKey = clientKey.ModPow(Modulus, PrimeRoot);
        }

        /// <summary>
        /// Gets the public key as a hexadecimal string for transmission to the client.
        /// </summary>
        /// <returns>Hexadecimal string representation of the public key</returns>
        public string GetPublicKey() => PublicKey.ToString(16);
        
        /// <summary>
        /// Gets the computed private key as a byte array for use with symmetric ciphers.
        /// </summary>
        /// <returns>Byte array representation of the shared private key</returns>
        public byte[] GetPrivateKey() => PrivateKey.ToByteArrayUnsigned();
    }
}
