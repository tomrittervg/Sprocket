using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public struct SQLParam
    {
        public string Name { get; set; }
        public string Type { get; set; }

        /// <param name="n">The Name of the Parameter</param>
        /// <param name="t">The Type of the Parameter</param>
        public SQLParam(string n, string t) : this()
        {
            Name = n;
            Type = t;
        }
    }
}
