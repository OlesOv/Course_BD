using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SQLite;
using Microsoft.Win32;
using System.IO;

namespace Course_BD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<Product> _productList;
        private List<string> _products;
        private List<WhProduct> _whProductList;
        private List<WhProduct> _filteredWhProductList;
        private List<string> _categories;
        public List<Recipe> FilteredRecipesList;
        private List<Category> _filteredCategoriesList;

        public MainWindow()
        {
            var recipesList = Controller.GetRecipes("");
            FilteredRecipesList = recipesList;
            var categoriesList = Controller.GetCategories();
            _filteredCategoriesList = categoriesList;
            InitializeComponent();
            LoadCategoriesList();
            CategoryCb.ItemsSource = _categories;
        }

        public void LoadRecipes()
        {
            RecipesStack.Children.Clear();
            foreach (var p in FilteredRecipesList)
            {
                var t = new Button
                {
                    Content = p.Name + " (" + p.Time + " хв)",
                    Name = "rec" + Convert.ToString(p.Id)
                };
                t.Click += (s, e) =>
                {
                    var r = new RecipeWindow(this, p.Id);
                    r.Show();
                    this.Hide();
                };
                RecipesStack.Children.Add(t);
            }
        }

        public void LoadCategories()
        {
            _filteredCategoriesList = Controller.SearchCategories(SearchCategoryTextBox.Text);
            CategoriesStack.Children.Clear();
            foreach (var p in _filteredCategoriesList)
            {
                var t = new Button
                {
                    Content = p.Name
                };
                t.Click += (s, e) =>
                {
                    var с = new CategoryWindow(this, p.Name);
                    с.Show();
                    this.Hide();
                };
                CategoriesStack.Children.Add(t);
            }
        }

        public void LoadCategoriesList()
        {
            List<string> res = new List<string>();
            var t = Controller.GetCategories();
            foreach (var p in t)
            {
                res.Add(p.Name);
            }

            _categories = res;
            CategoryCb.ItemsSource = _categories;
        }

        public void LoadRecipesNow()
        {
            FilteredRecipesList = Controller.GetRecipesNow();
            RecipesNowStack.Children.Clear();
            foreach (var p in FilteredRecipesList)
            {
                var t = new Button
                {
                    Content = p.Name + " (" + p.Time + " хв)",
                    Name = "rec" + Convert.ToString(p.Id)
                };
                t.Click += (s, e) =>
                {
                    var r = new RecipeWindow(this, p.Id);
                    r.Show();
                    this.Hide();
                };
                RecipesNowStack.Children.Add(t);
            }
        }

        private void Selectbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}");
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText = QueryBox.Text
                };
                SQLiteDataReader reader = command.ExecuteReader();
                var selectResult = new List<string>();
                while (reader.Read())
                {
                    var t = "";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        t += reader.GetValue(i) + ", ";
                    }

                    selectResult.Add(t);
                }

                ResultTBlock.Text = string.Join("\n", selectResult);
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void NonSelectbtn_Click(object sender, RoutedEventArgs e)
        {
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}");
            connect.Open();
            try
            {
                var updateSql = new SQLiteCommand(QueryBox.Text, connect);
                ResultTBlock.Text = $"Affected {updateSql.ExecuteNonQuery()} rows";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void NewRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            var p = new RecipeWindow(this, -1);
            p.Show();
            this.Hide();
        }

        private void RecipesTab_Selected(object sender, RoutedEventArgs e)
        {
            FilteredRecipesList = Controller.GetRecipes("");
            CategoryCb.SelectedIndex = -1;
            LoadRecipes();
        }

        private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var p = new CategoryWindow(this, "");
            p.Show();
            this.Hide();
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilteredRecipesList = Controller.SearchRecipes(SearchTermTextBox.Text);
            LoadRecipes();
        }

        private void CategoryCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilteredRecipesList = CategoryCb.SelectedIndex != -1
                ? Controller.GetRecipes(CategoryCb.SelectedItem.ToString())
                : Controller.GetRecipes("");
            LoadRecipes();
        }

        private void CategoriesTab_Selected(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        private void SearchCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadCategories();
        }

        private void WarehouseTab_Selected(object sender, RoutedEventArgs e)
        {
            LoadWarehouseList();
            ProductColumn.ItemsSource = _products;
            WhDataGrid.DataContext = _filteredWhProductList;
        }

        public void LoadWarehouseList()
        {
            _whProductList = Controller.GetWhProducts();
            _filteredWhProductList = _whProductList;
            _productList = Controller.GetProducts();
            foreach (var p in _filteredWhProductList)
            {
                for (int i = 0; i < _productList.Count; i++)
                {
                    if (p.Id == _productList[i].Id) p.Index = i;
                }
            }

            _products = new List<string>();
            foreach (var p in _productList)
            {
                _products.Add(p.Name);
            }
        }

        private void SearchWarehouseTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = new List<WhProduct>();
            foreach (var p in _whProductList)
            {
                if (p.Name.ToLower().IndexOf(SearchWarehouseTextBox.Text.ToLower(), StringComparison.Ordinal) >=
                    0) t.Add(p);
            }

            _filteredWhProductList = t;
            WhDataGrid.DataContext = _filteredWhProductList;
        }

        private void RecipesNowTab_Selected(object sender, RoutedEventArgs e)
        {
            LoadRecipesNow();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText = "DELETE FROM Warehouse"
                };
                command.ExecuteNonQuery();
                foreach (var p in _filteredWhProductList)
                {
                    var index = 0;
                    for (int i = 0; i < _products.Count; i++)
                    {
                        if (p.Name == _products[i]) index = i;
                    }

                    command = new SQLiteCommand
                    {
                        Connection = connect,
                        CommandText =
                            $"INSERT INTO Warehouse (ProductID, Amount) VALUES ({_productList[index].Id}, {p.Amount})"
                    };
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Збережено!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText =
                        "SELECT Name, ID, s.SA FROM (SELECT ProductID, (Amount * s.CNT) SA FROM  Ingredient INNER JOIN (SELECT RecipeID, COUNT(RecipeID) CNT FROM Statistics WHERE Time >= Date('now', '-1 month') GROUP BY RecipeID) s ON s.RecipeID = Ingredient.RecipeID) s LEFT JOIN Product ON ProductID = Product.ID;"
                };
                SQLiteDataReader sqlReader = command.ExecuteReader();
                while (sqlReader.Read())
                {
                    message += sqlReader.GetString(0) + $" — {sqlReader.GetDouble(2)}\n";
                }

                connect.Close();
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "Текстові файли (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,
                Title = "Продукти за місяць",
                FileName = "Продукти за місяць"
            };

            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName.Length > 0) File.WriteAllText(saveFileDialog1.FileName, message);
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "Текстові файли (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,
                Title = "Статистика рецептів",
                FileName = "Статистика рецептів"
            };

            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName.Length > 0)
                File.WriteAllText(saveFileDialog1.FileName, Controller.GetStatistics());
        }
    }
}