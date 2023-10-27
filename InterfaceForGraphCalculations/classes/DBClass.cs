using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Controls;
using System.Reflection;

namespace InterfaceForGraphCalculations.classes
{
    public static class DBClass
    {
        private static string cn_String;
        public static List<string> tableNames = new List<string>();
        public static void Tables_Upload()
        {
            using (SqlConnection con = Get_DB_Connection())
                foreach (DataRow row in con.GetSchema("Tables").Rows) 
                    if (!tableNames.Contains(row["TABLE_NAME"]))
                        tableNames.Add((string)row["TABLE_NAME"]);
        }

        public static void FillDataGrid(DataGrid tableGrid, string tableName)
        {
            using (SqlConnection con = Get_DB_Connection())
            {
                string cmdString = "SELECT * FROM " + tableName;
                SqlCommand cmd = new SqlCommand(cmdString, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable(tableName);         
                sda.Fill(dt);
                
                tableGrid.ItemsSource = dt.DefaultView;
            }
        }

        public static SqlConnection Get_DB_Connection()
        {
            cn_String = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\data\dbTest.mdf; Integrated Security=True;"; 
            SqlConnection cn_connection = new SqlConnection(cn_String);
            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();
            return cn_connection;
        }



        public static DataTable Get_DataTable(string SQL_Text)
        {
            SqlConnection cn_connection = Get_DB_Connection();
            DataTable table = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(SQL_Text, cn_connection);
            adapter.Fill(table);
            return table;
        }

        public static void Execute_SQL(string SQL_Text)
        {
            try
            {
                SqlConnection cn_connection = Get_DB_Connection();
                SqlCommand cmd_Command = new SqlCommand(SQL_Text, cn_connection);
                cmd_Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                    throw new Exception(ex + " on command \"" + SQL_Text + "\"");
            }
        }

        public static void Close_DB_Connection()
        {
            SqlConnection cn_connection = new SqlConnection(cn_String);
            if (cn_connection.State != ConnectionState.Closed) cn_connection.Close();
        }

        public static void DB_Add_Record(string tableName, List<string> new_element)
        {
            DataTable dt = Get_DataTable("SELECT * FROM " + tableName + ";");
            List<string> columnNames = new List<string>();
            foreach (DataColumn dc in dt.Columns)
                columnNames.Add(dc.ColumnName);

            StringBuilder fields = new StringBuilder();

            for (int i = 1; i < columnNames.Count; i++)
            {
                fields.Append("[");
                fields.Append(columnNames[i]);
                fields.Append("]");
                if (i != columnNames.Count - 1) fields.Append(",");
            }            

            StringBuilder values = new StringBuilder();
            for (int i = 0; i < new_element.Count; i++)
            {
                values.Append("'"); values.Append(new_element[i]); values.Append("'");
                if (i != new_element.Count - 1) values.Append(",");
            }
            string sql_Add = "INSERT INTO " + tableName + " (" + fields + ") VALUES(" + values + ")";
            Execute_SQL(sql_Add);
        }


        public static void DB_Update_Record(string tableName, List<string> element_to_update)
        {
            DataTable dt = Get_DataTable("SELECT * FROM " + tableName + ";");
            List<string> columnNames = new List<string>();
            foreach (DataColumn dc in dt.Columns)
                columnNames.Add(dc.ColumnName);
            string sSQL = "SELECT * FROM " + tableName + " WHERE [" + columnNames[0] + "] = '" + element_to_update[0] + "'";
            DataTable tbl = DBClass.Get_DataTable(sSQL);
            if (tbl.Rows.Count > 0)
            {
                for (int i = 1; i < element_to_update.Count; i++)
                {
                    string sql_Update = "UPDATE " + tableName + " SET [" + columnNames[i] + "] = '" + element_to_update[i] + "' WHERE [" + columnNames[0] + "] = '" + element_to_update[0] + "'";
                    DBClass.Execute_SQL(sql_Update);
                }
            }
        }
    }
}
