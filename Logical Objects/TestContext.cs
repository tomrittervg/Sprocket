using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sprocket
{
    public enum OriginalProcLocations
    {
        Unset = 0,
        PhysicalFile = 1,
        AnotherProc = 2
    }

    [Serializable]
    public partial class TestContext : INotifyPropertyChanged
    {
        public TestContext()
        {
            OriginalProcLocation = OriginalProcLocations.Unset;
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
            this.SetUpOriginalProc();

            var runner = new TestRunner(this);
            runner.RunTests();
        }
        //==================================================================================================================

        public int QueryCombinations
        {
            get
            {
                return this.ParameterValues.QueryCombinations();
            }
        }

        public bool IsValidContext
        {
            get
            {
                bool originalProcValid = false;

                if (OriginalProcLocation == OriginalProcLocations.Unset)
                    originalProcValid = false;
                else if (OriginalProcLocation == OriginalProcLocations.AnotherProc)
                    originalProcValid = !this.ComparisonProc.IsNullOrEmpty();
                else if (OriginalProcLocation == OriginalProcLocations.PhysicalFile)
                    originalProcValid = !this.OriginalProcFilename.IsNullOrEmpty();
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

        private List<TestCase> _testCases;
        public List<TestCase> TestCases
        {
            get
            {
                if (_testCases == null) 
                    _testCases = IronPython.GetTestCases(this.ParameterValues);
                return _testCases;
            }
        }

        //==================================================================================================================
        private void SetUpOriginalProc()
        {
            if (OriginalProcLocation == OriginalProcLocations.PhysicalFile)
            {
                var newName = SQL.Queries.TurnFileIntoProcedure(this.OriginalProcFilename, this.Server, this.Database);
                var newProcParams = SQL.Queries.GetStoredProcParameters(this.Server, this.Database, newName);

                if (!this.ParameterValues.ParametersMatch(newProcParams))
                    throw new Exception("Proc Parameters Don't Match");

                this.ComparisonProc = newName;
            }
            else if (OriginalProcLocation == OriginalProcLocations.AnotherProc)
            {
                var newProcParams = SQL.Queries.GetStoredProcParameters(this.Server, this.Database, this.ComparisonProc);

                if (!this.ParameterValues.ParametersMatch(newProcParams))
                    throw new Exception("Proc Parameters Don't Match");
            }
            else
                throw new WTFException();
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
            _testCases = null;
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
