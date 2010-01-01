using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    /*
     * I throw these when something not-totally-out-of-bounds has gone wrong, and catch them and handle them in intelligent ways.
     * I use my own class because I don't want to catch .Net's exceptions
     */ 
    public class SpecificException : Exception
    {
        public const string InputCouldNotBeParsed = "Input file could not be parsed by my Regex.";
        public const string ProcParametersDontMatch = "Proc Parameters Don't Match";

        public SpecificException() : base() { }
        public SpecificException(string msg) : base(msg) { }
    }
}
