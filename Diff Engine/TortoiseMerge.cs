using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        private static string FindTortoiseBinary()
        {
            try
            {
                return Registry.LocalMachine.OpenSubKey("Software\\TortoiseSVN").GetValue("TMergePath").ToString();
            }
            catch(Exception ex)
            {
                throw new Exception("Could not Find TortoiseMarge Path from Registry.  Any number things could be incorrect.");
            }
        }
    }
}
