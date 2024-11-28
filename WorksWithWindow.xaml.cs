using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace CompanyManagementApp
{
    public partial class WorksWithWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public WorksWithWindow()
        {
            InitializeComponent();
            LoadWorksWithData();
            LoadEmployeeData();
            LoadClientData();
        }

        private void LoadWorksWithData()
        {
            string query = @"SELECT ww.employee_id AS EmployeeID, 
                                CONCAT(e.Given_Name, ' ', e.Family_Name) AS EmployeeName, 
                                ww.client_id AS ClientID, 
                                c.client_name AS ClientName, 
                                ww.total_sales AS TotalSales 
                         FROM working_with ww
                         LEFT JOIN employees e ON ww.employee_id = e.ID
                         LEFT JOIN clients c ON ww.client_id = c.client_id";

            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            WorksWithDataGrid.ItemsSource = dt.DefaultView;
        }

        private void LoadEmployeeData()
        {
            string query = "SELECT ID, CONCAT(Given_Name, ' ', Family_Name) AS FullName FROM employees";
            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            EmployeeComboBox.ItemsSource = dt.DefaultView;
        }

        private void LoadClientData()
        {
            string query = "SELECT client_id, client_name FROM clients";
            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            ClientComboBox.ItemsSource = dt.DefaultView;
        }

        private void WorksWithDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WorksWithDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)WorksWithDataGrid.SelectedItem;
                EmployeeComboBox.SelectedValue = row["EmployeeID"];
                ClientComboBox.SelectedValue = row["ClientID"];
                TotalSalesTextBox.Text = row["TotalSales"].ToString();
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            int employeeId = Convert.ToInt32(EmployeeComboBox.SelectedValue);
            int clientId = Convert.ToInt32(ClientComboBox.SelectedValue);
            int totalSales = int.Parse(TotalSalesTextBox.Text);

            string query = $"INSERT INTO working_with (employee_id, client_id, total_sales) " +
                           $"VALUES ({employeeId}, {clientId}, {totalSales})";
            dbHelper.ExecuteNonQuery(query);
            LoadWorksWithData();
            ClearFields(); // Clear fields after insertion
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorksWithDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)WorksWithDataGrid.SelectedItem;
                int employeeId = Convert.ToInt32(row["EmployeeID"]);
                int clientId = Convert.ToInt32(row["ClientID"]);
                int totalSales = int.Parse(TotalSalesTextBox.Text);

                string query = $"UPDATE working_with SET total_sales = {totalSales} " +
                               $"WHERE employee_id = {employeeId} AND client_id = {clientId}";
                dbHelper.ExecuteNonQuery(query);
                LoadWorksWithData();
                ClearFields(); // Clear fields after update
            }
            else
            {
                MessageBox.Show("Please select a record to update.");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorksWithDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)WorksWithDataGrid.SelectedItem;
                int employeeId = Convert.ToInt32(row["EmployeeID"]);
                int clientId = Convert.ToInt32(row["ClientID"]);

                string query = $"DELETE FROM working_with WHERE employee_id = {employeeId} AND client_id = {clientId}";
                dbHelper.ExecuteNonQuery(query);
                LoadWorksWithData();
                ClearFields(); // Clear fields after deletion
            }
            else
            {
                MessageBox.Show("Please select a record to delete.");
            }
        }

        private void ClearFields()
        {
            EmployeeComboBox.SelectedIndex = -1;
            ClientComboBox.SelectedIndex = -1;
            TotalSalesTextBox.Text = string.Empty;
            WorksWithDataGrid.SelectedItem = null; // Deselect any selected row
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();

            try
            {
                // Base query to select works_with data
                string query = @"SELECT ww.employee_id AS EmployeeID, 
                                CONCAT(e.Given_Name, ' ', e.Family_Name) AS EmployeeName, 
                                ww.client_id AS ClientID, 
                                c.client_name AS ClientName, 
                                ww.total_sales AS TotalSales 
                         FROM working_with ww
                         LEFT JOIN employees e ON ww.employee_id = e.ID
                         LEFT JOIN clients c ON ww.client_id = c.client_id";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // Check if there is a search query
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += @" WHERE LOWER(CONCAT(e.Given_Name, ' ', e.Family_Name)) LIKE @Search
                        OR LOWER(e.Given_Name) LIKE @Search
                        OR LOWER(e.Family_Name) LIKE @Search";
                    parameters.Add(new MySqlParameter("@Search", $"%{searchQuery.ToLower()}%"));
                }

                // Execute the query with parameters
                MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query, parameters.ToArray());
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Bind the filtered results to the DataGrid
                WorksWithDataGrid.ItemsSource = dt.DefaultView;

                // Optionally handle the case where no results are found
                if (dt.Rows.Count == 0)
                {
                    WorksWithDataGrid.ItemsSource = null; // Clear the grid
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while searching: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
