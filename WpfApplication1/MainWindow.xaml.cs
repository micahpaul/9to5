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
using System.Data;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    /*
        To Do:
        1. InitializeTable() - ensure DB columns are consistent with field list (show warning / fix if not?)
        2. On close, back up DB to a configured location
        3. Configurable default values: Table? .cfg file?
        4. Re-default FieldListItem InputElements when re-opening the entry screen
        5. Persist DB changes from History form - https://www.codeproject.com/articles/153407/wpf-and-sqlite-database; https://msdn.microsoft.com/en-us/library/y2ad8t9c.aspx
        6. History form date filter?
        7. Refactoring: Decouple and Generalize classes as much as possible
        8. Move various classes to more appropriate .cs files
        9. Review "using" directives; update appropriately
        10. Implement Invoice Generation
        11. Allow Invoice Generation from History form
        12. Implement Monthly Sales Tax Report (date filter; update Excel template)
        13. VIN validation: 17 characters exactly
        14. VIN API? https://vpic.nhtsa.dot.gov/api/Home/Index/LanguageExamples; https://vpic.nhtsa.dot.gov/api/
    */

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

        public void InitializeTable(String _TableName, List<FieldListItem> _List)
        {
            try
            {
                String nl = "\r\n";
                String Sql = "CREATE TABLE IF NOT EXISTS " + _TableName + " (" + nl;
                Sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE";

                foreach (FieldListItem item in _List)
                {
                    Sql += nl + ", " + item.FieldSql();
                }

                Sql += nl + ")";

                Connection.Open();
                SQLiteCommand Command = new SQLiteCommand(Sql, Connection);
                Command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("InitializeTables() Error: {0}", e.Message);
            }
            finally
            {
                if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
        }

        public long InsertRow(String _TableName, List<FieldListItem> _List)
        {
            long Result = 0;

            try
            {
                Dictionary<String, object> InsertDict = new Dictionary<String, object>();

                foreach (FieldListItem item in _List)
                {
                    item.InsertField(InsertDict);
                }

                if (InsertDict.Count > 0)
                {
                    Connection.Open();
                    SQLiteCommand Command = new SQLiteCommand(Connection);
                    String FieldNames = "";

                    foreach (KeyValuePair<String, object> entry in InsertDict)
                    {
                        if (FieldNames.Length > 0)
                        {
                            FieldNames += ", ";
                        }

                        FieldNames += entry.Key;
                        Command.Parameters.Add(new SQLiteParameter("@" + entry.Key, entry.Value));
                    }

                    Command.CommandText = "INSERT INTO " + _TableName + " (" + FieldNames + ")\r\n";
                    Command.CommandText += "VALUES (" + "@" + FieldNames.Replace(", ", ", @") + ")";

                    Command.CommandType = System.Data.CommandType.Text;
                    Result = Command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("InsertRow() Error: {0}", e.Message);
            }
            finally
            {
                if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }

            return Result;
        }

        public void PopulateDataGridFromTable(String _TableName, DataGrid _Grid)
        {
            Connection.Open();

            try
            {
                String SelectSql = "SELECT * FROM " + _TableName;
                SQLiteDataAdapter da = new SQLiteDataAdapter(SelectSql, Connection);
                DataTable dt = new DataTable(_TableName);
                da.Fill(dt);
                _Grid.DataContext = dt.DefaultView;
                //da.Update(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine("PopulateDataGridFromTable() Error: {0}", e.Message);
            }
            finally
            {
                if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
        }
    }

    public partial class MainWindow : Window
    {
        private List<FieldListItem> FieldList = new List<FieldListItem>();

        public MainWindow()
        {
            InitializeComponent();
            BuildFieldList();
            InitializeDB();
        }

        private void BuildFieldList()
        {
            FieldList.Add(new FieldListItem("First Name", "CST_FNAME", FieldType.ftString));
            FieldList.Add(new FieldListItem("Last Name", "CST_LNAME", FieldType.ftString));
            FieldList.Add(new FieldListItem("Address 1", "CST_ADDRESS_1", FieldType.ftString));
            FieldList.Add(new FieldListItem("Address 2", "CST_ADDRESS_2", FieldType.ftString));
            FieldList.Add(new FieldListItem("City", "CST_CITY", FieldType.ftString, "TOPEKA"));
            FieldList.Add(new FieldListItem("State", "CST_STATE", FieldType.ftString, "KS"));
            FieldList.Add(new FieldListItem("Zip", "CST_ZIP", FieldType.ftString));
            FieldList.Add(new FieldListItem("Phone Number", "CST_PHONE", FieldType.ftString));
            FieldList.Add(new FieldListItem("Warranty Coverage", "WAR_COVERAGE", FieldType.ftReal, "3000.00"));
            FieldList.Add(new FieldListItem("Warranty Period", "WAR_PERIOD", FieldType.ftString, "36 MONTHS"));
            FieldList.Add(new FieldListItem("Warranty Miles", "WAR_MILES", FieldType.ftInt, "36000"));
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
            FieldList.Add(new FieldListItem("Trade-In VIN", "SAL_TRADE_VIN", FieldType.ftString));
            FieldList.Add(new FieldListItem("Trade-In Miles", "SAL_TRADE_MILES", FieldType.ftInt));
            FieldList.Add(new FieldListItem("Tax Rate", "SAL_TAX_RATE", FieldType.ftReal, ".0915"));
            ComboBox box = new ComboBox();
            SalesTypeBinder Binder = new SalesTypeBinder(box);
            FieldList.Add(new FieldListItem("Type of Sale", "SAL_TYPE", FieldType.ftChoice, "", box));
        }

        private void InitializeDB()
        {
            DBConnection DBConnection = new DBConnection();
            DBConnection.InitializeTable("TXN", FieldList);
        }

        private void SaleButton_Click(object sender, RoutedEventArgs e)
        {
            SalesWindow salesWindow = new SalesWindow(FieldList);
            salesWindow.Owner = this;
            salesWindow.ShowDialog();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow historyWindow = new HistoryWindow(FieldList);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }
    }
}
