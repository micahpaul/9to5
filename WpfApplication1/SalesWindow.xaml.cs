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

    // Visual auxilliary
    public class MarginSetter
    {
        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter), new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

        public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Make sure this is put on a panel
            var panel = sender as Panel;

            if (panel == null) return;


            panel.Loaded += new RoutedEventHandler(panel_Loaded);

        }

        static void panel_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as Panel;

            // Go over the children and set margin for them:
            foreach (var child in panel.Children)
            {
                var fe = child as FrameworkElement;

                if (fe == null) continue;

                fe.Margin = MarginSetter.GetMargin(panel);
            }
        }
    }

    // Non-Visual
    public enum FieldType { ftUndefined, ftString, ftInt, ftReal, ftDate, ftChoice };

    // Non-Visual
    public enum SaleType { Retail, Wholesale, RetailOutOfState };
    public struct SaleTypeInfo
    {
        public SaleType Type;
        public String DBCode;
        public String FriendlyName;

        public SaleTypeInfo(SaleType _Type)
        {
            Type = _Type;

            switch (Type)
            {
                case SaleType.Retail:
                    DBCode = "R";
                    FriendlyName = "Retail";
                    break;
                case SaleType.Wholesale:
                    DBCode = "W";
                    FriendlyName = "Wholesale";
                    break;
                case SaleType.RetailOutOfState:
                    DBCode = "O";
                    FriendlyName = "Retail - Out of State";
                    break;
                default:
                    DBCode = "";
                    FriendlyName = "None";
                    break;
            }
        }
    }
    // Visual auxilliary
    public class SalesTypeBinder
    {
        public Dictionary<String, String> Dict = new Dictionary<String, String>();

        public SalesTypeBinder(ComboBox _Box)
        {
            foreach (SaleType type in Enum.GetValues(typeof(SaleType)))
            {
                SaleTypeInfo info = new SaleTypeInfo(type);
                Dict.Add(info.DBCode, info.FriendlyName);
            }

            _Box.ItemsSource = Dict;
            _Box.DisplayMemberPath = "Value";
            _Box.SelectedValuePath = "Key";
            _Box.SelectedIndex = 0;
        }
    }

    // Visual Auxilliary
    public class FieldListItem
    {
        public String DisplayName = "";
        public String DBFieldName = "";
        public FieldType FieldType = FieldType.ftUndefined;
        public UIElement InputElement;

        public FieldListItem(String _DisplayName, String _DBFieldName, FieldType _Type,
                             String _Default="", UIElement _InputElement = null)
        {
            DisplayName = _DisplayName;
            DBFieldName = _DBFieldName;
            FieldType = _Type;

            if (_InputElement != null)
            {
                InputElement = _InputElement;
            }
            else
            {
                switch (_Type)
                {
                    case FieldType.ftChoice:
                        InputElement = new ComboBox();
                        break;
                    case FieldType.ftDate:
                        DatePicker pck = new DatePicker();
                        pck.SelectedDate = DateTime.Today;
                        InputElement = pck;
                        break;
                    case FieldType.ftString:
                    case FieldType.ftInt:
                    case FieldType.ftReal:
                    default:
                        TextBox txt = new TextBox();
                        txt.CharacterCasing = CharacterCasing.Upper;
                        txt.GotFocus += TextBox_GotFocus;

                        if (_Default.Length > 0)
                        {
                            txt.Text = _Default;
                        }

                        InputElement = txt;
                        break;
                }
            }
        }

        void TextBox_GotFocus(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            box.SelectAll();
        }

        private String GetSqlType()
        {
            String Result = "";

            switch (FieldType)
            {
                case FieldType.ftInt:
                    Result = "INTEGER";
                    break;
                case FieldType.ftReal:
                    Result = "REAL";
                    break;
                case FieldType.ftDate:
                case FieldType.ftChoice:
                case FieldType.ftString:
                default:
                    Result = "TEXT";
                    break;
            }

            return Result;
        }

        public String FieldSql()
        {
            return DBFieldName + " " + GetSqlType();
        }

        public void ClearValue()
        {
            if (InputElement is TextBox)
            {
                TextBox box = InputElement as TextBox;
                box.Text = "";
            }
            else if (InputElement is DatePicker)
            {
                DatePicker pck = InputElement as DatePicker;
                pck.Text = "";
            }
            else if (InputElement is ComboBox)
            {
                ComboBox cmb = InputElement as ComboBox;
                cmb.SelectedIndex = 0;
            }
        }

        public bool InputIsValid(out String _InvalidField)
        {
            bool Result = false;

            switch (FieldType)
            {
                case FieldType.ftChoice:
                case FieldType.ftDate:
                case FieldType.ftString:
                    {
                        Result = true;
                        break;
                    }
                case FieldType.ftInt:
                case FieldType.ftReal:
                    {
                        TextBox box = InputElement as TextBox;
                        if (box.Text.Length > 0)
                        {
                            if (FieldType == FieldType.ftInt)
                            {
                                int n;
                                Result = int.TryParse(box.Text, out n);
                            }
                            else
                            {
                                double d;
                                Result = double.TryParse(box.Text, out d);
                            }
                        }
                        else
                        {
                            Result = true;
                        }

                        break;
                    }
                default:
                    Result = false;
                    break;
            }

            _InvalidField = Result ? "" : DisplayName;
            return Result;
        }

        public void InsertField(Dictionary<String, object> _Dict)
        {
            object val = null;

            switch (FieldType)
            {
                case FieldType.ftChoice:
                    ComboBox box = InputElement as ComboBox;

                    if (box.SelectedIndex >= 0)
                    {
                        val = box.SelectedValue;
                    }

                    break;
                case FieldType.ftDate:
                    DatePicker pck = InputElement as DatePicker;

                    if (pck.SelectedDate != null)
                    {
                        val = pck.SelectedDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }

                    break;
                case FieldType.ftString:
                case FieldType.ftInt:
                case FieldType.ftReal:
                    TextBox txt = InputElement as TextBox;

                    if (txt.Text.Length > 0)
                    {
                        if (FieldType == FieldType.ftString)
                        {
                            val = txt.Text;
                        }
                        else if (FieldType == FieldType.ftInt)
                        {
                            int i;

                            if (int.TryParse(txt.Text, out i))
                            {
                                val = i;
                            }
                        }
                        else if (FieldType == FieldType.ftReal)
                        {
                            double d;

                            if (double.TryParse(txt.Text, out d))
                            {
                                val = d;
                            }
                        }
                    }

                    break;
            }

            if(val != null)
            {
                _Dict.Add(DBFieldName, val);
            }
        }
    }

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
                if( con.InsertRow("TXN", FieldList) > 0 )
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
                MessageBox.Show("Generating... Just kidding; that's not implemented yet.");
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