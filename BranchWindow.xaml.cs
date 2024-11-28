using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace CompanyManagementApp
{
    public partial class BranchWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public BranchWindow()
        {
            InitializeComponent();
            LoadBranchData();
            LoadManagerData(); // Load employees for manager selection
        }

        private void LoadBranchData()
        {
            string query = "SELECT b.id, b.branch_name AS BranchName, CONCAT(e.Given_Name, ' ', e.Family_Name) AS ManagerName, b.manager_started_at AS ManagerStartedAt " +
                           "FROM branches b " +
                           "LEFT JOIN employees e ON b.manager_id = e.id";

            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            BranchDataGrid.ItemsSource = dt.DefaultView;
        }

        private void LoadManagerData()
        {
            // Load employees into ManagerComboBox
            string query = "SELECT id, CONCAT(Given_Name, ' ', Family_Name) AS FullName FROM employees";
            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            ManagerComboBox.ItemsSource = dt.DefaultView;
            ManagerComboBox.SelectedIndex = 0; // Set the first item as default selection
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            string branchName = BranchNameTextBox.Text;
            DateTime managerStartedAt = ManagerStartedAtPicker.SelectedDate ?? DateTime.Now;

            // Check if a manager is selected and retrieve the ID
            if (ManagerComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a manager.");
                return;
            }

            int managerId = Convert.ToInt32(ManagerComboBox.SelectedValue); // Convert SelectedValue safely to int

            // Insert query to include all fields
            string query = $"INSERT INTO branches (branch_name, manager_id, manager_started_at) " +
                           $"VALUES ('{branchName}', {managerId}, '{managerStartedAt:yyyy-MM-dd}')";
            dbHelper.ExecuteNonQuery(query);
            LoadBranchData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BranchDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)BranchDataGrid.SelectedItem;
                int branchId = Convert.ToInt32(row["id"]);
                string query = $"DELETE FROM branches WHERE id = {branchId}";
                dbHelper.ExecuteNonQuery(query);
                LoadBranchData();
            }
            else
            {
                MessageBox.Show("Please select a branch to delete.");
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();

            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    // If search box is empty, reload all branches
                    LoadBranchData();
                    return;
                }

                // Parameterized query to search for branches
                string query = @"SELECT b.id, b.branch_name AS BranchName, 
                                CONCAT(e.Given_Name, ' ', e.Family_Name) AS ManagerName, 
                                b.manager_started_at AS ManagerStartedAt 
                         FROM branches b
                         LEFT JOIN employees e ON b.manager_id = e.id
                         WHERE LOWER(b.branch_name) LIKE @Search 
                            OR LOWER(CONCAT(e.Given_Name, ' ', e.Family_Name)) LIKE @Search";

                // Pass the search parameter to prevent SQL injection
                MySqlParameter searchParam = new MySqlParameter("@Search", $"%{searchQuery.ToLower()}%");

                MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query, searchParam);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Bind the filtered results to the DataGrid
                BranchDataGrid.ItemsSource = dt.DefaultView;

                // Optionally handle the case where no results are found
                if (dt.Rows.Count == 0)
                {
                    BranchDataGrid.ItemsSource = null; // Clear the grid
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while searching: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
