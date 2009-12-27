using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sprocket
{
    public class TestRunner
    {
        private const int cTrivialNumberOfTestsLimit = 100;
        private int? _numTests;
        private DiffEngine.IDiffEngine _diffEngine;

        private TestContext Context { get; set; }
        public TestRunner(TestContext context)
        {
            _diffEngine = new DiffEngine.TortoiseMerge();
            Context = context;
        }

        public void RunTests()
        {
            _numTests = Context.QueryCombinations;
            if (_numTests < cTrivialNumberOfTestsLimit)
            {
                var filesToRun = this.WriteTestCasesToFile();
                var fileResults = this.RunTestCases(filesToRun);
                _diffEngine.ShowDiffWindowFiles(fileResults);
            }
            else
            {
                int testsPerFile = this.GetNumTestsPerFile();
            }
        }

        private ComparisonPair RunTestCases(ComparisonPair runset)
        {
            var results = new ComparisonPair(runset.Old.ReplaceLast(".txt", ".results.txt"), runset.New.ReplaceLast(".txt", ".results.txt"));

            string oldArguments = string.Format("-S {0} -E -i \"{1}\" -o \"{2}\"", Context.Server, runset.Old, results.Old);
            string newArguments = string.Format("-S {0} -E -i \"{1}\" -o \"{2}\"", Context.Server, runset.New, results.New);

            
            ProcessStartInfo startInfo = new ProcessStartInfo("sqlcmd", oldArguments) { UseShellExecute = true };
            Process job = new Process() { StartInfo = startInfo };
            job.Start();
            job.WaitForExit();
            job.Close();

            startInfo.Arguments = newArguments;
            job = new Process() { StartInfo = startInfo };
            job.Start();
            job.WaitForExit();
            job.Close();

            return results;
        }

        private ComparisonPair WriteTestCasesToFile()
        {
            return WriteTestCasesToFile(0, -1);
        }
        private ComparisonPair WriteTestCasesToFile(int offset, int limit)
        {
            string filenameOld = "", filenameNew = "";

            var root = Path.GetTempFileName().ReplaceLast(".tmp", "");
            filenameOld = root + ".old.txt";
            filenameNew = root + ".new.txt";
            MainWindow.TemporaryFilesCreated.Add(filenameOld);
            MainWindow.TemporaryFilesCreated.Add(filenameNew);

            var testCases = Context.TestCases;
            if (testCases.Count != _numTests) throw new WTFException();

            var oldHndl = new StreamWriter(filenameOld, false);
            var newHndl = new StreamWriter(filenameNew, false);
            oldHndl.WriteLine("Use " + Context.Database + ";");
            newHndl.WriteLine("Use " + Context.Database + ";");

            for (int i = 0; i < _numTests.Value; i++)
            {
                if (i < offset) continue;
                if (i > limit && limit > 0) break;

                var paramString = "";
                foreach (var p in testCases[i].Parameters)
                    paramString += " " + p.Name + "=" + testCases[i].GetEscapedValue(p) + ",";
                paramString = paramString.TrimEnd(',');

                oldHndl.WriteLine("print '" + paramString.Replace("'", "''") + "'");
                newHndl.WriteLine("print '" + paramString.Replace("'", "''") + "'");

                oldHndl.WriteLine(string.Format("exec {0} {1}", Context.ComparisonProc, paramString));
                newHndl.WriteLine(string.Format("exec {0} {1}", Context.StoredProcedure, paramString));
            }
            oldHndl.Close();
            newHndl.Close();

            return new ComparisonPair(filenameOld, filenameNew);
        }

        /// <summary>We need to split the number of proc executions per file, otherwise the files will be too large for merge programs to easily diff them</summary>
        private int GetNumTestsPerFile()
        {
            return 1000;
        }
    }
}
