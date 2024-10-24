using System;
using System.Data.SqlClient;
using System.Windows;
using OnlineStoreApp.Models;
namespace OnlineStoreApp.Views
{
    public partial class LoginForm : Window
    {
        private SqlConnection conn = new SqlConnection();
        private string conString = "Server=(local);Database=OnlineStoreDB;User=S24Gurleen;Password=1234";
        private SqlCommand cmd = new SqlCommand();

        public User AuthenticatedUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            conn.ConnectionString = conString;
            cmd = conn.CreateCommand();

            try
            {
                string query = "SELECT Users.*, Roles.RoleName FROM Users " +
                               "JOIN Roles ON Users.RoleID = Roles.RoleID " +
                               "WHERE Username = @Username AND Password = @Password";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    AuthenticatedUser = new User
                    {
                        UserID = (int)reader["UserID"],
                        Username = reader["Username"].ToString(),
                        RoleID = (int)reader["RoleID"],
                        RoleName = reader["RoleName"].ToString()
                    };

                    this.Hide();

                    MainWindow mainWindow = new MainWindow(AuthenticatedUser);
                    mainWindow.ShowDialog();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
            }
        }
    }
}