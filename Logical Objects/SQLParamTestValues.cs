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

                _csvValues = new List<string>();
                _csvValues.AddRange(_csv.Split(','));
                _csvValues.ConvertAll<string>(x => x.Trim());

                ChangeProperty("CSV");
            }
        }
        #endregion
        private List<string> _csvValues { get; set; }

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
        public void SetQueryResults(List<string> results)
        {
            StoreCurrentTestValue();
            if (TestType != SQLParamTestType.Query) throw new WTFException();
            _queryValues = results;
            ChangeProperty("TestValues");
        }

        //==================================================================================================================

        public int QueryCombinations
        {
            get
            {
                if (this.TestType == SQLParamTestType.CSV)
                    if (this._csvValues == null) return 0;
                    else return this._csvValues.Count;
                else if (this.TestType == SQLParamTestType.Query)
                    if (this._queryValues == null) return 0;
                    else return this._queryValues.Count;
                else
                    return 0;
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
                    testTypeValid = (_csvValues != null) && (_csvValues.Count > 0);
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

        public List<string> TestValues
        {
            get
            {
                if (TestType == SQLParamTestType.Query)
                    return _queryValues;
                else if (TestType == SQLParamTestType.CSV)
                    return _csvValues;
                else
                    throw new WTFException();
            }
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
