using System;
using System.IO;
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
        private void LoadProcedure(object sender, RoutedEventArgs e)
        {
            //TODO: Is there really no better way of show/hiding rows than by futzing with their height?
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
            //this.EnableWaitStatus();
            CurentContext.RunTests();
        }

        private void MainWin_Closed(object sender, EventArgs e)
        {
            //TODO: Track server and database changes, because this will leave messes behind if you change servers and databases.
            //As this is a cleanup, closing window function, we swallow any cleanup that could not be done, and try to continue.
            try
            {
                SQL.Queries.DeleteProcsBeginningWith("sprockettestrun" + "_" + (MainWindow.CurrentProcess.Id | MainWindow.CurrentProcess.MachineName.GetHashCode()).ToString(),
                    CurentContext.Server, CurentContext.Database);
            }
            catch (Exception ex) { }
            try
            {
                for (int i = 0; i < TemporaryFilesCreated.Count; i++)
                    File.Delete(TemporaryFilesCreated[i]);
            }
            catch (Exception ex) { }
        }
    }
}
