using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Security.Principal;

namespace SampleRestAPI
{
    internal class TokenValidationHandler : DelegatingHandler
    {        
        private const string AUD = "sampleapi.com";
        private const string ISS = "sampleapi";
        private const string ACCESS_DENIED = "Access denied.";

        /// <summary>
        /// private static string generateJWTToken(int consumerID, string roles)
        /// </summary>
        /// <param name="consumerID">int</param>
        /// <param name="roles">string</param>
        /// <returns></returns>
        private static string generateJWTToken(int consumerID, string roles)
        {
            var pfxName = WebConfigurationManager.AppSettings["JWT:Sign"];
            string certificatePathX = HostingEnvironment.MapPath($"~/{pfxName}");

            var cert = new X509Certificate2(certificatePathX);

            DateTime centuryBegin = new DateTime(1970, 1, 1);
            var exp = new TimeSpan(DateTime.Now.AddMonths(1).Ticks - centuryBegin.Ticks).TotalSeconds;
            
            var payload = new JwtPayload
            {
                { "iss", ISS },
                { "aud", AUD },
                { "exp", exp },
                { "id" , consumerID },
                { "roles", roles }
            };

            var credentials = new SigningCredentials(new X509SecurityKey(cert), "RS256");
            var JWTHeader = new JwtHeader(credentials);
            var securityTokenX = new JwtSecurityToken(JWTHeader, payload);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtSecurityTokenHandler.WriteToken(securityTokenX);

            return jwtToken;
        }

        /// <summary>
        /// public static AuthToken CreateToken(Consumer data)
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        public static Token CreateToken(Consumer con)
        {
            /*1. Verify Consumer*/
            if (con == null)
                throw new Exception("Invalid.");

            // Verify consumer by your way.

            // This is just my example
            var isValidConsumer = con.ConsumerKey == "AA" && con.ConsumerSecret == "AAAAA";
            if (!isValidConsumer)
            {
                throw new Exception("Access denied.");
            }

            var consumer = new Consumer() { ConsumerID = 1, Roles = "admin" };

            // 2. Generate JwtToken
            string token = generateJWTToken(consumer.ConsumerID, consumer.Roles);

            return new Token { TokenType = "Bearer", AccessToken = token };
        }

        /// <summary>
        /// private static bool tryRetrieveToken(HttpRequestMessage request, out AuthToken authToken)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        private static bool tryRetrieveToken(HttpRequestMessage request, out Token authToken)
        {
            authToken = new Token();

            IEnumerable<string> authzHeaders;
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders))
                return false;

            var authString = authzHeaders.ElementAt(0);
            var items = authString.Split(' ');

            if (items.Length < 1)
                return false;

            authToken.TokenType = items[0];
            authToken.AccessToken = items[1];

            return true;
        }

        /// <summary>
        /// protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpStatusCode statusCode;
            string message = String.Empty;

            Token authToken;
            var hasToken = tryRetrieveToken(request, out authToken);

            // Determine whether a jwt exists or not
            if (!hasToken)
            {
                // Allow requests with no token - whether a action method needs an authentication can be set with the claimsauthorization attribute
                return base.SendAsync(request, cancellationToken);
            }

            try
            {
                var accessToken = authToken.AccessToken;
                IPrincipal principal = null;

                do
                {
                    SecurityToken securityToken;
                    System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                    var certName = WebConfigurationManager.AppSettings["JWT:Verify"];
                    string certificatePath = HostingEnvironment.MapPath($"~/{certName}");

                    X509Certificate2 defaultCert = new X509Certificate2(certificatePath);
                    X509SecurityKey defaultKey = new X509SecurityKey(defaultCert);
                    SigningCredentials signingCredentials = new SigningCredentials(defaultKey, SecurityAlgorithms.RsaSha256Signature);

                    TokenValidationParameters validationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = ISS,
                        ValidAudience = AUD,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        LifetimeValidator = this.LifetimeValidator,
                        IssuerSigningKey = defaultKey,
                    };

                    // Get Token and doing something what you want to.
                    //var secToken = handler.ReadToken(accessToken) as JwtSecurityToken;
                    //var id = secToken.Claims.First(claim => claim.Type == "id").Value;
                    
                    principal = handler.ValidateToken(accessToken, validationParameters, out securityToken);
                } while (false);

                HttpContext.Current.User = Thread.CurrentPrincipal = principal;

                return base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = ex.Message;
            }

            return Task<HttpResponseMessage>.Factory.StartNew(() => request.CreateErrorResponse(statusCode, message));
        }

        /// <summary>
        /// public bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        /// </summary>
        /// <param name="notBefore"></param>
        /// <param name="expires"></param>
        /// <param name="securityToken"></param>
        /// <param name="validationParameters"></param>
        /// <returns></returns>
        public bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires != null)
                return DateTime.UtcNow < expires;

            return false;
        }
    }
}