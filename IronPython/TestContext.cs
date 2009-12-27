using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Sprocket
{
    public partial class TestContext
    {
        private static class IronPython
        {
            private static ScriptEngine pythonEngine;
            private static ScriptScope pythonScope;

            static IronPython()
            {
                pythonEngine = Python.CreateEngine();
                pythonScope = pythonEngine.CreateScope();
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
                var src = pythonEngine.CreateScriptSourceFromString(listisizeCode, SourceCodeKind.Statements);
                src.Execute(pythonScope);

                var data = new List<List<string>>();
                for (int i = 0; i < paramValues.Count; i++)
                    data.Add(paramValues[i].TestValues);

                var listisize = pythonScope.GetVariable<Func<List<List<string>>, List<List<string>>>>("listisize");
                var result = listisize(data);

                var testCases = new List<TestCase>();
                for (int i = 0; i < result.Count; i++)
                    testCases.Add(new TestCase(result[i], paramValues));

                return testCases;
            }
        }
    }
}
