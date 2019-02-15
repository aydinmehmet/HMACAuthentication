using HMACAuthentication.Handlers.Common;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HMACAuthentication.ClientApplication
{
    /// <summary>
    /// The HMACAuthentication Test Program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The application identifier
        /// </summary>
        private static readonly string APPId = "4d53bce03ec34c0a911182d4c228ee6c";
        /// <summary>
        /// The API key
        /// </summary>
        private static readonly string APIKey = "A93reRTUJHsCuQSHR+L3GxqOJyDmQpCgps102ciuabc=";
        /// <summary>
        /// The base adress
        /// </summary>
        private const string baseAdress = "https://localhost:44370/";
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            RunAsync().Wait();
        }
        /// <summary>
        /// Runs the asynchronous.
        /// </summary>
        /// <returns></returns>
        private static async Task RunAsync()
        {
            Console.WriteLine("Press any key to continue..");

            Console.ReadKey();

            HMACDelegatingHandler hmacDelegatingHandler = new HMACDelegatingHandler(appId: APPId, apiKey: APIKey);

            HttpClient client = HttpClientFactory.Create(hmacDelegatingHandler);

            PostModel model = new PostModel()
            {
                Value = ".net core client application"
            };

            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync($"{baseAdress}api/values", stringContent);

            string responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"\n\rHTTP Status: {response.StatusCode}\n\rReason {response.ReasonPhrase}\n\rMessage {responseString}.\n\rPress ENTER to exit");
            }
            else
            {
                Console.WriteLine($"\n\rFailed to call the API. HTTP Status: {response.StatusCode}\n\rReason {response.ReasonPhrase}\n\rMessage {responseString}\n\rPress ENTER to exit");
            }

            Console.ReadKey();
        }
    }
    public class PostModel
    {
        public string Value { get; set; }
    }
}
