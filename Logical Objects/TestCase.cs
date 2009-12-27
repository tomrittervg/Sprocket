using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public class TestCase
    {
        private Dictionary<string, string> values;

        public TestCase(List<string> orderedValues, List<SQLParamTestValues> orderedParameters)
        {
            if(orderedParameters.Count != orderedValues.Count) throw new WTFException();

            values = new Dictionary<string, string>();

            for (int i = 0; i < orderedParameters.Count; i++)
                values.Add(orderedParameters[i].Parameter.Name, orderedValues[i]);
        }

        public IEnumerable<string> AllParameters
        {
            get
            {
                return values.Keys;
            }
        }

        public string this[string key]
        {
            get
            {
                return values[key];
            }
        }
    }
}
