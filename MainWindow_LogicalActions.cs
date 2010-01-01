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
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                CurentContext.RunTests();
            });
        }

        protected void ValidatePhysicalProcFile(object data)
        {
            try
            {
                var newName = SQL.Queries.TurnFileIntoProcedure(CurentContext.OriginalProcFilename, CurentContext.Server, CurentContext.Database);
                var newProcParams = SQL.Queries.GetStoredProcParameters(CurentContext.Server, CurentContext.Database, newName);

                if (!CurentContext.ParameterValues.ParametersMatch(newProcParams))
                    throw new SpecificException(SpecificException.ProcParametersDontMatch);

                CurentContext.ComparisonProc = newName;
                CurentContext.ComparisonProcValid = true;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    originalProcLocation_PhysicalFile_statusImage.Source = GoodBMP;
                    originalProcLocation_PhysicalFile_statusImage.MouseUp -= new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);
                }));
            }
            catch (SpecificException ex)
            {
                if (ex.Message.OneOf(SpecificException.ProcParametersDontMatch, SpecificException.InputCouldNotBeParsed))
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        originalProcLocation_PhysicalFile_statusImage.Source = BadBMP;
                        originalProcLocation_PhysicalFile_statusImage.Tag = ex.Message;
                        originalProcLocation_PhysicalFile_statusImage.MouseUp += new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);
                    }));
                    CurentContext.ComparisonProc = string.Empty;
                    CurentContext.ComparisonProcValid = false;
                }
                else
                    throw;
            }
        }

        void displayErrorMessage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show((sender as FrameworkElement).Tag.ToString(), "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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
