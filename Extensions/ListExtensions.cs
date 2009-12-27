using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public static class ListExtensions
    {
        /// <summary>True if the Parameters in testValues match paramList exactly, False otherwise</summary>
        public static bool ParametersMatch(this List<SQLParamTestValues> testValues, List<SQLParam> paramList)
        {
            for (int i = 0; i < paramList.Count; i++)
                if (!testValues.Any(x => x.Parameter.Equals(paramList[i])))
                    return false;
            return paramList.Count == testValues.Count;
        }

        /// <summary>The Count of query combinations from the given test values</summary>
        public static int QueryCombinations(this List<SQLParamTestValues> testValues)
        {
            return testValues.Aggregate<SQLParamTestValues, int, int>(1, (a, x) => a * x.QueryCombinations, x => x);
        }
    }
}
