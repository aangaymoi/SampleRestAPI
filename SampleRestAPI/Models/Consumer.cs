using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleRestAPI
{
    public class Consumer
    {
        public int ConsumerID { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string Roles { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
                return false;

            return (obj as Consumer).ConsumerID == this.ConsumerID;
        }

        public override int GetHashCode()
        {
            return this.ConsumerID;
        }
    }
}