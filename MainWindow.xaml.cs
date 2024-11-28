using System.Windows;

namespace CompanyManagementApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenEmployeeModule_Click(object sender, RoutedEventArgs e)
        {
            EmployeeWindow employeeWindow = new EmployeeWindow();
            employeeWindow.Show();
        }

        private void OpenBranchModule_Click(object sender, RoutedEventArgs e)
        {
            BranchWindow branchWindow = new BranchWindow();
            branchWindow.Show();
        }

        private void OpenWorksWithModule_Click(object sender, RoutedEventArgs e)
        {
            WorksWithWindow worksWithWindow = new WorksWithWindow();
            worksWithWindow.Show();
        }

        private void OpenClientModule_Click(object sender, RoutedEventArgs e)
        {
            ClientsWindow clientsWindow = new ClientsWindow();  
            clientsWindow.Show();   
        }
    }
}
