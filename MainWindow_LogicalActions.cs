using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Data.SqlClient;
namespace Sprocket
{
    public partial class MainWindow : Window
    {
        protected void LoadProcedure(object sender, RoutedEventArgs e)
        {
            //TODO: Is there really no better way of show/hiding rows than by futzing with their height?
            NoSPLoadedRow.Height = new GridLength(0);
            SPLoadedRow.Height = new GridLength(0);

            try
            {
                var data = SQL.Queries.GetStoredProcParameters(serverName.Text, database.Text, procName.Text);
                spParameters.ItemsSource = data;
                CurrentContext.LoadParameters(data);
                SPLoadedRow.Height = new GridLength();
            }
            catch (SqlException ex)
            {
                noParamsStatusMessage.Content = "Procedure Load Failed: \n" + ex.Message;
                NoSPLoadedRow.Height = new GridLength();
            }
        }

        protected void RunTests(object sender, RoutedEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                CurrentContext.RunTests();
            });
        }

        public void UpdateQueryFromModal(string paramName, string query)
        {
            var parameter = CurrentContext.ParameterValues.Find(x => x.Parameter.Name == paramName);
            parameter.Query = query.ToString();
            parameter.TestValues = null;

            var queryLabel = spParameters.GetFirstChildControl(x => x.Tag != null && x.Tag.ToString() == paramName && x.Name == "paramNameSource_QueryResults_queryLabel") as TextBlock;
            var queryImage = spParameters.GetFirstChildControl(x => x.Tag != null && x.Tag.ToString() == paramName && x.Name == "paramNameSource_QueryResults_statusImage") as StatusImage;
            queryLabel.Text = parameter.Query.Replace("\r\n", " ");
            queryImage.Source = WaitBMP;

            ThreadPool.QueueUserWorkItem(GetQueryResults, paramName);
        }

        protected void GetQueryResults(object pName)
        {
            string paramName = pName.ToString();
            var parameter = CurrentContext.ParameterValues.Find(x => x.Parameter.Name == paramName);
            Thread.Sleep(1000);//Let the user know some work is being done

            try
            {
                var results = SQL.Queries.GetCollapsedQueryResults(CurrentContext.Server, CurrentContext.Database, parameter.Query);
                parameter.TestValues = results.ToList<string>();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    var queryImage = spParameters.GetFirstChildControl(x => x.Tag != null && x.Tag.ToString() == paramName && x.Name == "paramNameSource_QueryResults_statusImage") as StatusImage;
                    queryImage.Source = GoodBMP;
                    queryImage.MouseUp -= new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);

                    var resultCount = spParameters.GetFirstChildControl(x => x.Tag != null && x.Tag.ToString() == paramName && x.Name == "paramNameSource_QueryResults_resultCount") as Label;
                    resultCount.Content = parameter.TestValues.Count.ToString() + " Results";
                }));
            }
            catch (SqlException ex)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    var queryImage = spParameters.GetFirstChildControl(x => x.Tag != null && x.Tag.ToString() == paramName && x.Name == "paramNameSource_QueryResults_statusImage") as StatusImage;
                    queryImage.Source = BadBMP; ;
                    queryImage.ErrorMessage = ex.Message;
                    queryImage.MouseUp += new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);
                }));
            }
        }

        protected void ValidatePhysicalProcFile(object data)
        {
            Thread.Sleep(1000);//Let the user know some work is being done
            try
            {
                var newName = SQL.Queries.TurnFileIntoProcedure(CurrentContext.OriginalProcFilename, CurrentContext.Server, CurrentContext.Database);
                var newProcParams = SQL.Queries.GetStoredProcParameters(CurrentContext.Server, CurrentContext.Database, newName);

                if (!CurrentContext.ParameterValues.ParametersMatch(newProcParams))
                    throw new SpecificException(SpecificException.ProcParametersDontMatch);

                CurrentContext.ComparisonProc = newName;
                CurrentContext.ComparisonProcValid = true;
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
                        originalProcLocation_PhysicalFile_statusImage.ErrorMessage = ex.Message;
                        originalProcLocation_PhysicalFile_statusImage.MouseUp += new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);
                    }));
                    CurrentContext.ComparisonProc = string.Empty;
                    CurrentContext.ComparisonProcValid = false;
                }
                else
                    throw;
            }
        }

        protected void displayErrorMessage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show((sender as StatusImage).ErrorMessage, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MainWin_Closed(object sender, EventArgs e)
        {
            //TODO: Track server and database changes, because this will leave messes behind if you change servers and databases.
            //As this is a cleanup, closing window function, we swallow any cleanup that could not be done, and try to continue.
            try
            {
                SQL.Queries.DeleteProcsBeginningWith("sprockettestrun" + "_" + (MainWindow.CurrentProcess.Id | MainWindow.CurrentProcess.MachineName.GetHashCode()).ToString(),
                    CurrentContext.Server, CurrentContext.Database);
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
