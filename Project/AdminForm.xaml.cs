using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using OnlineStoreApp.Models;

namespace OnlineStoreApp.Views
{
    public partial class AdminForm : Window
    {
        private SqlConnection conn;
        private string conString = "Server=(local);Database=OnlineStoreDB;User=S24Gurleen;Password=1234";
        private SqlCommand cmd;

        private User currentUser;

        public AdminForm(User user)
        {
            currentUser = user;
            InitializeComponent();
            conn = new SqlConnection(conString);
            LoadProducts();
            LoadCategories();
        }

        private void LoadProducts()
        {
            cmd = conn.CreateCommand();

            try
            {
                string query = "SELECT * FROM Products";
                cmd.CommandText = query;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                DataTable dataTable = new DataTable();
                dataTable.Load(reader);

                dataGridViewProducts.ItemsSource = dataTable.DefaultView;

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

        private void LoadCategories()
        {
            cmd = conn.CreateCommand();

            try
            {
                string query = "SELECT DISTINCT Category FROM Products";
                cmd.CommandText = query;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                cmbCategory.Items.Clear();  // Clear existing items, if any
                while (reader.Read())
                {
                    cmbCategory.Items.Add(reader["Category"].ToString());
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = tbName.Text;
            string description = tbDescription.Text;
            string category = cmbCategory.SelectedItem != null ? cmbCategory.SelectedItem.ToString() : null;
            decimal price;

            if (!decimal.TryParse(tbPrice.Text, out price) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category))
            {
                MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            cmd = conn.CreateCommand();

            try
            {
                // Check if the product already exists
                string checkQuery = "SELECT COUNT(*) FROM Products WHERE Name = @Name AND Category = @Category";
                cmd.CommandText = checkQuery;
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Category", category);

                conn.Open();
                int productExists = (int)cmd.ExecuteScalar();

                if (productExists > 0)
                {
                    MessageBox.Show("This product already exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Insert the new product
                string insertQuery = "INSERT INTO Products (Name, Description, Price, Category) VALUES (@Name, @Description, @Price, @Category)";
                cmd.CommandText = insertQuery;
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@Price", price);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                LoadProducts();
                LoadCategories();  // Refresh categories after adding a product
            }
        }


        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            int productId;
            if (!int.TryParse(tbProductID.Text, out productId))
            {
                MessageBox.Show("Please enter a valid Product ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string name = tbName.Text;
            string description = tbDescription.Text;
            string category = cmbCategory.SelectedItem != null ? cmbCategory.SelectedItem.ToString() : null;
            decimal price;

            if (!decimal.TryParse(tbPrice.Text, out price) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category))
            {
                MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            cmd = conn.CreateCommand();

            try
            {
                string query = "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, Category = @Category WHERE ProductID = @ProductID";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Category", category);

                conn.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                LoadProducts();
                LoadCategories();  // Refresh categories after updating a product
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int productId;
            if (!int.TryParse(tbProductID.Text, out productId))
            {
                MessageBox.Show("Please enter a valid Product ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            cmd = conn.CreateCommand();

            try
            {
                string query = "DELETE FROM Products WHERE ProductID = @ProductID";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@ProductID", productId);

                conn.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                LoadProducts();
                LoadCategories();  // Refresh categories after deleting a product
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadCategories();  // Refresh categories when the Refresh button is clicked
        }
    }
}
