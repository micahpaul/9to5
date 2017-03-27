using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private List<FieldListItem> FieldList;
        private DBConnection DBConnection = new DBConnection();

        public HistoryWindow( List<FieldListItem> _FieldList)
        {
            InitializeComponent();
            FieldList = _FieldList;
            DBConnection.PopulateDataGridFromTable("TXN", SalesDataGrid);
        }

        private void SalesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DBConnection.SaveChanges();
        }

        private void HistoryWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DBConnection.Close();
        }
    }
}
