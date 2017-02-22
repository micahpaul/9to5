using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace WpfApplication1
{
    public enum FieldType { ftUndefined, ftString, ftInt, ftReal, ftDate, ftChoice };

    public class FieldListItem
    {
        public String DisplayName = "";
        public String DBFieldName = "";
        public FieldType FieldType = FieldType.ftUndefined;
        public UIElement InputElement;

        public FieldListItem(String _DisplayName, String _DBFieldName, FieldType _Type,
                             String _Default = "", UIElement _InputElement = null)
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

        public object GetValue( bool _ForSql=false )
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
                        String TmpDateFormat = _ForSql ? "yyyy-MM-dd HH:mm:ss.fff" : "MM/dd/yyyy";
                        val = pck.SelectedDate.Value.Date.ToString(TmpDateFormat);
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

            if( ( ! _ForSql ) && ( val == null ) )
            {
                val = "";
            }

            return val;
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
            object val = GetValue(true);

            if (val != null)
            {
                _Dict.Add(DBFieldName, val);
            }
        }
    }
}
