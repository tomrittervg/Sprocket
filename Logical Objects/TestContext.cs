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
            ComparisonProc_AnotherProc_Valid = false;
            ComparisonProc_PhysicalFile_Valid = false;
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

        #region ComparisonProc_PhysicalFile_Filename
        private string _comparisonProc_PhysicalFile_Filename { get; set; }
        /// <summary>The filename of the Comparison Proc.  Used when OriginalProcLocation is set to PhysicalFile</summary>
        public string ComparisonProc_PhysicalFile_Filename
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProc_PhysicalFile_Filename; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProc_PhysicalFile_Filename = value;
                ChangeProperty("ComparisonProc_PhysicalFile_Filename");
            }
        }
        #endregion
        #region ComparisonProc_PhysicalFile_ProcName
        private string _comparisonProc_PhysicalFile_ProcName { get; set; }
        /// <summary>The procedure we will compare StoredProcedure to</summary>
        public string ComparisonProc_PhysicalFile_ProcName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProc_PhysicalFile_ProcName; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProc_PhysicalFile_ProcName = value;
                ChangeProperty("ComparisonProc_PhysicalFile_ProcName");
            }
        }
        #endregion
        #region ComparisonProc_PhysicalFile_Valid
        private bool _comparisonProc_PhysicalFile_Valid { get; set; }
        /// <summary>Whether or not the Comparison Proc has matching parameters</summary>
        public bool ComparisonProc_PhysicalFile_Valid
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProc_PhysicalFile_Valid; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProc_PhysicalFile_Valid = value;
                ChangeProperty("ComparisonProc_PhysicalFile_Valid");
            }
        }
        #endregion

        #region ComparisonProc_AnotherProc_ProcName
        private string _comparisonProc_AnotherProc_ProcName { get; set; }
        /// <summary>The procedure we will compare StoredProcedure to</summary>
        public string ComparisonProc_AnotherProc_ProcName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProc_AnotherProc_ProcName; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProc_AnotherProc_ProcName = value;
                ChangeProperty("ComparisonProc_AnotherProc_ProcName");
            }
        }
        #endregion
        #region ComparisonProc_AnotherProc_Valid
        private bool _comparisonProc_AnotherProc_Valid { get; set; }
        /// <summary>Whether or not the Comparison Proc has matching parameters</summary>
        public bool ComparisonProc_AnotherProc_Valid
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _comparisonProc_AnotherProc_Valid; }
            set
            {
                StoreCurrentTestValue();
                _comparisonProc_AnotherProc_Valid = value;
                ChangeProperty("ComparisonProc_AnotherProc_Valid");
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
            this.OriginalProcLocation = OriginalProcLocations.Unset;
            this.ComparisonProc_AnotherProc_ProcName = "";
            this.ComparisonProc_PhysicalFile_Filename = "";
            this.ComparisonProc_PhysicalFile_ProcName = "";
        }

        public void RunTests()
        {
            TestRunner.RunTests(this);
        }
        //==================================================================================================================
        /// <summary>The number of test cases in this TestContext, based on the values of the parameters</summary>
        public int QueryCombinations
        {
            get
            {
                if (this.ParameterValues == null) return 0;
                return this.ParameterValues.Aggregate<SQLParamTestValues, int, int>(1, (a, x) => a * (x.QueryCombinations == 0 ? 1 : x.QueryCombinations), x => x);
            }
        }

        public string ComparisonProc
        {
            get
            {
                if (OriginalProcLocation == OriginalProcLocations.AnotherProc)
                    return ComparisonProc_AnotherProc_ProcName;
                else if (OriginalProcLocation == OriginalProcLocations.PhysicalFile)
                    return ComparisonProc_PhysicalFile_ProcName;
                else
                    throw new WTFException();
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
                    originalProcValid = ComparisonProc_AnotherProc_Valid;
                else if (OriginalProcLocation == OriginalProcLocations.PhysicalFile)
                    originalProcValid = ComparisonProc_PhysicalFile_Valid;
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
