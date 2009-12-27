using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket.DiffEngine
{
    public interface IDiffEngine
    {
        void ShowDiffWindowFiles(string oldFileName, string newFileName);
        void ShowDiffWindowStrings(string oldstring, string newstring);
    }
}
