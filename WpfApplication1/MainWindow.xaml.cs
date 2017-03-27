using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    /*
        To Do:
       * Implement Monthly Sales Tax Report (date filter; update Excel template)
       * Re-default FieldListItem InputElements when re-opening the entry screen
       * Refactoring: Decouple and Generalize classes as much as possible
       * Allow Invoice Generation from History form
       * History form date filter?
       * VIN validation: 17 characters exactly
       * InitializeTable() - ensure DB columns are consistent with field list (show warning / fix if not?)
       * On close, back up DB to a configured location
       * Configurable default values: Table? .cfg file?
       * Review "using" directives; update appropriately
       * VIN API? https://vpic.nhtsa.dot.gov/api/Home/Index/LanguageExamples; https://vpic.nhtsa.dot.gov/api/
    */

    

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
            SaleTypeBinder Binder = new SaleTypeBinder(box);
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
