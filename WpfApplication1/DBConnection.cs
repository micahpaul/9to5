using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Windows.Controls;
using Microsoft.Office.Interop.Excel;


namespace SalesEntryAndReporting
{
    public class DBConnection
    {
        private const String DbFileName = "9To5.sqlite";
        private const String BackupDir = "Backups";
        private SQLiteConnection Connection;
        private SQLiteDataAdapter DataAdapter;
        private System.Data.DataTable DataTable;

        public DBConnection()
        {
            if (!File.Exists(DbFileName))
            {
                SQLiteConnection.CreateFile(DbFileName);
            }

            System.IO.Directory.CreateDirectory(BackupDir);
            File.Copy(DbFileName, Path.Combine( BackupDir, DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + DbFileName));

            Connection = new SQLiteConnection("Data Source=" + DbFileName + ";Version=3;");
        }

        public void Close()
        {
            try
            {
                if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("DBConnection.Close() Error: {0}", e.Message);
            }
        }

        private int Exec(String _Sql, SQLiteCommand _Cmd = null)
        {
            int Result = 0;

            try
            {
                SQLiteCommand Command = _Cmd;

                if (Command == null)
                {
                    Command = new SQLiteCommand(_Sql, Connection);
                }

                Connection.Open();
                Result = Command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("DBConnection.Exec() Error: {0}", e.Message);
            }
            finally
            {
                Close();
            }

            return Result;
        }

        public void InitializeTable(String _TableName, List<FieldListItem> _List)
        {
            String nl = "\r\n";
            String Sql = "CREATE TABLE IF NOT EXISTS " + _TableName + " (" + nl;
            Sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE";

            foreach (FieldListItem item in _List)
            {
                Sql += nl + ", " + item.FieldSql();
            }

            Sql += nl + ")";

            Exec(Sql);
        }

        public long InsertRow(String _TableName, List<FieldListItem> _List)
        {
            long Result = 0;

            Dictionary<String, object> InsertDict = new Dictionary<String, object>();

            foreach (FieldListItem item in _List)
            {
                item.InsertField(InsertDict);
            }

            if (InsertDict.Count > 0)
            {
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
                Result = Exec("", Command);
            }

            return Result;
        }

        public void PopulateDataGridFromTable(String _TableName, DataGrid _Grid)
        {
            Connection.Open();

            try
            {
                String SelectSql = "SELECT * FROM " + _TableName;
                DataAdapter = new SQLiteDataAdapter(SelectSql, Connection);
                DataTable = new System.Data.DataTable(_TableName);
                DataAdapter.Fill(DataTable);
                _Grid.DataContext = DataTable.DefaultView;
            }
            catch (Exception e)
            {
                Console.WriteLine("PopulateDataGridFromTable() Error: {0}", e.Message);
            }
        }

        public void ExcelSheetQuery(String _FileName, String _SheetName, String _TableName, String _Where)
        {
            try
            {
                // Build query string
                String SelectSql = "SELECT ";

                // open excel.
                Application xlapp = new Application();
                xlapp.DisplayAlerts = false;

                // open the workbook. 
                Workbook wb = xlapp.Workbooks.Open(_FileName);
                Worksheet ws = (Worksheet)wb.Worksheets[_SheetName];

                // Get select fields from header row
                bool AddComma = false;

                for( int ix = 1; ix <= ws.Rows[1].Cells.Count && ix <= 30; ix++ )
                {
                    if (ws.Rows[1].Cells[ix].Text.Length > 0)
                    {
                        if (AddComma)
                        {
                            SelectSql += ", ";
                        }

                        SelectSql += ws.Rows[1].Cells[ix].Text;

                        AddComma = true;
                    }
                }

                SelectSql += " FROM " + _TableName + " " + _Where;

                Connection.Open();

                SQLiteCommand cmd = new SQLiteCommand(SelectSql, Connection);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                int CurrentRow = 2;

                while (rdr.Read())
                {
                    for ( int ix = 0; ix < rdr.FieldCount; ix++ )
                    {
                        ws.Rows[CurrentRow].Columns[ix + 1] = Convert.ToString(rdr[ix]);
                    }

                    CurrentRow++;
                }

                // save and close. 
                wb.Sheets[1].Select();
                wb.Save();
                xlapp.Quit();
                xlapp = null;

            }
            catch (Exception e)
            {
                Console.WriteLine("ExcelSheetQuery() Error: {0}", e.Message);
            }
            finally
            {
                Close();
            }
        }

        public void SaveChanges()
        {
            if (( DataTable != null) && (DataAdapter != null))
            {
                DataAdapter.UpdateCommand = new SQLiteCommandBuilder(DataAdapter).GetUpdateCommand();
                DataAdapter.Update(DataTable);
                DataTable.AcceptChanges();
            }
        }
    }
}
