using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Sprocket
{
    /*
     * Right about here, you're probably confused as hell and wondering why anyone in their right mind would embed dynamic python code. 
     * Well there's a few reasons:
     *  1) This is my project and I can do what I want to
     *  2) This is ultimately for fun and learning
     *  3) Python has a very nice function I wasn't to call
     *  
     * The problem I'm trying to solve here is, given a List of Lists, how to get all the combinations of values from the inner lists.
     * It's easily solved for a fixed length number of lists - it's just nested for loops.  But for a dynamic number of lists it's trickier.
     * Especially because I needed to be able to iterate over the list non-sequentially between function calls (so a yield return would
     * be messy requiring me to pass around an iterator places).  My point is, it'd be possibly, but tricky/complicated/ugly.  This
     * is those things but also fun, so it wins.
     * 
     * Python has a module for it: itertools.product
     * So I wrote a litter wrapper function to massage types into what I want, and then call that.
     * 
     * By the way, this embedding stuff without typesafety is horrible.  I *wanted* to just have another project in the solution 
     *   written in IronPython, that I referenced from the Main Project (like you can do between C# and VB.Net).  But it seems that's not 
     *   possible.  That really needs to happen to get the dynamic languages up to first-class-citizen status.
     */ 

    public partial class TestContext
    {
        private static class IronPython
        {
            private static ScriptEngine pythonEngine;
            private static ScriptScope pythonScope;

            private static Func<List<List<string>>, List<List<string>>> Listisize;

            static IronPython()
            {
                pythonEngine = Python.CreateEngine();
                pythonScope = pythonEngine.CreateScope();

                var src = pythonEngine.CreateScriptSourceFromString(listisizeCode, SourceCodeKind.Statements);
                src.Execute(pythonScope);

                Listisize = pythonScope.GetVariable<Func<List<List<string>>, List<List<string>>>>("listisize");
            }

            const string listisizeCode = @"
from System.Collections.Generic import *
import itertools
def listisize(l):
    ret = List[List[str]]()
    for i in itertools.product(*l):
        ret.Add(List[str]())
        for j in i:
            ret[ret.Count-1].Add(j)
    return ret";

            public static List<TestCase> GetTestCases(List<SQLParamTestValues> paramValues)
            {
                //One of the tricky/subtle/dangerous things about this function is that it assumes the order of the parameters
                // in the ilsts returned from Listisize correspond to the order of the lists given.  If that contract is broken,
                // all sorts of subtle things will blow up majorly.

                var data = new List<List<string>>();
                for (int i = 0; i < paramValues.Count; i++)
                    data.Add(paramValues[i].TestValues);

                var result = Listisize(data);

                var testCases = new List<TestCase>();
                for (int i = 0; i < result.Count; i++)
                    testCases.Add(new TestCase(result[i], paramValues));

                return testCases;
            }
        }
    }
}
