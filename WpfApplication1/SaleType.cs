using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace WpfApplication1
{
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
    public class SaleTypeBinder
    {
        public Dictionary<String, String> Dict = new Dictionary<String, String>();

        public SaleTypeBinder(ComboBox _Box)
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
}
