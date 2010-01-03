using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public static class HTTPWebResponseExtensions
    {
        public static string GetResponseString(this WebResponse response)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buffer =new byte[1024];
            int read = 0;
            using (var stream = response.GetResponseStream())
            {
                do
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if (read != 0)
                        sb.Append(Encoding.ASCII.GetString(buffer, 0, read));
                } while (read > 0);
            }
            return sb.ToString();
        }
    }
}
