using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;

namespace SalesEntryAndReporting
{
    class ExcelFindReplace
    {
        String FileName;
        String SheetName;

        public ExcelFindReplace( String _FileName, String _SheetName )
        {
            this.FileName = _FileName;
            this.SheetName = _SheetName;
        }

        public void DelimReplace(List<FieldListItem> _FieldList)
        {
            // open excel.
            Application xlapp = new Application();
            xlapp.DisplayAlerts = false;

            // open the workbook. 
            Workbook wb = xlapp.Workbooks.Open(FileName);
            Worksheet ws = (Worksheet)wb.Worksheets[SheetName];
            String Delim = "|";

            // do replace
            foreach(FieldListItem i in _FieldList)
            {
                ws.Cells.Replace(Delim + i.DBFieldName + Delim, i.GetValue());
            }

            // save and close. 
            wb.Sheets[1].Select();
            wb.Save();
            xlapp.Quit();
            xlapp = null;
        }

        public void SimpleReplace(String _OldValue, String _NewValue)
        {
            // open excel.
            Application xlapp = new Application();
            xlapp.DisplayAlerts = false;

            // open the workbook. 
            Workbook wb = xlapp.Workbooks.Open(FileName);
            Worksheet ws = (Worksheet)wb.Worksheets[SheetName];

            ws.Cells.Replace(_OldValue, _NewValue);

            // save and close. 
            wb.Sheets[1].Select();
            wb.Save();
            xlapp.Quit();
            xlapp = null;
        }
    }
}
