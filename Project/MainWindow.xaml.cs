using System.Windows;
using OnlineStoreApp.Models;
using OnlineStoreApp.Views;

namespace OnlineStoreApp
{
    public partial class MainWindow : Window
    {
        private User currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            if (currentUser.RoleName == "Admin")
            {
                btnManageProducts.Visibility = Visibility.Visible;
                btnViewProducts.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnManageProducts.Visibility = Visibility.Collapsed;
                btnViewProducts.Visibility = Visibility.Visible;
            }
        }

        private void btnManageProducts_Click(object sender, RoutedEventArgs e)
        {
            AdminForm adminForm = new AdminForm(currentUser);
            adminForm.ShowDialog();
        }

        private void btnViewProducts_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.RoleName == "PremiumUser")
            {
                PremiumUserForm premiumUserForm = new PremiumUserForm(currentUser);
                premiumUserForm.ShowDialog();
            }
            else if (currentUser.RoleName == "SimpleUser")
            {
                SimpleUserForm simpleUserForm = new SimpleUserForm(currentUser);
                simpleUserForm.ShowDialog();
            }
        }
    }
}
