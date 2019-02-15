using System;
using System.Security.Cryptography;

namespace HMACAuthentication.CreateAppIDAndSecretKey
{
    /// <summary>
    /// The Create AppID And SecretKey Program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            string AppID = string.Empty;
            string SecretKey = string.Empty;

            for (int i = 1; i < 10; i++)
            {
                using (RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider())
                {
                    byte[] secretKeyByteArray = new byte[32]; //256 bit

                    cryptoProvider.GetBytes(secretKeyByteArray);

                    SecretKey = Convert.ToBase64String(secretKeyByteArray);
                }

                AppID = Guid.NewGuid().ToString("N");

                Console.WriteLine($"{i} *".PadRight(100, '*'));
                Console.WriteLine($"Application ID  : {AppID}");
                Console.WriteLine($"SecretKey       : {SecretKey}");
                Console.WriteLine("*".PadRight(100, '*'));

                Console.WriteLine("");
            }

            Console.ReadKey();
        }
    }
}
