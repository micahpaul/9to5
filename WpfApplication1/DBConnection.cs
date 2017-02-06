using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Windows.Controls;


namespace WpfApplication1
{
    public class DBConnection
    {
        private const String DbFileName = "9To5.sqlite";
        private SQLiteConnection Connection;

        public DBConnection()
        {
            if (!File.Exists(DbFileName))
            {
                SQLiteConnection.CreateFile(DbFileName);
            }

            Connection = new SQLiteConnection("Data Source=" + DbFileName + ";Version=3;");
        }

        private int Exec(String _Sql, SQLiteCommand _Cmd=null)
        {
            int Result = 0;

            try
            {
                SQLiteCommand Command = _Cmd;
                
                if( Command == null)
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
                if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
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
                SQLiteDataAdapter da = new SQLiteDataAdapter(SelectSql, Connection);
                DataTable dt = new DataTable(_TableName);
                da.Fill(dt);
                _Grid.DataContext = dt.DefaultView;
                //da.Update(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine("PopulateDataGridFromTable() Error: {0}", e.Message);
            }
            finally
            {
                if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
        }
    }
}
