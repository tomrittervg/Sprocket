using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace Sprocket
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TestContext CurentContext { get; set; }
        public static Random rndm = new Random();
        public static System.Diagnostics.Process CurrentProcess = System.Diagnostics.Process.GetCurrentProcess();

        public MainWindow()
        {
            CurentContext = new TestContext();
            InitializeComponent();
        }

        private void originalProcLocation_PhysicalFile_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".sql";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                originalProcLocation_PhysicalFile_Name.Content = ofd.FileName;
                CurentContext.OriginalProcFilename = ofd.FileName;
            }
        }

        private void ReloadContextFromGUI(object sender, RoutedEventArgs e)
        {
            CurentContext.Server = serverName.Text;
            CurentContext.Database = database.Text;
            CurentContext.StoredProcedure = procName.Text;

            if (originalProcLocation_AnotherProc.IsChecked == true) CurentContext.OriginalProcLocation = OriginalProcLocations.AnotherProc;
            else if (originalProcLocation_PhysicalFile.IsChecked == true) CurentContext.OriginalProcLocation = OriginalProcLocations.PhysicalFile;

            if (e.OriginalSource is System.Windows.Controls.RadioButton)
            {
                var radio = e.OriginalSource as System.Windows.Controls.RadioButton;
                if (radio.GroupName[0] == '@')
                {
                    SQLParamTestType testtype;

                    if (radio.Name == "paramNameSource_QueryResults") testtype = SQLParamTestType.Query;
                    else if (radio.Name == "paramNameSource_CSV") testtype = SQLParamTestType.CSV;
                    else if (radio.Name == "paramNameSource_ConstantValue") testtype = SQLParamTestType.ConstantValue;
                    else throw new WTFException();

                    CurentContext.ParameterValues.Find(x => x.Parameter.Name == radio.GroupName).TestType = testtype;
                }
            }
        }

        private void paramNameSource_ConstantValue_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var txtBox = sender as TextBox;
            CurentContext.ParameterValues.Find(x => x.Parameter.Name == txtBox.Tag.ToString()).ConstantValue = txtBox.Text;
        }

        private void MainWin_Closed(object sender, EventArgs e)
        {
            //delete all stored procs that start with "sprockettestrun" + "_" + (MainWindow.CurrentProcess.Id | MainWindow.CurrentProcess.MachineName.GetHashCode()).ToString()
        }
    }
}
