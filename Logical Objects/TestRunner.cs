using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public class TestRunner
    {
        public TestRunner(TestContext context)
        {
            DiffEngine.IDiffEngine diffEngine = new DiffEngine.TortoiseMerge();
        }
    }
}
