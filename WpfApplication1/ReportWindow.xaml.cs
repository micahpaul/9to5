using System;
using System.Windows;
using System.IO;
using System.Diagnostics;

namespace SalesEntryAndReporting
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            StartDatePicker.SelectedDate = month.AddMonths(-1);
            EndDatePicker.SelectedDate   = month.AddDays(-1);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            String NewFileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_New_Invoice.xlsx";
            NewFileName = Path.Combine(Path.GetTempPath(), NewFileName);

            try
            {
                File.Copy("sales_tax_report_template.xlsx", NewFileName);
                ExcelFindReplace FR = new ExcelFindReplace(NewFileName, "Summary");
                FR.SimpleReplace("|START_DATE|", StartDatePicker.Text);
                FR.SimpleReplace("|END_DATE|", EndDatePicker.Text);

                DBConnection con = new DBConnection();
                String Qt = "\"";
                DateTime StartDate = (DateTime) StartDatePicker.SelectedDate;
                DateTime EndDate = (DateTime) EndDatePicker.SelectedDate;
                String StartVal = Qt + StartDate.ToString("yyyy-MM-dd") + Qt;
                String EndVal   = Qt + EndDate.ToString("yyyy-MM-dd") + Qt;
                con.ExcelSheetQuery(NewFileName, "Details", "TXN", "WHERE SAL_DATE BETWEEN " + StartVal + " AND " + EndVal );
                Process.Start(NewFileName);
            }
            catch (Exception E)
            {
                MessageBox.Show("Error generating invoice: " + E.Message);
            }
        }
    }
}
