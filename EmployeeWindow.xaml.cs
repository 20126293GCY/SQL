using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace CompanyManagementApp
{
    public partial class EmployeeWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public EmployeeWindow()
        {
            InitializeComponent();
            LoadEmployeeData();
            LoadBranchData();
            LoadSupervisorData();
        }

        private void LoadEmployeeData()
        {
            string query = "SELECT e.ID, e.Given_Name, e.Family_Name, e.Date_Of_Birth, e.Gender_Identity, e.Gross_Salary, e.Supervisor_Id, e.Branch_Id, CONCAT(s.Given_Name, ' ', s.Family_Name) AS SupervisorName " +
                           "FROM employees e " +
                           "LEFT JOIN employees s ON e.Supervisor_Id = s.ID";

            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            EmployeeDataGrid.ItemsSource = dt.DefaultView;
        }

        private void LoadBranchData()
        {
            string query = "SELECT id, branch_name FROM branches";
            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            BranchComboBox.ItemsSource = dt.DefaultView;
        }

        private void LoadSupervisorData()
        {
            string query = "SELECT ID, CONCAT(Given_Name, ' ', Family_Name) AS FullName FROM employees";
            MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            SupervisorComboBox.ItemsSource = dt.DefaultView;
        }

        private void EmployeeDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)EmployeeDataGrid.SelectedItem;
                GivenNameTextBox.Text = row["Given_Name"].ToString();
                FamilyNameTextBox.Text = row["Family_Name"].ToString();
                DateOfBirthPicker.SelectedDate = DateTime.Parse(row["Date_Of_Birth"].ToString());
                GenderComboBox.Text = row["Gender_Identity"].ToString();
                SalaryTextBox.Text = row["Gross_Salary"].ToString();
                BranchComboBox.SelectedValue = row["Branch_Id"];
                SupervisorComboBox.SelectedValue = row["Supervisor_Id"];
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            string givenName = GivenNameTextBox.Text;
            string familyName = FamilyNameTextBox.Text;
            int grossSalary = int.Parse(SalaryTextBox.Text);
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime dateOfBirth = DateOfBirthPicker.SelectedDate ?? DateTime.Now;
            int branchId = Convert.ToInt32(BranchComboBox.SelectedValue);
            int supervisorId = SupervisorComboBox.SelectedValue != null ? Convert.ToInt32(SupervisorComboBox.SelectedValue) : 0;

            string query = $"INSERT INTO employees (Given_Name, Family_Name, Date_Of_Birth, Gender_Identity, Gross_Salary, Branch_Id, Supervisor_Id) " +
                           $"VALUES ('{givenName}', '{familyName}', '{dateOfBirth:yyyy-MM-dd}', '{gender}', {grossSalary}, {branchId}, {supervisorId})";
            dbHelper.ExecuteNonQuery(query);
            LoadEmployeeData();
            ClearFields(); // Clear fields after insertion
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)EmployeeDataGrid.SelectedItem;
                int employeeId = Convert.ToInt32(row["ID"]);

                string givenName = GivenNameTextBox.Text;
                string familyName = FamilyNameTextBox.Text;
                int grossSalary = int.Parse(SalaryTextBox.Text);
                string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                DateTime dateOfBirth = DateOfBirthPicker.SelectedDate ?? DateTime.Now;
                int branchId = Convert.ToInt32(BranchComboBox.SelectedValue);
                int supervisorId = SupervisorComboBox.SelectedValue != null ? Convert.ToInt32(SupervisorComboBox.SelectedValue) : 0;

                string query = $"UPDATE employees SET Given_Name = '{givenName}', Family_Name = '{familyName}', Date_Of_Birth = '{dateOfBirth:yyyy-MM-dd}', " +
                               $"Gender_Identity = '{gender}', Gross_Salary = {grossSalary}, Branch_Id = {branchId}, Supervisor_Id = {supervisorId} " +
                               $"WHERE ID = {employeeId}";
                dbHelper.ExecuteNonQuery(query);
                LoadEmployeeData();
                ClearFields(); // Clear fields after update
            }
            else
            {
                MessageBox.Show("Please select an employee to update.");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)EmployeeDataGrid.SelectedItem;
                int employeeId = Convert.ToInt32(row["ID"]);
                string query = $"DELETE FROM employees WHERE ID = {employeeId}";
                dbHelper.ExecuteNonQuery(query);
                LoadEmployeeData();
                ClearFields(); // Clear fields after deletion
            }
            else
            {
                MessageBox.Show("Please select an employee to delete.");
            }
        }

        private void ClearFields()
        {
            GivenNameTextBox.Text = string.Empty;
            FamilyNameTextBox.Text = string.Empty;
            DateOfBirthPicker.SelectedDate = null;
            GenderComboBox.SelectedIndex = -1;
            SalaryTextBox.Text = string.Empty;
            BranchComboBox.SelectedIndex = -1;
            SupervisorComboBox.SelectedIndex = -1;
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();

            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    // If the search query is empty, reload all data
                    LoadEmployeeData();
                    return;
                }

                // SQL query with parameterized search
                string query = @"SELECT e.ID, e.Given_Name, e.Family_Name, e.Date_Of_Birth, e.Gender_Identity, e.Gross_Salary, e.Supervisor_Id, e.Branch_Id, 
                                CONCAT(s.Given_Name, ' ', s.Family_Name) AS SupervisorName 
                         FROM employees e 
                         LEFT JOIN employees s ON e.Supervisor_Id = s.ID 
                             WHERE LOWER(e.Given_Name) LIKE @Search 
                                OR LOWER(e.Family_Name) LIKE @Search 
                                OR CAST(e.ID AS CHAR) LIKE @Search
                                OR LOWER(e.Gender_Identity) LIKE @Search 
                                OR LOWER(e.Branch_Id) LIKE @Search
                                OR CAST(e.Gross_Salary AS CHAR) LIKE @Search 
                                OR LOWER(CONCAT(s.Given_Name, ' ', s.Family_Name)) LIKE @Search";

                // Parameterized query to prevent SQL injection
                MySqlParameter searchParam = new MySqlParameter("@Search", $"%{searchQuery.ToLower()}%");

                // Execute the query and bind the results to the DataGrid
                MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query, searchParam);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                EmployeeDataGrid.ItemsSource = dt.DefaultView;

                // Show message if no results are found
                if (dt.Rows.Count == 0)
                {
                    // Optionally, clear the DataGrid or display an empty result message
                    EmployeeDataGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                // Display error message if something goes wrong
                MessageBox.Show($"An error occurred while searching: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse minimum and maximum salary values
                if (decimal.TryParse(MinSalaryTextBox.Text.Trim(), out decimal minGross_Salary) &&
                    decimal.TryParse(MaxSalaryTextBox.Text.Trim(), out decimal maxGross_Salary))
                {
                    if (minGross_Salary > maxGross_Salary)
                    {
                        MessageBox.Show("Minimum salary cannot be greater than maximum salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Query to filter employees by salary range
                    string query = @"SELECT ID, 
                                    CONCAT(Given_Name, ' ', Family_Name) AS FullName, 
                                    Gross_Salary 
                             FROM employees 
                             WHERE Gross_Salary BETWEEN @MinGross_Salary AND @MaxGross_Salary";

                    // Add parameters to prevent SQL injection
                    MySqlParameter[] parameters = new MySqlParameter[]
                    {
                new MySqlParameter("@MinGross_Salary", minGross_Salary),
                new MySqlParameter("@MaxGross_Salary", maxGross_Salary)
                    };

                    MySqlDataAdapter adapter = dbHelper.ExecuteQuery(query, parameters);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Bind results to the DataGrid
                    EmployeeDataGrid.ItemsSource = dt.DefaultView;

                    // Handle no results case
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No employees found within the specified salary range.", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                        EmployeeDataGrid.ItemsSource = null; // Clear the grid
                    }
                }
                else
                {
                    MessageBox.Show("Please enter valid numeric values for both minimum and maximum salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filtering: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
