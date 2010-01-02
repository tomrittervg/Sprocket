using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sprocket
{
    public static class TestRunner
    {
        public static Action<string> EnableWaitStatus;
        public static Action DisableWaitStatus;

        private const int cTrivialNumberOfTestsLimit = 100;
        private static DiffEngine.IDiffEngine _diffEngine;

        
        static TestRunner()
        {
            _diffEngine = new DiffEngine.TortoiseMerge();
        }

        public static void RunTests(TestContext context)
        {
            TestRunner.EnableWaitStatus("Beginning Tests");

            int? _numTests = context.QueryCombinations;
            if (_numTests < cTrivialNumberOfTestsLimit)
            {
                var filesToRun = WriteTestCasesToFile(context);

                TestRunner.EnableWaitStatus("Running Test Cases");
                var fileResults = RunTestCases(filesToRun, context.Server);

                TestRunner.EnableWaitStatus("Displaying Differences");
                _diffEngine.CompareFiles(fileResults);
            }
            else
            {
                //TODO: Splitting Tests Across Files
                throw new WTFException("This number of tests isn't supported yet");
            }
            
            TestRunner.DisableWaitStatus();
        }

        private static ComparisonPair RunTestCases(ComparisonPair runset, string server)
        {
            var results = new ComparisonPair(runset.Old.ReplaceLast(".txt", ".results.txt"), runset.New.ReplaceLast(".txt", ".results.txt"));

            string oldArguments = string.Format("-S {0} -E -i \"{1}\" -o \"{2}\"", server, runset.Old, results.Old);
            string newArguments = string.Format("-S {0} -E -i \"{1}\" -o \"{2}\"", server, runset.New, results.New);

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

            MainWindow.TemporaryFilesCreated.Add(results.Old);
            MainWindow.TemporaryFilesCreated.Add(results.New);

            return results;
        }

        private static ComparisonPair WriteTestCasesToFile(TestContext context)
        {
            return WriteTestCasesToFile(context, 0, -1);
        }
        private static ComparisonPair WriteTestCasesToFile(TestContext context, int offset, int limit)
        {
            if (offset != 0 || limit != -1) throw new WTFException("Splitting Tests Across Files isn't really implemented yet...");

            string filenameOld = "", filenameNew = "";

            var root = Path.GetTempFileName();
            MainWindow.TemporaryFilesCreated.Add(root);
            root = root.ReplaceLast(".tmp", "");
            filenameOld = root + ".old.txt";
            filenameNew = root + ".new.txt";
            MainWindow.TemporaryFilesCreated.Add(filenameOld);
            MainWindow.TemporaryFilesCreated.Add(filenameNew);

            var testCases = context.TestCases;

            var oldHndl = new StreamWriter(filenameOld, false);
            var newHndl = new StreamWriter(filenameNew, false);
            oldHndl.WriteLine("Use " + context.Database + ";");
            newHndl.WriteLine("Use " + context.Database + ";");

            for (int i = 0; i < testCases.Count; i++)
            {
                if (i < offset) continue;
                if (i > limit && limit > 0) break;

                var paramString = testCases[i].GetParameterString();
                var paramStringEscaped = paramString.Replace("'", "''");

                oldHndl.WriteLine("print '" + paramStringEscaped + "'");
                newHndl.WriteLine("print '" + paramStringEscaped + "'");

                oldHndl.WriteLine(string.Format("exec {0} {1}", context.ComparisonProc, paramString));
                newHndl.WriteLine(string.Format("exec {0} {1}", context.StoredProcedure, paramString));
            }
            oldHndl.Close();
            newHndl.Close();

            return new ComparisonPair(filenameOld, filenameNew);
        }

        /// <summary>We need to split the number of proc executions per file, otherwise the files will be too large for merge programs to easily diff them</summary>
        private static int GetNumTestsPerFile()
        {
            //TODO: Run a random sample of procs and guess the size of each test case.  Then see how many test cases we can fit in a 1 or 2 meg file, and only
            //  put that many test cases per fileset
            return 1000;
        }
    }
}
