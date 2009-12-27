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
        ConstantValue = 1,
        CSV = 2,
        Query = 3
    }

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

        #region ConstantValue
        private string _constantValue;
        public string ConstantValue
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _constantValue; }
            set
            {
                StoreCurrentTestValue();
                _constantValue = value;
                ChangeProperty("ConstantValue");
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
                _csv = value;
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
                StoreCurrentTestValue(); _query = value;
                ChangeProperty("Query");
            }
        }
        #endregion

        public int QueryCombinations
        {
            get
            {
                if (this.TestType == SQLParamTestType.ConstantValue)
                    return 1;
                else if (this.TestType == SQLParamTestType.CSV)
                    return this.CSV.CountOf(',');
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
                else if (this.TestType == SQLParamTestType.ConstantValue)
                    testTypeValid = !this.ConstantValue.IsNullOrEmpty();
                else if (this.TestType == SQLParamTestType.CSV)
                    testTypeValid = !string.IsNullOrEmpty(this.CSV);
                else
                    throw new WTFException();

                return
                    !this.Parameter.Name.IsNullOrEmpty() &&
                    testTypeValid;
            }
        }

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
