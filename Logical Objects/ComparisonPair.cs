using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public struct ComparisonPair
    {
        public string Old { get; set; }
        public string New { get; set; }

        public ComparisonPair(string o, string n) : this()
        {
            Old = o;
            New = n;
        }
    }
}
