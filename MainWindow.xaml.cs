using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprocket
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AssertNoDesignChanges();
        }

        public void PopulateSPParamaters()
        {
            var pretend = new List<SQLParam>()
            {
                new SQLParam("companyId", "int"),
                new SQLParam("objectId", "int"),
                new SQLParam("ninja", "varchar(255)")
            };
            spParameters.ItemsSource = pretend;
        }

        private void loadProc(object sender, RoutedEventArgs e)
        {
            NoSPLoadedRow.Height = new GridLength(0);
            SPLoadedRow.Height = new GridLength();

            PopulateSPParamaters();
        }
    }
}
