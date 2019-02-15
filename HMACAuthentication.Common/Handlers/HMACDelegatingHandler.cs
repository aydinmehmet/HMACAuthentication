using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HMACAuthentication.Handlers.Common
{
    /// <summary>
    /// The HMACDelegatingHandler
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    public class HMACDelegatingHandler : DelegatingHandler
    {
        /// <summary>
        /// The application identifier
        /// </summary>
        private readonly string _appId;
        /// <summary>
        /// The API key
        /// </summary>
        private readonly string _apiKey;
        /// <summary>
        /// Initializes a new instance of the <see cref="HMACDelegatingHandler"/> class.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="apiKey">The API key.</param>
        public HMACDelegatingHandler(string appId, string apiKey)
        {
            _appId  = appId;
            _apiKey = apiKey;

        }
        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response      = null;
            byte[] requestContentHash         = null;
            string requestContentBase64String = string.Empty;
            string requestUri                 = System.Web.HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());
            string requestHttpMethod          = request.Method.Method;
            DateTime epochStart               = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan                 = DateTime.UtcNow - epochStart;

            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
            string nonce            = Guid.NewGuid().ToString("N");

            if (request.Content != null)
            {
                string contentString = await request.Content.ReadAsStringAsync();
                byte[] content       = Encoding.UTF8.GetBytes(contentString);

                using (MD5 md5 = MD5.Create())
                {
                    requestContentHash = md5.ComputeHash(content);
                }

                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            string signatureRawData   = $"{_appId}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentBase64String}";
            byte[] secretKeyByteArray = Convert.FromBase64String(_apiKey);
            byte[] signature          = Encoding.UTF8.GetBytes(signatureRawData);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes               = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);

                request.Headers.Add("x-Authorization-Schema", "amx");
                request.Headers.Add("x-Authorization", $"{_appId}:{requestSignatureBase64String}:{nonce}:{requestTimeStamp}");
            }

            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
