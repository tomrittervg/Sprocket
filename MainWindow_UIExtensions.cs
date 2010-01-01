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
        protected const string icon_resource_bad = "Sprocket.icon_packet.knobs.PNG.Knob Cancel.png";
        protected const string icon_resource_wait = "Sprocket.icon_packet.knobs.PNG.Knob Orange.png";
        protected const string icon_resource_good = "Sprocket.icon_packet.knobs.PNG.Knob Valid Green.png";

        protected static ImageSource BadBMP;
        protected static ImageSource GoodBMP;
        protected static ImageSource WaitBMP;

        protected void InitializeImageResources()
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
    }
}
