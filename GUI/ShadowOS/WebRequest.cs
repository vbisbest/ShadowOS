using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowOS
{
    class WebRequest
    {
        public int ID;
        public string Host;
        public string Headers;
        public string Postdata;
        public string RequestLine;

        public override string ToString()
        {
            return Host;
        }
    }
}
