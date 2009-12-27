using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket.DiffEngine
{
    public interface IDiffEngine
    {
        void ShowDiffWindowFiles(ComparisonPair compareFiles);
        void ShowDiffWindowStrings(ComparisonPair compareStrings);
    }
}
