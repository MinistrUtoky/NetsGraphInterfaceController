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
        public DataWindow()
        {
            InitializeComponent();
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
            currentTableName = tableName;
            Refresh_Table();
        }

        private void TableButton_Click(object sender, RoutedEventArgs e) => SwitchTable((sender as Button).Content.ToString());

    
    }
}

