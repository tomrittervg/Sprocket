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
using System.Deployment.Application;

namespace Sprocket
{
    public partial class MainWindow : Window
    {
        protected void LoadProcedure(object sender, RoutedEventArgs e)
        {
            EnableWaitStatus("Attempting to load procedure");

            //TODO: Is there really no better way of show/hiding rows than by futzing with their height?
            NoSPLoadedRow.Height = new GridLength(0);
            SPLoadedRow.Height = new GridLength(0);
            ThreadPool.QueueUserWorkItem(LoadProcedure, new string[] { serverName.Text, database.Text, procName.Text });
        }

        protected void LoadProcedure(object stte)
        {
            string[] state = (string[])stte;
            try
            {
                var data = SQL.Queries.GetStoredProcParameters(state[0], state[1], state[2]);
                CurrentContext.LoadParameters(data);

                this.Dispatcher.Invoke((Action)(() =>
                {
                    spParameters.ItemsSource = data;
                    SPLoadedRow.Height = new GridLength(1, GridUnitType.Star);
                    
                    originalProcLocation_AnotherProc.IsChecked = false;
                    originalProcLocation_AnotherProc_Name.Text = "";
                    originalProcLocation_AnotherProc_statusImage.Source = null;

                    originalProcLocation_PhysicalFile.IsChecked = false;
                    originalProcLocation_PhysicalFile_Name.Content = "";
                    originalProcLocation_PhysicalFile_statusImage.Source = null;

                    DisableWaitStatus();
                }));
            }
            catch (Exception ex)
            {
                if (ex is SqlException || (ex is SpecificException && ex.Message.OneOf(SpecificException.ProcedureNotFound)))
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        noParamsStatusMessage.Text = "Procedure Load Failed: \n" + ex.Message;
                        DisableWaitStatus();
                        NoSPLoadedRow.Height = new GridLength();
                    }));
                else
                    throw;
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

                    numTestsLabel.Content = (CurrentContext.QueryCombinations == 1 ? 0 : CurrentContext.QueryCombinations).ToString() + " Test Combinations";
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

        #region Specific Original Proc Comparison Functions
        protected void ValidateOriginalProcFromPhysicalFile(object data)
        {
            try
            {
                var newName = SQL.Queries.TurnFileIntoProcedure(CurrentContext.ComparisonProc_PhysicalFile_Filename, CurrentContext.Server, CurrentContext.Database);
                CurrentContext.ComparisonProc_PhysicalFile_ProcName = newName;

                ValidateProcParametersMatchCurrentContext(newName, originalProcLocation_PhysicalFile_statusImage,
                    delegate(bool status) { CurrentContext.ComparisonProc_PhysicalFile_Valid = status; });
            }
            catch (SpecificException ex)
            {
                if (ex.Message.OneOf(SpecificException.InputCouldNotBeParsed))
                    ValidateProcParametersMatchFail(ex.Message, originalProcLocation_PhysicalFile_statusImage,
                        delegate(bool status) { CurrentContext.ComparisonProc_PhysicalFile_Valid = status; });
                else
                    throw;
            }
        }
        protected void ValidateOriginalProcFromAnotherProc(object data)
        {
            ValidateProcParametersMatchCurrentContext(CurrentContext.ComparisonProc_AnotherProc_ProcName, originalProcLocation_AnotherProc_statusImage,
                delegate(bool status) { CurrentContext.ComparisonProc_AnotherProc_Valid = status; });
        }
        #endregion

        #region Generic Original Proc Comparison Functions
        protected void ValidateProcParametersMatchCurrentContext(string procName, StatusImage imgToUpdate, Action<bool> setValidStatus)
        {
            try
            {
                var newProcParams = SQL.Queries.GetStoredProcParameters(CurrentContext.Server, CurrentContext.Database, procName);

                if (!CurrentContext.ParameterValues.ParametersMatch(newProcParams))
                    throw new SpecificException(SpecificException.ProcParametersDontMatch);

                setValidStatus(true);
                this.Dispatcher.Invoke((Action)(() =>
                {
                    imgToUpdate.Source = GoodBMP;
                    imgToUpdate.MouseUp -= new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);
                }));
            }
            catch (SpecificException ex)
            {
                if (ex.Message.OneOf(SpecificException.ProcParametersDontMatch, SpecificException.ProcedureNotFound))
                    ValidateProcParametersMatchFail(ex.Message, imgToUpdate, setValidStatus);
                else
                    throw;
            }
        }
        protected void ValidateProcParametersMatchFail(string errorMessage, StatusImage imgToUpdate, Action<bool> setValidStatus)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                imgToUpdate.Source = BadBMP;
                imgToUpdate.ErrorMessage = errorMessage;
                imgToUpdate.MouseUp += new System.Windows.Input.MouseButtonEventHandler(displayErrorMessage);
            }));
            setValidStatus(false);
        }
        #endregion

        public void CheckUpdateStatus(object data)
        {//As many update messages as possible should include "Man".  Any references you can make to aviator sunglasses or incorporating the word "gnarly" would also be excellent additions.
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            try
            {
                var request = WebRequest.Create("http://ritter.vg/sprocket/latestversion.txt");
                var response = request.GetResponse();

                var latestVersion = new Version(response.GetResponseString());

                bool isInstalled = true;
                try { var throwaway = ApplicationDeployment.CurrentDeployment.TimeOfLastUpdateCheck; }
                catch (Exception ex) { if (ex.Message == "Application is not installed.") isInstalled = false; }

                if (!isInstalled)
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        updateLabel.Text = "It looks like this is a debug build (or it's not installed for some reason).";
                        currentVersionLabel.Text = "v" + currentVersion.ToString();
                    }));
                else if (currentVersion >= latestVersion)
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
                                MessageBox.Show("Rock on man!  I'm gonna update the shit out of this application, and then disappear, cause WPF doesn't have a Application.Restart method.  You're gonna have to re-open me.", 
                                    "Thanks!", MessageBoxButton.OK, MessageBoxImage.Error);
                                try
                                {
                                    ApplicationDeployment.CurrentDeployment.Update();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error while Updating Application, Bailing Out.  Sorry Man!", "Oh Shit", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                finally
                                {
                                    this.Close();
                                }
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