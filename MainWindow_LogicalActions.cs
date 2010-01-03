using System;
using System.IO;
using System.Net;
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
            parameter.SetQueryResults(null);

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
                parameter.SetQueryResults(results.ToList<string>());

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
                var newName = SQL.Queries.TurnFileIntoProcedure(CurrentContext.ComparisonProcFilename, CurrentContext.Server, CurrentContext.Database);
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

        public void CheckUpdateStatus(object data)
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            try
            {
                var request = WebRequest.Create("http://ritter.vg/sprocket/latestversion.txt");
                var response = request.GetResponse();

                var latestVersion = new Version(response.GetResponseString());

                if (currentVersion == latestVersion)
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        updateLabel.Text = "This is the latest version of Sprocket.";
                        currentVersionLabel.Text = "v" + currentVersion.ToString();
                    }));
                else
                    this.Dispatcher.Invoke((Action)(() =>
                        {
                            currentVersionLabel.Text = "v" + currentVersion.ToString();
                            updateLabel.Text = "A new version (" + latestVersion.ToString() + ") is available. Stop living in the past man!";
                            updateLabel.Foreground = new SolidColorBrush(Colors.Blue);
                            updateLabel.Cursor = System.Windows.Input.Cursors.Hand;
                            updateLabel.TextDecorations = TextDecorations.Underline;
                            updateLabel.MouseUp += new System.Windows.Input.MouseButtonEventHandler(delegate(object sender, System.Windows.Input.MouseButtonEventArgs e)
                            {
                                System.Diagnostics.Process.Start("http://ritter.vg/sprocket/update.html");
                            });
                        }));
            }
            catch (Exception)
            {
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        currentVersionLabel.Text = "v" + currentVersion.ToString();
                        updateLabel.Text = "Error Checking For Updates.  Application will try again on next load.";
                    }));
            }
        }
    }
}