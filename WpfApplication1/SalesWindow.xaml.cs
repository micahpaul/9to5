using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;


namespace SalesEntryAndReporting
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class SalesWindow : Window
    {
        private bool IsSaved = false;
        private List<FieldListItem> FieldList;

        public SalesWindow(List<FieldListItem> _FieldList)
        {
            FieldList = _FieldList;
            InitializeComponent();
            FillGrids();
        }

        private void FillGrids()
        {
            // Add 4 columns
            foreach (int ix in Enumerable.Range(0, 4))
            {
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            GridLength height = new GridLength(35);
            int CurrentRow = 0;
            int Col0 = 0;

            for (int ix = 0; ix < FieldList.Count; ix++)
            {
                if (Col0 == 0 && ix >= (FieldList.Count / 2))
                {
                    Col0 = 2;
                    CurrentRow = 0;
                }

                // Add a row to grid if it doesn't have enough
                if ((FieldGrid.RowDefinitions.Count - 1) < CurrentRow)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = height;
                    FieldGrid.RowDefinitions.Add(row);
                }

                // in Col0, add a label
                TextBlock txt = new TextBlock();
                txt.Text = FieldList[ix].DisplayName + ":";
                txt.HorizontalAlignment = HorizontalAlignment.Right;
                txt.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(txt, CurrentRow);
                Grid.SetColumn(txt, Col0);
                FieldGrid.Children.Add(txt);

                // in Col0 + 1 of the row, add a control to enter the data
                UIElement input = FieldList[ix].InputElement;
                Grid.SetRow(input, CurrentRow);
                Grid.SetColumn(input, Col0 + 1);
                FieldGrid.Children.Add(input);

                if (ix == 0)
                {
                    input.Focus();
                }

                CurrentRow++;
            }
        }

        private bool Save()
        {
            IsSaved = false;
            String InvalidFields;

            if (!ValidateFields(out InvalidFields))
            {
                MessageBox.Show("Unable to save. The following fields are not valid: " + InvalidFields + ".");
            }
            else
            {
                DBConnection con = new DBConnection();
                if (con.InsertRow("TXN", FieldList) > 0)
                {
                    MessageBox.Show("Saved!");
                    IsSaved = true;
                }
                else
                {
                    MessageBox.Show("Uh oh. Not saved.");
                    IsSaved = false;
                }
            }

            return IsSaved;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void GenerateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSaved && !Save())
            {
                MessageBox.Show("You can't generate an unsaved invoice.");
            }
            else
            {
                GenerateInvoice();
            }
        }

        private void GenerateInvoice()
        {
            String NewFileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_New_Invoice.xlsx";
            NewFileName = Path.Combine(Path.GetTempPath(), NewFileName);

            try
            {
                File.Copy("invoice_template.xlsx", NewFileName);
                ExcelFindReplace FR = new ExcelFindReplace(NewFileName, "Data");
                FR.DelimReplace(FieldList);
                Process.Start(NewFileName);
            }
            catch(Exception E)
            {
                MessageBox.Show("Error generating invoice: " + E.Message);
            }
        }

        private bool ValidateFields(out String _InvalidFields)
        {
            _InvalidFields = "";
            bool Result = true;
            String InvalidField = "";

            foreach (FieldListItem i in FieldList)
            {
                if (!i.InputIsValid(out InvalidField))
                {
                    Result = false;

                    if (_InvalidFields.Length > 0)
                    {
                        _InvalidFields += ", ";
                    }

                    _InvalidFields += InvalidField;
                }
            }

            return Result;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SalesWindow1_Closed(object sender, EventArgs e)
        {
            FieldGrid.Children.Clear();

            foreach (FieldListItem i in FieldList)
            {
                i.ClearValue();
            }
        }
    }
}