using HMACAuthentication.APIServer.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HMACAuthentication.APIServer.Middlewares
{
    /// <summary>
    /// The HMACMiddleware
    /// </summary>
    public class HMACMiddleware
    {
        /// <summary>
        /// The next
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        /// The memory cache
        /// </summary>
        private readonly IMemoryCache _memoryCache;
        /// <summary>
        /// Gets or sets the hmac configurations.
        /// </summary>
        /// <value>
        /// The hmac configurations.
        /// </value>
        private HMACConfigurations HMACConfigurations { get; set; }
        /// <summary>
        /// The allowed apps
        /// </summary>
        private static readonly Dictionary<string, string> allowedApps = new Dictionary<string, string>();
        /// <summary>
        /// The authentication scheme
        /// </summary>
        private readonly string authenticationScheme = "amx";
        /// <summary>
        /// The request maximum age in seconds
        /// </summary>
        private readonly ulong requestMaxAgeInSeconds = 300;  //5 mins
        /// <summary>
        /// Initializes a new instance of the <see cref="HMACMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="hmacConfigurations">The hmac configurations.</param>
        /// <param name="memoryCache">The memory cache.</param>
        /// <exception cref="ArgumentNullException">next</exception>
        public HMACMiddleware(
            RequestDelegate next,
            IOptions<HMACConfigurations> hmacConfigurations,
            IMemoryCache memoryCache)
        {
            _next              = next ?? throw new ArgumentNullException(nameof(next));
            _memoryCache       = memoryCache;
            HMACConfigurations = hmacConfigurations.Value;

            /// TODO :  allowedApps daha sonra SQL e bağlanıp appsettings.json dan çıkarılacaktır.
            /// Burdan başlayıp

            if (allowedApps.Count == 0)
            {
                AllowedApplication allowedApplication = HMACConfigurations.AllowedApplication;

                if (allowedApplication.Web.IsActive)
                {
                    allowedApps.Add(allowedApplication.Web.ID, allowedApplication.Web.Key);
                }

                if (allowedApplication.Android.IsActive)
                {
                    allowedApps.Add(allowedApplication.Android.ID, allowedApplication.Android.Key);
                }

                if (allowedApplication.IPhone.IsActive)
                {
                    allowedApps.Add(allowedApplication.IPhone.ID, allowedApplication.IPhone.Key);
                }
            }
            /// Burda biten satırlar SQL den gelecek kayıtlara göre düzenlenecektir.
        }
        /// <summary>
        /// Invokes the specified HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            HttpRequest request   = httpContext.Request;
            HttpResponse response = httpContext.Response;

            string xAuthorizationValue = string.Empty;

            if (HMACConfigurations.IsDisable)
            {
                await _next.Invoke(httpContext);
                return;
            }

            if (!request.Headers.TryGetValue("x-Authorization-Schema", out StringValues xAuthSchemaValues))
            {
                UnauthorizedObjectResult(httpContext, "Authorization schema not defined.");
                return;
            }

            xAuthorizationValue = xAuthSchemaValues[0];

            if (!string.Equals(authenticationScheme, xAuthorizationValue, StringComparison.OrdinalIgnoreCase))
            {
                UnauthorizedObjectResult(httpContext, "Authorization schema not found.");
                return;
            }

            if (!request.Headers.TryGetValue("x-Authorization", out StringValues xAuthValues))
            {
                UnauthorizedObjectResult(httpContext, "Authorization not defined.");
                return;
            }
            string rawXAuthorizationHeader = xAuthValues[0];
            string[] autherizationHeaderArray = GetAuthorizatioHeaderValues(rawXAuthorizationHeader);

            if (autherizationHeaderArray == null)
            {
                UnauthorizedObjectResult(httpContext, "Authorization parameters failed.");
                return;
            }

            string appId                   = autherizationHeaderArray[0];
            string incomingBase64Signature = autherizationHeaderArray[1];
            string nonce                   = autherizationHeaderArray[2];
            string requestTimeStamp        = autherizationHeaderArray[3];

            bool isValid = await IsValidRequestAsync(httpContext, appId, incomingBase64Signature, nonce, requestTimeStamp);

            if (!isValid)
            {
                UnauthorizedObjectResult(httpContext, "Authorization failed.");
                return;
            }

            /// Tamamsa context'i serbest bırak, işlemine devam etsin.

            await _next.Invoke(httpContext);

            /// Çalıştıktan sonra yapılması gereken bir işlem var ise bu açıklama satırından sonra yazılacak.
        }
        /// <summary>
        /// Unauthorizeds the object result.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="message">The message.</param>
        private void UnauthorizedObjectResult(HttpContext httpContext, string message)
        {
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            httpContext.Response.WriteAsync(message);
        }
        /// <summary>
        /// Gets the authorizatio header values.
        /// </summary>
        /// <param name="rawXAuthorizationHeader">The raw x authorization header.</param>
        /// <returns></returns>
        private string[] GetAuthorizatioHeaderValues(string rawXAuthorizationHeader)
        {
            string[] credArray      = rawXAuthorizationHeader.Split(':');
            return credArray.Length == 4 ? credArray : null;
        }
        /// <summary>
        /// Determines whether [is valid request] [the specified HTTP context].
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="appId">The application identifier.</param>
        /// <param name="incomingBase64Signature">The incoming base64 signature.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="requestTimeStamp">The request time stamp.</param>
        /// <returns>
        ///   <c>true</c> if [is valid request] [the specified HTTP context]; otherwise, <c>false</c>.
        /// </returns>
        private async Task<bool> IsValidRequestAsync(HttpContext httpContext, string appId, string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            string requestContentBase64String = string.Empty;
            string requestUri                 = HttpUtility.UrlEncode($"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path.Value}".ToLower());
            string requestHttpMethod          = httpContext.Request.Method;

            if (!allowedApps.ContainsKey(appId))
            {
                return false;
            }

            string sharedKey = allowedApps[appId];

            if (IsReplayRequest(nonce, requestTimeStamp))
            {
                return false;
            }

            httpContext.Request.EnableRewind();

            string bodyContent = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

            byte[] hash = ComputeHash(bodyContent);

            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }
            /// Performans artışı için string concatenation kullanıldı.

            string data           = appId + requestHttpMethod + requestUri + requestTimeStamp + nonce + requestContentBase64String;
            byte[] secretKeyBytes = Convert.FromBase64String(sharedKey);
            byte[] signature      = Encoding.UTF8.GetBytes(data);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                byte[] signatureBytes  = hmac.ComputeHash(signature);
                string signatureBase64 = Convert.ToBase64String(signatureBytes);

                /// Performans artışı için string.Equals kullanıldı.
                return string.Equals(incomingBase64Signature, signatureBase64, StringComparison.Ordinal);
            }
        }
        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="bodyContent">Content of the body.</param>
        /// <returns></returns>
        private static byte[] ComputeHash(string bodyContent)
        {
            byte[] hash = null;

            if (!string.IsNullOrWhiteSpace(bodyContent))
            {
                using (MD5 md5 = MD5.Create())
                {
                    hash = md5.ComputeHash(Encoding.UTF8.GetBytes(bodyContent));
                }
            }

            return hash;
        }
        /// <summary>
        /// Determines whether [is replay request] [the specified nonce].
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <param name="requestTimeStamp">The request time stamp.</param>
        /// <returns>
        ///   <c>true</c> if [is replay request] [the specified nonce]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            if (_memoryCache.TryGetValue(nonce, out string nonceInCache))
            {
                return true;
            }

            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs  = DateTime.UtcNow - epochStart;

            ulong serverTotalSeconds  = Convert.ToUInt64(currentTs.TotalSeconds);
            ulong requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

            if ((serverTotalSeconds - requestTotalSeconds) > requestMaxAgeInSeconds)
            {
                return true;
            }

            _memoryCache.Set(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(requestMaxAgeInSeconds));

            return false;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    /// <summary>
    /// The HMACMiddlewareExtensions
    /// </summary>
    public static class HMACMiddlewareExtensions
    {
        /// <summary>
        /// Uses the hmac middleware.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseHMACMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HMACMiddleware>();
        }
    }
}