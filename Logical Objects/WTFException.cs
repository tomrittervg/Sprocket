using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    //This is what I throw when a code assumption I've made (e.g. sequence of events or number of radio buttons I'm excepting to exist)
    //  is wrong/has changed.  I don't bother with a message because it's immediately obvious what's wrong.
    public class WTFException : Exception
    {
    }
}
