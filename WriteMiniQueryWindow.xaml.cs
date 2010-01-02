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
using System.Windows.Shapes;

namespace Sprocket
{
    /// <summary>
    /// Interaction logic for WriteMiniQueryWindow.xaml
    /// </summary>
    public partial class WriteMiniQueryWindow : Window
    {
        public MainWindow mainWindowReference { get; set; }
        public string ParamName { get; set; }
        public string PreviousQuery { get; set; }

        public WriteMiniQueryWindow()
        {
            InitializeComponent();
        }

        private void saveQuery(object sender, RoutedEventArgs e)
        {
            mainWindowReference.UpdateQueryFromModal(ParamName, query.Text);
            this.Close();
        }

        private void cancelEdit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            whichParam.Text = "Query for populating " + ParamName + "\n\n*All columns returned from the query will be used as inputs.";
            query.Text = PreviousQuery;
        }
    }
}
