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
        public char DBCode;
        public String FriendlyName;

        public SaleTypeInfo(SaleType _Type)
        {
            Type = _Type;

            switch (Type)
            {
                case SaleType.Retail:
                    DBCode = 'R';
                    FriendlyName = "Retail";
                    break;
                case SaleType.Wholesale:
                    DBCode = 'W';
                    FriendlyName = "Wholesale";
                    break;
                case SaleType.RetailOutOfState:
                    DBCode = 'O';
                    FriendlyName = "Retail - Out of State";
                    break;
                default:
                    DBCode = ' ';
                    FriendlyName = "None";
                    break;
            }
        }
    }

    // Visual auxilliary
    public class SalesTypeBinder
    {
        public Dictionary<SaleType, String> Dict = new Dictionary<SaleType, String>();

        public SalesTypeBinder(ComboBox _Box)
        {
            foreach (SaleType type in Enum.GetValues(typeof(SaleType)))
            {
                Dict.Add(type, new SaleTypeInfo(type).FriendlyName);
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

        public FieldListItem( String _DisplayName, String _DBFieldName, FieldType _Type, UIElement _InputElement=null )
        {
            DisplayName    = _DisplayName;
            DBFieldName    = _DBFieldName;
            FieldType      = _Type;

            if( _InputElement != null )
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
                    case FieldType.ftString:
                    default:
                        InputElement = new TextBox();
                        break;
                }
            }
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
    }

    public partial class SalesWindow : Window
    {
        private bool IsSaved = false;

        public SalesWindow( List<FieldListItem> _FieldList )
        {
            InitializeComponent();
            FillGrids(_FieldList);
        }

        private void FillGrids(List<FieldListItem> _FieldList)
        {
            // Add 4 columns
            foreach (int ix in Enumerable.Range(0, 4))
            {
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            GridLength height = new GridLength(30);
            int CurrentRow = 0;
            int Col0 = 0;

            for (int ix = 0; ix < _FieldList.Count; ix++)
            {
                if(Col0 == 0 && ix > (_FieldList.Count / 2))
                {
                    Col0 = 2;
                    CurrentRow = 0;
                }

                // Add a row to grid if we're in the first half
                if (Col0 == 0)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = height;
                    FieldGrid.RowDefinitions.Add(row);
                }

                // in Col0, add a label
                TextBlock txt = new TextBlock();
                txt.Text = _FieldList[ix].DisplayName + ":";
                txt.HorizontalAlignment = HorizontalAlignment.Right;
                txt.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(txt, CurrentRow);
                Grid.SetColumn(txt, Col0);
                FieldGrid.Children.Add(txt);

                // in Col0 + 1 of the row, add a control to enter the data
                UIElement input = _FieldList[ix].InputElement;
                Grid.SetRow(input, CurrentRow);
                Grid.SetColumn(input, Col0 + 1);
                FieldGrid.Children.Add(input);

                CurrentRow++;
            }
        }

        private bool Save()
        {
            IsSaved = false;

            if ( ! ValidateFields() )
            {
                MessageBox.Show("Unable to save; one or more fields not valid.");
            }
            else
            {
                //System.Data.sqllite
                MessageBox.Show("Saving... Just kidding; that's not implemented yet.");
                IsSaved = true;
            }

            return IsSaved;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save();    
        }

        private void PrintInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSaved && !Save())
            {
                MessageBox.Show("You can't print an unsaved invoice.");
            }
            else
            {
                MessageBox.Show("Printing... Just kidding; that's not implemented yet.");
            }
        }

        private bool ValidateFields()
        {
            return true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}