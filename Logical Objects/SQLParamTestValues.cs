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
            get { return _query; }
            set
            {
                StoreCurrentTestValue(); _query = value;
                ChangeProperty("Query");
            }
        }
        #endregion

        public bool IsValidTestValue
        {
            get
            {
                return
                    !string.IsNullOrEmpty(this.Parameter.Name) &&
                    this.TestType != SQLParamTestType.Unset &&
                    (
                        (
                            this.TestType == SQLParamTestType.ConstantValue &&
                            !string.IsNullOrEmpty(this.ConstantValue)
                        )
                        ||
                        (
                            this.TestType == SQLParamTestType.CSV &&
                            !string.IsNullOrEmpty(this.CSV)
                        )
                    );
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
