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
            if (orderedParameters.Count != orderedValues.Count) throw new WTFException();

            values = new Dictionary<string, string>();
            Parameters = new List<SQLParam>();

            for (int i = 0; i < orderedParameters.Count; i++)
            {
                Parameters.Add(orderedParameters[i].Parameter);
                values.Add(orderedParameters[i].Parameter.Name, orderedValues[i]);
            }
        }

        public List<SQLParam> Parameters { get; protected set; }

        public string GetEscapedValue(SQLParam key)
        {
            var val = values[key.Name];
            
            if (key.Type == "varchar")
                return "'" + val + "'";
            else
                return val;
        }
    }
}
