using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Threading;

namespace Sprocket
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Random rndm = new Random();
        public static System.Diagnostics.Process CurrentProcess = System.Diagnostics.Process.GetCurrentProcess();
        public static List<string> TemporaryFilesCreated = new List<string>();

        public TestContext CurrentContext { get; set; }
        

        public MainWindow()
        {
            CurrentContext = new TestContext();
            TestRunner.EnableWaitStatus = delegate(string msg)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.EnableWaitStatus(msg);
                }));
            };
            TestRunner.DisableWaitStatus = delegate()
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.DisableWaitStatus();
                }));
            };

            InitializeComponent();
            InitializeImageResources();
            DisableWaitStatus();
        }

        private void originalProcLocation_PhysicalFile_Button_Click(object sender, RoutedEventArgs e)
        {
            //TODO: DefaultExt isn't working, and this dialog could be improved.
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".sql";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                originalProcLocation_PhysicalFile_Name.Content = ofd.FileName;
                
                CurrentContext.ComparisonProcFilename = ofd.FileName;
                CurrentContext.ComparisonProcValid = false;
                
                originalProcLocation_PhysicalFile_statusImage.Source = WaitBMP;
                
                ThreadPool.QueueUserWorkItem(ValidatePhysicalProcFile);
            }
        }

        private void paramNameSource_QueryResults_editButton_Click(object sender, RoutedEventArgs e)
        {
            WriteMiniQueryWindow queryWin = new WriteMiniQueryWindow();
            queryWin.mainWindowReference = this;
            queryWin.ParamName = (sender as Button).Tag.ToString(); ;
            queryWin.PreviousQuery = CurrentContext.ParameterValues.Find(x => x.Parameter.Name == queryWin.ParamName).Query;
            queryWin.ShowDialog();
        }

        private void ReloadContextFromGUI(object sender, RoutedEventArgs e)
        {
            //TODO: This function could be improved somehow to not reload everything when only one thing is changed... I think
            CurrentContext.Server = serverName.Text;
            CurrentContext.Database = database.Text;
            CurrentContext.StoredProcedure = procName.Text;

            if (originalProcLocation_AnotherProc.IsChecked == true) CurrentContext.OriginalProcLocation = OriginalProcLocations.AnotherProc;
            else if (originalProcLocation_PhysicalFile.IsChecked == true) CurrentContext.OriginalProcLocation = OriginalProcLocations.PhysicalFile;

            if (e.OriginalSource is System.Windows.Controls.RadioButton)
            {
                var radio = e.OriginalSource as System.Windows.Controls.RadioButton;
                if (radio.GroupName[0] == '@')
                {
                    SQLParamTestType testtype;

                    if (radio.Name == "paramNameSource_CSV") testtype = SQLParamTestType.CSV;
                    else if (radio.Name == "paramNameSource_QueryResults") testtype = SQLParamTestType.Query;
                    else throw new WTFException();

                    CurrentContext.ParameterValues.Find(x => x.Parameter.Name == radio.GroupName).TestType = testtype;
                }
            }
        }

        private void paramNameSource_CSV_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var txtBox = sender as TextBox;
            var paramTestValue = CurrentContext.ParameterValues.Find(x => x.Parameter.Name == txtBox.Tag.ToString());

            if (txtBox.Name == "paramNameSource_CSV_value") paramTestValue.CSV = txtBox.Text;
            else throw new WTFException();
        }
    }
}
