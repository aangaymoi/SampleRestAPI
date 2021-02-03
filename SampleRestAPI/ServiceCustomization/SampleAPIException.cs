using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace SampleRestAPI
{
    public class SampleAPIException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public SampleAPIException(string message)
            : this(HttpStatusCode.Forbidden, message)
        {
        }

        public SampleAPIException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}