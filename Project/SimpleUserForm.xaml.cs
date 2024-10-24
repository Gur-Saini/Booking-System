using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OnlineStoreApp.Models;

namespace OnlineStoreApp.Views
{
    public partial class SimpleUserForm : Window
    {
        private SqlConnection conn = new SqlConnection();
        private string conString = "Server=(local);Database=OnlineStoreDB;User=S24Gurleen;Password=1234";
        private SqlCommand cmd = new SqlCommand();

        private User currentUser;

        public SimpleUserForm(User user)
        {
            currentUser = user;
            InitializeComponent();
            LoadProductsAsync();
        }

        private async void LoadProductsAsync()
        {
            using (SqlConnection conn = new SqlConnection(conString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                try
                {
                    string query = @"
                        SELECT 
                            ProductID, 
                            Name, 
                            Description, 
                            Price,
                            Category
                        FROM Products";

                    cmd.CommandText = query;

                    await conn.OpenAsync();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    dataGridViewProducts.ItemsSource = dataTable.DefaultView;

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridViewProducts.SelectedItems.Count > 0)
            {
                var selectedRow = (DataRowView)dataGridViewProducts.SelectedItem;

                if (selectedRow["ProductID"] != DBNull.Value &&
                    selectedRow["Price"] != DBNull.Value &&
                    !string.IsNullOrEmpty(tbQuantity.Text))
                {
                    int productId = Convert.ToInt32(selectedRow["ProductID"]);
                    decimal price = Convert.ToDecimal(selectedRow["Price"]);
                    int quantity;

                    if (int.TryParse(tbQuantity.Text, out quantity) && quantity > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(conString))
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            try
                            {
                                await conn.OpenAsync();

                                // Check if the product already exists in the cart
                                string query = "SELECT Quantity FROM Cart WHERE UserID = @UserID AND ProductID = @ProductID";
                                cmd.CommandText = query;
                                cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);
                                cmd.Parameters.AddWithValue("@ProductID", productId);

                                object result = await cmd.ExecuteScalarAsync();

                                if (result != null)
                                {
                                    // Product exists in the cart, update the quantity
                                    int existingQuantity = Convert.ToInt32(result);
                                    query = "UPDATE Cart SET Quantity = @Quantity WHERE UserID = @UserID AND ProductID = @ProductID";
                                    cmd.CommandText = query;
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@Quantity", existingQuantity + quantity);
                                    cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                }
                                else
                                {
                                    // Product does not exist in the cart, insert a new entry
                                    query = "INSERT INTO Cart (UserID, ProductID, Quantity, Price) VALUES (@UserID, @ProductID, @Quantity, @Price)";
                                    cmd.CommandText = query;
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                                    cmd.Parameters.AddWithValue("@Price", price);
                                }

                                await cmd.ExecuteNonQueryAsync();

                                MessageBox.Show("Product added to cart successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            finally
                            {
                                await LoadCartAsync(); // Refresh the cart to reflect the updated quantity
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Selected product or quantity is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnViewCart_Click(object sender, RoutedEventArgs e)
        {
            await LoadCartAsync();
        }

        private async Task LoadCartAsync()
        {
            using (SqlConnection conn = new SqlConnection(conString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                try
                {
                    string query = "SELECT Cart.CartID, Cart.ProductID, Products.Name, Cart.Quantity, Cart.Price FROM Cart " +
                                   "JOIN Products ON Cart.ProductID = Products.ProductID " +
                                   "WHERE Cart.UserID = @UserID";
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);

                    await conn.OpenAsync();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    DataTable cartTable = new DataTable();
                    cartTable.Load(reader);

                    if (cartTable.Rows.Count == 0)
                    {
                        dataGridViewCart.ItemsSource = null;
                        MessageBox.Show("Your cart is empty.", "Cart", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        dataGridViewCart.ItemsSource = cartTable.DefaultView;
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void tbQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            DataRowView row = (DataRowView)dataGridViewCart.SelectedItem;

            if (row != null && int.TryParse(textBox.Text, out int newQuantity) && newQuantity > 0)
            {
                int cartId = (int)row["CartID"];

                using (SqlConnection conn = new SqlConnection(conString))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        string query = "UPDATE Cart SET Quantity = @Quantity WHERE CartID = @CartID";
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                        cmd.Parameters.AddWithValue("@CartID", cartId);

                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();

                        MessageBox.Show("Quantity updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        await LoadCartAsync(); // Refresh the cart to reflect the updated quantity
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid quantity entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int cartId = Convert.ToInt32(button.CommandParameter);

            using (SqlConnection conn = new SqlConnection(conString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Cart WHERE CartID = @CartID";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@CartID", cartId);

                    await cmd.ExecuteNonQueryAsync();

                    MessageBox.Show("Item removed from cart.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadCartAsync(); // Refresh the cart view
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(conString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Cart WHERE UserID = @UserID";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    DataTable cartTable = new DataTable();
                    cartTable.Load(reader);

                    if (cartTable.Rows.Count == 0)
                    {
                        MessageBox.Show("Your cart is empty.", "Checkout", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    decimal totalAmount = 0;
                    foreach (DataRow row in cartTable.Rows)
                    {
                        totalAmount += (decimal)row["Price"] * (int)row["Quantity"];
                    }

                    reader.Close();

                    // Insert order into Orders table
                    query = "INSERT INTO Orders (UserID, OrderDate, TotalAmount) OUTPUT INSERTED.OrderID VALUES (@UserID, @OrderDate, @TotalAmount)";
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);
                    cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);

                    int orderId = (int)await cmd.ExecuteScalarAsync(); // Capture the OrderID

                    // Insert order details into OrderDetails table
                    foreach (DataRow row in cartTable.Rows)
                    {
                        query = "INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price) VALUES (@OrderID, @ProductID, @Quantity, @Price)";
                        cmd.CommandText = query;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@ProductID", (int)row["ProductID"]);
                        cmd.Parameters.AddWithValue("@Quantity", (int)row["Quantity"]);
                        cmd.Parameters.AddWithValue("@Price", (decimal)row["Price"]);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Clear the cart after checkout
                    query = "DELETE FROM Cart WHERE UserID = @UserID";
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserID", currentUser.UserID);
                    await cmd.ExecuteNonQueryAsync();

                    MessageBox.Show($"Your total amount is {totalAmount:C}. Checkout successful!", "Checkout", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadCartAsync(); // Refresh the cart view to show it as empty
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
