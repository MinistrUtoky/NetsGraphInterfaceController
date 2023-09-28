using InterfaceForGraphCalculations.classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml.Linq;

namespace InterfaceForGraphCalculations
{
    public partial class DataWindow : Window
    {
        private string currentTableName;
        private List<string> columnNames;
        public DataWindow()
        {
            InitializeComponent();
            columnNames = new List<string>();
            DBClass.Tables_Upload();
            currentTableName = DBClass.tableNames[0];
            Fill_Table_Names_Tree();
            Refresh_Table();
        }

        private void Fill_Table_Names_Tree()
        {
            foreach (string s in DBClass.tableNames)
            {
                Button tableButton = new Button();
                tableButton.Content = s;
                tableButton.Background = Brushes.White;
                tableButton.Foreground = Brushes.Blue;
                tableButton.Click += TableButton_Click;
                tableNamesTree.Items.Add(tableButton);
            }
        }
        private void Refresh_Table() => DBClass.FillDataGrid(tableGrid, currentTableName);
        public void SwitchTable(string tableName)
        {
            columnNames.Clear();
            currentTableName = tableName;
            Refresh_Table();
        }

        private void TableButton_Click(object sender, RoutedEventArgs e) => SwitchTable((sender as Button).Content.ToString());
        public void DB_Add_Record(List<string> new_element)
        {
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
            string sql_Add = "INSERT INTO " + currentTableName + " (" + fields + ") VALUES(" + values + ")";
            DBClass.Execute_SQL(sql_Add);
        }
        public void DB_Update_Record(List<string> element_to_update)
        {
            string sSQL = "SELECT * FROM " + currentTableName + " WHERE [" + columnNames[0] + "] = '" + element_to_update[0] + "'";
            DataTable tbl = DBClass.Get_DataTable(sSQL);
            if (tbl.Rows.Count > 0)
            {
                for (int i = 1; i < element_to_update.Count; i++)
                {
                    string sql_Update = "UPDATE " + currentTableName + " SET [" + columnNames[i] + "] = '" + element_to_update[i] + "' WHERE [" + columnNames[0] + "] = '" + element_to_update[0] + "'";
                    DBClass.Execute_SQL(sql_Update);
                }
            }
        }
        public void DB_Remove_Record(string id)
        {
            string sSQL = "SELECT * FROM " + currentTableName + " WHERE [" + columnNames[0] + "] = '" + id + "'";
            DataTable tbl = DBClass.Get_DataTable(sSQL);
            if (tbl.Rows.Count > 0)
            {
                string sql_Remove = "DELETE FROM " + currentTableName + " WHERE [" + columnNames[0] + "] = '" + id + "'";
                DBClass.Execute_SQL(sql_Remove);
            }
        }       

        private static bool EmptyElements(string s) { return s == ""; }

        private void Nope_Click(object sender, RoutedEventArgs e) => (sender as MenuItem).Header = "Nope";
        private void Help_Click(object sender, RoutedEventArgs e) => Jss.IsOpen = true;
        private void Thanks_Click(object sender, RoutedEventArgs e) => Jss.IsOpen = false;
    
    }
}

