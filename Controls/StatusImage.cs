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

namespace Sprocket
{
    public class StatusImage : Image
    {
        public string ErrorMessage
        {
            get { return (string)this.GetValue(ErrorMessageProperty); }
            set { this.SetValue(ErrorMessageProperty, value); }
        }
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(StatusImage), new PropertyMetadata(""));
    }
}
