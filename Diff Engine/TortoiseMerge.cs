using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;

namespace Sprocket.DiffEngine
{
    public class TortoiseMerge : IDiffEngine
    {
        private static string _binary { get; set; }

        static TortoiseMerge()
        {
            _binary = FindTortoiseBinary();
        }

        public void CompareFiles(ComparisonPair compareFiles)
        {
            ShowDiffWindowFiles(compareFiles);
        }

        private static void ShowDiffWindowFiles(ComparisonPair compareFiles)
        {
            //TODO: Multithread the shit out of this app, so stuff like this doesn't lock the main GUI like a newb

            Process job = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(_binary, string.Format("\"{0}\" \"{1}\"", compareFiles.Old, compareFiles.New));
            startInfo.UseShellExecute = true;
            job.StartInfo = startInfo;
            job.Start();

            job.WaitForExit();
            job.Close();
        }

        private static string FindTortoiseBinary()
        {
            //TODO: Need to do some better searching for TortoiseMerge, also need to Handle the error intelligently rather than bombing.
            try
            {
                return Registry.LocalMachine.OpenSubKey("Software\\TortoiseSVN").GetValue("TMergePath").ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not Find TortoiseMarge Path from Registry.  Any number things could be incorrect. Sorry.");
            }
        }
    }
}
