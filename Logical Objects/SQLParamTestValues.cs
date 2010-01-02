using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sprocket
{
    public enum SQLParamTestType
    {
        Unset = 0,
        CSV = 1,
        Query = 2
    }

    /// <summary>A SQLParameter and all the values it will be tested with</summary>
    public class SQLParamTestValues : INotifyPropertyChanged
    {
        public SQLParamTestValues(SQLParam param)
        {
            Parameter = param;
            TestType = SQLParamTestType.Unset;
        }

        public SQLParam Parameter { get; set; }

        #region TestType
        private SQLParamTestType _testType;
        public SQLParamTestType TestType
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _testType; }
            set
            {
                StoreCurrentTestValue();
                _testType = value;
                ChangeProperty("TestType");
            }
        }
        #endregion

        #region CSV
        private string _csv;
        public string CSV
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _csv; }
            set
            {
                StoreCurrentTestValue();
                _csv = value.Replace(",,", ",").TrimEnd(',', ' ').Trim();
                ChangeProperty("CSV");
            }
        }
        #endregion
        #region Query
        private string _query;
        public string Query
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _query; }
            set
            {
                StoreCurrentTestValue(); 
                _query = value;
                ChangeProperty("Query");
            }
        }
        #endregion
        private List<string> _queryValues { get; set; }

        //==================================================================================================================

        public int QueryCombinations
        {
            get
            {
                if (this.TestType == SQLParamTestType.CSV)
                    return this.CSV.CountOf(',') + 1;
                else if (this.TestType == SQLParamTestType.Query)
                    return this._queryValues.Count();
                else
                    throw new WTFException();
            }
        }

        public bool IsValidTestValue
        {
            get
            {
                bool testTypeValid = false;
                if (this.TestType == SQLParamTestType.Unset)
                    testTypeValid = false;
                else if (this.TestType == SQLParamTestType.CSV)
                    testTypeValid = !string.IsNullOrEmpty(this.CSV);
                else if (this.TestType == SQLParamTestType.Query)
                    testTypeValid = (_queryValues != null) && (_queryValues.Count > 0);
                else
                    throw new WTFException();

                return
                    !this.Parameter.Name.IsNullOrEmpty() &&
                    testTypeValid;
            }
        }

        //==================================================================================================================

        private List<string> _testValues;
        public List<string> TestValues
        {
            get
            {
                if (TestType == SQLParamTestType.Query)
                    return _queryValues;
                else if (_testValues == null)
                    _testValues = this.GetTestValues();
                return _testValues;
            }
            set
            {
                StoreCurrentTestValue();
                if (TestType != SQLParamTestType.Query)
                    throw new WTFException();
                _queryValues = value;
                ChangeProperty("TestValues");
            }
        }

        private List<string> GetTestValues()
        {
            if (TestType == SQLParamTestType.Unset)
                throw new WTFException();
            else if (TestType == SQLParamTestType.CSV)
            {
                var ret = new List<string>();
                ret.AddRange(this.CSV.Split(','));
                ret.ConvertAll<string>(x => x.Trim());
                return ret;
            }
            else if (TestType == SQLParamTestType.Query)
                throw new WTFException("This function shouldn't be called except from TestValues, which shortcircuits this case");
            else
                throw new WTFException();
        }

        //==================================================================================================================

        #region INotifyPropertyChanged Stuff
        private void StoreCurrentTestValue()
        {
            if (_isValidTestValueBeforeChange != null) throw new WTFException();
            _isValidTestValueBeforeChange = this.IsValidTestValue;
        }
        private bool? _isValidTestValueBeforeChange;
        private void ChangeProperty(string property)
        {
            if(property != "TestValues") _testValues = null;
            if (_isValidTestValueBeforeChange == null) throw new WTFException();
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
                if (_isValidTestValueBeforeChange != this.IsValidTestValue)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsValidTestValue"));
            }
            _isValidTestValueBeforeChange = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
