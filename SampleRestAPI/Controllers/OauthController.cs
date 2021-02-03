using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SampleRestAPI.Controllers
{
    [RoutePrefix("api/v1/oauth")]
    public class OauthController : ApiController
    {
        [HttpPost]
        [Route("request_token")]
        public object RequestToken([FromBody] Consumer consumer)
        {
            try
            {
                return TokenValidationHandler.CreateToken(consumer);
            }
            catch (Exception ex)
            {
                throw new SampleAPIException(HttpStatusCode.Forbidden, ex.Message);
            }
        }
    }
}