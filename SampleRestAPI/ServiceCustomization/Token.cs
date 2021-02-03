using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleRestAPI
{
    public class Token
    {
        public string TokenType { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Authorization: {TokenType} {AccessToken}";
        }
    }
}
