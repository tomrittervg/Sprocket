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
        private string _binary { get; set; }

        public TortoiseMerge()
        {
            _binary = FindTortoiseBinary();
        }

        public void ShowDiffWindowFiles(string oldFileName, string newFileName)
        {
            Process job = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(_binary, string.Format("{0} {1}", oldFileName, newFileName));
            startInfo.UseShellExecute = true;
            job.StartInfo = startInfo;
            job.Start();

            job.WaitForExit();
            job.Close();
        }

        public void ShowDiffWindowStrings(string oldstring, string newstring)
        {
            throw new NotImplementedException();

            Process job = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(_binary, string.Format("{0} {1}", oldstring, oldstring));
            startInfo.UseShellExecute = true;
            job.StartInfo = startInfo;
            job.Start();

            job.WaitForExit();
            job.Close();
        }


        private static string FindTortoiseBinary()
        {
            try
            {
                return Registry.LocalMachine.OpenSubKey("Software\\TortoiseSVN").GetValue("TMergePath").ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not Find TortoiseMarge Path from Registry.  Any number things could be incorrect.");
            }
        }
    }
}
