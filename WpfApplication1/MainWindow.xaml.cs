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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data.SQLite;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class DBConnection
    {
        private const String DbFileName = "9To5.sqlite";
        private SQLiteConnection Connection;

        public DBConnection()
        {
            if (!File.Exists(DbFileName))
            {
                SQLiteConnection.CreateFile(DbFileName);
            }

            Connection = new SQLiteConnection("Data Source=" + DbFileName + ";Version=3;");
        }

        public void InitializeTables(List<FieldListItem> _List)
        {
            try
            {
                Connection.Open();
                String nl = "\r\n";
                String Sql  = "CREATE TABLE IF NOT EXISTS TXN (" + nl;
                Sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE," + nl;
                Sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE," + nl;
            }
            catch( Exception e)
            {
                Console.WriteLine("InitializeTables() Error: {0}", e.Message);
            }
            finally
            {
                if ( Connection != null && Connection.State != System.Data.ConnectionState.Closed )
                {
                    Connection.Close();
                }
            }
        }
    }

    public partial class MainWindow : Window
    {
        DBConnection DBConnection = new DBConnection();
        private List<FieldListItem> FieldList = new List<FieldListItem>();

        public MainWindow()
        {
            InitializeComponent();
            BuildFieldList();
            //InitializeDB();
        }

        private void BuildFieldList()
        {
            FieldList.Add(new FieldListItem("First Name", "CST_FNAME", FieldType.ftString));
            FieldList.Add(new FieldListItem("Last Name", "CST_LNAME", FieldType.ftString));
            FieldList.Add(new FieldListItem("Address 1", "CST_ADDRESS_1", FieldType.ftString));
            FieldList.Add(new FieldListItem("Address 2", "CST_ADDRESS_2", FieldType.ftString));
            FieldList.Add(new FieldListItem("City", "CST_CITY", FieldType.ftString));
            FieldList.Add(new FieldListItem("State", "CST_STATE", FieldType.ftString));
            FieldList.Add(new FieldListItem("Zip", "CST_ZIP", FieldType.ftString));
            FieldList.Add(new FieldListItem("Phone Number", "CST_PHONE", FieldType.ftString));
            FieldList.Add(new FieldListItem("Warranty Coverage", "WAR_COVERAGE", FieldType.ftReal));
            FieldList.Add(new FieldListItem("Warranty Period", "WAR_PERIOD", FieldType.ftString));
            FieldList.Add(new FieldListItem("Warranty Miles", "WAR_MILES", FieldType.ftInt));
            FieldList.Add(new FieldListItem("Warranty Deductible", "WAR_DEDUCTIBLE", FieldType.ftReal));
            FieldList.Add(new FieldListItem("VIN", "VEH_VIN", FieldType.ftString));
            FieldList.Add(new FieldListItem("Year", "VEH_YEAR", FieldType.ftInt));
            FieldList.Add(new FieldListItem("Make", "VEH_MAKE", FieldType.ftString));
            FieldList.Add(new FieldListItem("Model", "VEH_MODEL", FieldType.ftString));
            FieldList.Add(new FieldListItem("Mileage", "VEH_MILEAGE", FieldType.ftReal));
            FieldList.Add(new FieldListItem("Trim", "VEH_TRIM", FieldType.ftString));
            FieldList.Add(new FieldListItem("Date of Sale", "SAL_DATE", FieldType.ftDate));
            FieldList.Add(new FieldListItem("Sale Amount", "SAL_AMOUNT", FieldType.ftReal));
            FieldList.Add(new FieldListItem("Trade-In Amount", "SAL_TRADE_AMT", FieldType.ftReal));
            ComboBox box = new ComboBox();
            SalesTypeBinder Binder = new SalesTypeBinder(box);
            FieldList.Add(new FieldListItem("Type of Sale", "SAL_TYPE", FieldType.ftChoice, box));
        }

        private void SaleButton_Click(object sender, RoutedEventArgs e)
        {
            SalesWindow salesWindow = new SalesWindow(FieldList);
            salesWindow.Owner = this;
            salesWindow.ShowDialog();
        }
    }
}
