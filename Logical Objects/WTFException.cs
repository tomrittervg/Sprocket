using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    //This is what I throw when a code assumption I've made (e.g. sequence of events or number of radio buttons I'm excepting to exist)
    //  is wrong/has changed.  I usually don't bother with a message because it's immediately obvious what's wrong.
    //I also will add checks than throw these exceptions that could be Debug.Asserts.  Except those don't got into Release Builds, and
    //  if the statement is true, all hell has risen on Earth and the program should crash before it can do bad things.
    public class WTFException : Exception
    {
        public WTFException() : base() { }
        public WTFException(string msg) : base(msg) { }
    }
}
