using System;
using System.Data.SqlClient;
using System.Windows;
using System.Xml.Linq;

namespace Project
{
    public partial class RegistrationForm : Window
    {
        private SqlConnection conn;
        private string conString = "Server=(local);Database=OnlineStoreDB;User=S24Gurleen;Password=1234";
        private SqlCommand cmd;

        public RegistrationForm()
        {
            InitializeComponent();
            conn = new SqlConnection(conString);
        }

        private void cmbRole_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedRole = cmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem;
            if (selectedRole != null && selectedRole.Content.ToString() == "PremiumUser")
            {
                lblPaymentMethod.Visibility = Visibility.Visible;
                tbPaymentMethod.Visibility = Visibility.Visible;
            }
            else
            {
                lblPaymentMethod.Visibility = Visibility.Collapsed;
                tbPaymentMethod.Visibility = Visibility.Collapsed;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = pbPassword.Password;
            string email = tbEmail.Text;
            string name = tbName.Text;
            string phoneNumber = tbPhoneNumber.Text;
            string role = cmbRole.SelectedItem != null ? (cmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem).Content.ToString() : null;
            string paymentMethod = tbPaymentMethod.Text;

            // Validate input fields
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (role == "PremiumUser" && string.IsNullOrEmpty(paymentMethod))
            {
                MessageBox.Show("Please provide a payment method for Premium users.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            cmd = conn.CreateCommand();

            try
            {
                // SQL query to insert user data into the Users table
                string query = @"
                    INSERT INTO Users (Username, Password, Email, Name, PhoneNumber, RoleID, PaymentMethod)
                    VALUES (@Username, @Password, @Email, @Name, @PhoneNumber, 
                    (SELECT RoleID FROM Roles WHERE RoleName = @RoleName), @PaymentMethod)";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrEmpty(phoneNumber) ? DBNull.Value : (object)phoneNumber);
                cmd.Parameters.AddWithValue("@RoleName", role);
                object paymentValue = role == "PremiumUser" ? (object)paymentMethod : DBNull.Value;
                cmd.Parameters.AddWithValue("@PaymentMethod", paymentValue);

                conn.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();  // Close the registration form after successful registration
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
