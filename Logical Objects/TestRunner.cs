using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public class TestRunner
    {
        private const int cTrivialNumberOfTestsLimit = 100;
        private int? _numTests;

        private TestContext Context { get; set; }
        public TestRunner(TestContext context)
        {
            DiffEngine.IDiffEngine diffEngine = new DiffEngine.TortoiseMerge();
            Context = context;
        }

        public void RunTests()
        {
            _numTests = Context.QueryCombinations;
            if (_numTests < cTrivialNumberOfTestsLimit)
            {
                var file = this.WriteTestCasesToFile();
            }
            else
            {
                int testsPerFile = this.GetNumTestsPerFile();
            }
        }

        private ComparisonPair WriteTestCasesToFile()
        {
            return WriteTestCasesToFile(0, -1);
        }
        private ComparisonPair WriteTestCasesToFile(int offset, int limit)
        {
            string filenameOld = "", filenameNew = "";

            var testCases = Context.TestCases;
            if (testCases.Count != _numTests) throw new WTFException();

            for (int i = 0; i < _numTests.Value; i++)
            {
                if (i < offset) continue;
                if (i > limit && limit > 0) break;


            }

            return new ComparisonPair(filenameOld, filenameNew);
        }

        /// <summary>We need to split the number of proc executions per file, otherwise the files will be too large for merge programs to easily diff them</summary>
        private int GetNumTestsPerFile()
        {
            return 1000;
        }
    }
}
