using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sprocket
{
    /// <summary>Where the original Stored Procedure (before editing) we are testing is located</summary>
    public enum OriginalProcLocations
    {
        Unset = 0,
        PhysicalFile = 1,
        AnotherProc = 2
    }

    /// <summary>All the data necessary to run a regression test against a stored procedure</summary>
    public partial class TestContext : INotifyPropertyChanged
    {
        public TestContext()
        {
            OriginalProcLocation = OriginalProcLocations.Unset;
            ComparisonProcValid = false;
        }

        #region Server
        private string _server;
        public string Server
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _server; }
            set
            {
                StoreCurrentTestValue();
                _server = value;
                ChangeProperty("server");
            }
        }
        #endregion
        #region Database
        private string _database;
        public string Database
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _database; }
            set
            {
                StoreCurrentTestValue();
                _database = value;
                ChangeProperty("Database");
            }
        }
        #endregion

        #region Stored Procedure
        private string _storedProcedure { get; set; }
        /// <summary>The Stored Procedure we are testing</summary>
        public string StoredProcedure
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _storedProcedure; }
            set
            {
                StoreCurrentTestValue();
                _storedProcedure = value;
                ChangeProperty("StoredProcedure");
            }
        }
        #endregion

        #region OriginalProcLocation
        private OriginalProcLocations _originalProcLocation { get; set; }
        /// <summary>Where the original Stored Procedure (before editing) is located</summary>
        public OriginalProcLocations OriginalProcLocation
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _originalProcLocation; }
            set
            {
                StoreCurrentTestValue();
                _originalProcLocation = value;
                ChangeProperty("OriginalProcLocation");
            }
        }
        #endregion

        #region OriginalProcFilename
        private string _originalProcFilename { get; set; }
        /// <summary>The filename of the Original Proc.  Used when OriginalProcLocation is set to PhysicalFile</summary>
        public string OriginalProcFilename
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _originalProcFilename; }
            set
            {
                StoreCurrentTestValue();
                _originalProcFilename = value;
                ChangeProperty("OriginalProcFilename");
            }
        }
        #endregion
        #region ComparisonProc
        private string _comparisonProc { get; set; }
        /// <summary>The procedure we will compare StoredProcedure to</summary>
        public string ComparisonProc
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProc; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProc = value;
                ChangeProperty("ComparisonProc");
            }
        }
        #endregion
        #region ComparisonProcValid
        private bool _comparisonProcValid { get; set; }
        /// <summary>The procedure we will compare StoredProcedure to</summary>
        public bool ComparisonProcValid
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProcValid; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProcValid = value;
                ChangeProperty("ComparisonProcValid");
            }
        }
        #endregion

        /// <summary>The List of parameters to this stored procedure, and how they will be tested</summary>
        public List<SQLParamTestValues> ParameterValues { get; protected set; }

        //==================================================================================================================
        public void LoadParameters(List<SQLParam> data)
        {
            ParameterValues = new List<SQLParamTestValues>(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                ParameterValues.Add(new SQLParamTestValues(data[i]));
                ParameterValues[i].PropertyChanged += new PropertyChangedEventHandler(SQLParameterPropertyChanged);
            }
        }

        public void RunTests()
        {
            var runner = new TestRunner(this);
            runner.RunTests();
        }
        //==================================================================================================================
        /// <summary>The number of test cases in this TestContext, based on the values of the parameters</summary>
        public int QueryCombinations
        {
            get
            {
                return this.ParameterValues.QueryCombinations();
            }
        }
        /// <summary>If the context is complete, and able to have its tests run</summary>
        public bool IsValidContext
        {
            get
            {
                bool originalProcValid = false;

                if (OriginalProcLocation == OriginalProcLocations.Unset)
                    originalProcValid = false;
                else if (OriginalProcLocation == OriginalProcLocations.AnotherProc)
                    originalProcValid = ComparisonProcValid;
                else if (OriginalProcLocation == OriginalProcLocations.PhysicalFile)
                    originalProcValid = ComparisonProcValid;
                else
                    throw new WTFException();

                return
                    !this.Server.IsNullOrEmpty() &&
                    !this.Database.IsNullOrEmpty() &&
                    !this.StoredProcedure.IsNullOrEmpty() &&
                    originalProcValid &&
                    ParameterValues != null &&
                    ParameterValues.All(x => x.IsValidTestValue);
            }
        }

        /// <summary>FYI: This is an expensive function</summary>
        public List<TestCase> TestCases
        {
            get
            {
                if (!IsValidContext) throw new WTFException("Cannot request TestCases if you don't have a valid Context");
                return IronPython.GetTestCases(this.ParameterValues);
            }
        }

        //==================================================================================================================
        #region INotifyPropertyChanged Stuff
        private void SQLParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValidTestValue")
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsValidContext"));
        }

        void StoreCurrentTestValue()
        {
            if (_isValidContextBeforeChange != null) throw new WTFException();
            _isValidContextBeforeChange = this.IsValidContext;
        }
        private bool? _isValidContextBeforeChange;
        private void ChangeProperty(string property)
        {
            if (_isValidContextBeforeChange == null) throw new WTFException();
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
                if (_isValidContextBeforeChange != this.IsValidContext)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsValidContext"));
            }
            _isValidContextBeforeChange = null;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
