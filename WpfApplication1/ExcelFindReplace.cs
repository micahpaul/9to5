using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;

namespace WpfApplication1
{
    class ExcelFindReplace
    {
        List<FieldListItem> FieldList;
        String FileName;

        public ExcelFindReplace(List<FieldListItem> _FieldList, String _FileName )
        {
            this.FieldList = _FieldList;
            this.FileName = _FileName;
        }

        public void DoReplace()
        {
            object m = Type.Missing;

            // open excel.
            Application xlapp = new Application();
            xlapp.DisplayAlerts = false;

            // open the workbook. 
            Workbook wb = xlapp.Workbooks.Open(FileName);

            // get the active worksheet. (Replace this if you need to.) 
            Worksheet ws = (Worksheet)wb.Worksheets["Invoice"];
            String Delim = "|";

            // do replace
            foreach(FieldListItem i in FieldList)
            {
                ws.Cells.Replace(Delim + i.DBFieldName + Delim, i.GetValue());
            }
            
            // save and close. 
            wb.Save();
            xlapp.Quit();
            xlapp = null;
        }
    }
}
