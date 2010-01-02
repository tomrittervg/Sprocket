using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Data.SqlClient;

namespace Sprocket
{
    public partial class MainWindow : Window
    {
        public void EnableWaitStatus(string message)
        {
            statusContainer.Visibility = Visibility.Visible;
            greyRectangle.Visibility = Visibility.Visible;
            statusLabel.Content = message;
            mainLayoutGrid.IsEnabled = false;
        }
        public void DisableWaitStatus()
        {
            statusContainer.Visibility = Visibility.Hidden;
            greyRectangle.Visibility = Visibility.Hidden;
            mainLayoutGrid.IsEnabled = true;
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

        #region Image Resources
        private const string icon_resource_bad = "Sprocket.icon_packet.knobs.PNG.Knob Cancel.png";
        private const string icon_resource_wait = "Sprocket.icon_packet.knobs.PNG.Knob Orange.png";
        private const string icon_resource_good = "Sprocket.icon_packet.knobs.PNG.Knob Valid Green.png";

        protected static ImageSource BadBMP;
        protected static ImageSource GoodBMP;
        protected static ImageSource WaitBMP;

        private void InitializeImageResources()
        {
            using (var badStream = this.GetType().Assembly.GetManifestResourceStream(icon_resource_bad))
            {
                PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(badStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BadBMP = bitmapDecoder.Frames[0];
            }
            using (var waitStream = this.GetType().Assembly.GetManifestResourceStream(icon_resource_wait))
            {
                PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(waitStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                WaitBMP = bitmapDecoder.Frames[0];
            }
            using (var goodStream = this.GetType().Assembly.GetManifestResourceStream(icon_resource_good))
            {
                PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(goodStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                GoodBMP = bitmapDecoder.Frames[0];
            }
        }
        #endregion
    }
}
