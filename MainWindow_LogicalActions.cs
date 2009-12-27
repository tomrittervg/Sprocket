using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
namespace Sprocket
{
    public partial class MainWindow : Window
    {
        private void loadProc(object sender, RoutedEventArgs e)
        {
            NoSPLoadedRow.Height = new GridLength(0);
            SPLoadedRow.Height = new GridLength(0);

            try
            {
                var data = SQL.Queries.GetStoredProcParameters(serverName.Text, database.Text, procName.Text);
                spParameters.ItemsSource = data;
                CurentContext.LoadParameters(data);
                SPLoadedRow.Height = new GridLength();
            }
            catch (SqlException ex)
            {
                noParamsStatusMessage.Content = "Procedure Load Failed: \n" + ex.Message;
                NoSPLoadedRow.Height = new GridLength();
            }
        }

        private void RunTests(object sender, RoutedEventArgs e)
        {
            CurentContext.RunTests();
        }
    }
}
