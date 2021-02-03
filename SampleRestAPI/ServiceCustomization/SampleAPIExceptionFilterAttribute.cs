using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http.Filters;

namespace SampleRestAPI
{
    public class SampleAPIExceptionFilterAttribute : ExceptionFilterAttribute, IExceptionFilter
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            // Check the Exception Type
            if (actionExecutedContext.Exception is SampleAPIException)
            {
                var ex = actionExecutedContext.Exception as SampleAPIException;
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
            else if (actionExecutedContext.Exception is SampleAPIException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Access denied.");
            }
        }
    }
}