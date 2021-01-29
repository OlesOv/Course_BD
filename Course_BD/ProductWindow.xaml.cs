using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Course_BD
{
    /// <summary>
    /// Interaction logic for ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow
    {
        private readonly MainWindow _mParent;
        private List<int> categoryID;
        private List<string> category;
        private List<int> brandID;
        private List<string> brand;

        public ProductWindow(MainWindow parent)
        {
            _mParent = parent;
            InitializeComponent();
            LoadBrands();
            LoadCategories();
        }

        public void LoadCategories()
        {
            category = new List<string>();
            categoryID = new List<int>();
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    var command = new SQLiteCommand($"SELECT * FROM ProductCategory", connect);
                    var sqlreader = command.ExecuteReader();
                    while (sqlreader.Read())
                    {
                        categoryID.Add(sqlreader.GetInt32(0));
                        category.Add(sqlreader.GetString(1));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            CategoryCB.ItemsSource = category;
        }

        public void LoadBrands()
        {
            brand = new List<string>();
            brandID = new List<int>();
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    var command = new SQLiteCommand($"SELECT * FROM Brand", connect);
                    var sqlreader = command.ExecuteReader();
                    while (sqlreader.Read())
                    {
                        brandID.Add(sqlreader.GetInt32(0));
                        brand.Add(sqlreader.GetString(1));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            BrandCB.ItemsSource = brand;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var p = new ProductCategoryWindow(this);
            Hide();
            p.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var p = new BrandWindow(this);
            Hide();
            p.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var p = true;
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    if (CategoryCB.SelectedIndex >= 0 && BrandCB.SelectedIndex >= 0)
                    {
                        var updateSql = new SQLiteCommand(
                            $"INSERT INTO Product (ID, Name, UPCEAN, CategoryID, BrandID) VALUES ((SELECT MAX(ID)+1 FROM Product), '{NameBox.Text}', '{UpceanBox.Text}', {categoryID[CategoryCB.SelectedIndex]}, {brandID[BrandCB.SelectedIndex]})",
                            connect);
                        updateSql.ExecuteNonQuery();
                    }
                    else
                    {
                        MessageBox.Show("Заповніть усі поля");
                        p = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            if (p)
            {
                MessageBox.Show("Готово!");
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mParent.LoadWarehouseList();
            _mParent.Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}