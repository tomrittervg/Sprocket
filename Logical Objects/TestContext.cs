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
    public class TestContext : INotifyPropertyChanged
    {
        public TestContext()
        {
            OriginalProcLocation = OriginalProcLocations.Unset;
        }

        #region Server
        private string _server;
        public string Server
        {
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
        public string _storedProcedure { get; set; }
        public string StoredProcedure
        {
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
        public OriginalProcLocations _originalProcLocation { get; set; }
        public OriginalProcLocations OriginalProcLocation
        {
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
        public string _originalProcFilename { get; set; }
        public string OriginalProcFilename
        {
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
        public string _comparisonProc { get; set; }
        public string ComparisonProc
        {
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

        public bool IsValidContext
        {
            get
            {
                return
                    this.OriginalProcLocation != OriginalProcLocations.Unset &&
                    !string.IsNullOrEmpty(this.Server) &&
                    !string.IsNullOrEmpty(this.Database) &&
                    !string.IsNullOrEmpty(this.StoredProcedure) &&
                    (
                        (
                            this.OriginalProcLocation == OriginalProcLocations.PhysicalFile &&
                            !string.IsNullOrEmpty(this.OriginalProcFilename)
                        )
                        ||
                        (
                            this.OriginalProcLocation == OriginalProcLocations.AnotherProc &&
                            !string.IsNullOrEmpty(this.ComparisonProc)
                        )
                    ) &&
                    ParameterValues != null &&
                    ParameterValues.All(x => x.IsValidTestValue);
            }
        }

        public void LoadParameters(List<SQLParam> data)
        {
            ParameterValues = new List<SQLParamTestValues>(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                ParameterValues.Add(new SQLParamTestValues(data[i]));
                ParameterValues[i].PropertyChanged += new PropertyChangedEventHandler(SQLParameterPropertyChanged);
            }
        }

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
