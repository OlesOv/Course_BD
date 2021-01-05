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
    public partial class MainWindow : Window
    {
        private List<Product> productList;
        private List<string> products;
        private List<WHProduct> WHProductList;
        private List<WHProduct> filteredWHProductList;
        private List<Recipe> recipesList;
        private List<string> categories;
        public List<Recipe> filteredRecipesList;
        private List<Category> categoriesList;
        private List<Category> filteredCategoriesList;
        public MainWindow()
        {
            recipesList = Controller.GetRecipes("");
            filteredRecipesList = recipesList;
            categoriesList = Controller.GetCategories();
            filteredCategoriesList = categoriesList;
            InitializeComponent();
            LoadCategoriesList();
            CategoryCB.ItemsSource = categories;
        }
       
        public void LoadRecipes()
        {
            RecipesStack.Children.Clear();
            foreach(var p in filteredRecipesList)
            {
                var t = new Button
                {
                    Content = p.Name + " (" + p.Time + " хв)",
                    Name = "rec" + Convert.ToString(p.ID)
                };
                t.Click += (s, e) =>
                {
                    var r = new RecipeWindow(this, p.ID);
                    r.Show();
                    this.Hide();
                };
                RecipesStack.Children.Add(t);
            }
        }
        public void LoadCategories()
        {
            filteredCategoriesList = Controller.SearchCategories(SearchCategoryTextBox.Text);
            CategoriesStack.Children.Clear();
            foreach (var p in filteredCategoriesList)
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
            foreach(var p in t)
            {
                res.Add(p.Name);
            }
            categories = res;
            CategoryCB.ItemsSource = categories;
        }
        public void LoadRecipesNow()
        {
            filteredRecipesList = Controller.GetRecipesNow();
            RecipesNowStack.Children.Clear();
            foreach (var p in filteredRecipesList)
            {
                var t = new Button
                {
                    Content = p.Name + " (" + p.Time + " хв)",
                    Name = "rec" + Convert.ToString(p.ID)
                };
                t.Click += (s, e) =>
                {
                    var r = new RecipeWindow(this, p.ID);
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
                using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
                {
                    Connect.Open();
                    SQLiteCommand Command = new SQLiteCommand
                    {
                        Connection = Connect,
                        CommandText = QueryBox.Text
                    };
                    SQLiteDataReader reader = Command.ExecuteReader();
                    var selectResult = new List<string>();
                    while (reader.Read())
                    {
                        var t = "";
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            t += reader.GetValue(i).ToString() + ", ";
                        }
                        selectResult.Add(t);
                    }
                    ResultTBlock.Text = string.Join("\n", selectResult);
                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void NonSelectbtn_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                try
                {
                    var updateSql = new SQLiteCommand(QueryBox.Text, Connect);
                    ResultTBlock.Text = $"Affected {updateSql.ExecuteNonQuery()} rows";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
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
            filteredRecipesList = Controller.GetRecipes("");
            CategoryCB.SelectedIndex = -1;
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
            filteredRecipesList = Controller.SearchRecipes(SearchTermTextBox.Text);
            LoadRecipes();
        }
        private void CategoryCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filteredRecipesList = CategoryCB.SelectedIndex != -1 ? Controller.GetRecipes(CategoryCB.SelectedItem.ToString()) : Controller.GetRecipes("");
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
            ProductColumn.ItemsSource = products;
            WHDataGrid.DataContext = filteredWHProductList;
        }
        public void LoadWarehouseList()
        {
            WHProductList = Controller.GetWHProducts();
            filteredWHProductList = WHProductList;
            productList = Controller.GetProducts();
            foreach (var p in filteredWHProductList)
            {
                for (int i = 0; i < productList.Count; i++)
                {
                    if (p.ID == productList[i].ID) p.Index = i;
                }
            }
            products = new List<string>();
            foreach(var p in productList)
            {
                products.Add(p.Name);
            }
        }
        private void SearchWarehouseTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = new List<WHProduct>();
            foreach(var p in WHProductList)
            {
                if (p.Name.ToLower().IndexOf(SearchWarehouseTextBox.Text.ToLower()) >= 0) t.Add(p);
            }
            filteredWHProductList = t;
            WHDataGrid.DataContext = filteredWHProductList;
        }

        private void RecipesNowTab_Selected(object sender, RoutedEventArgs e)
        {
            LoadRecipesNow();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"DELETE FROM Warehouse"
                };
                Command.ExecuteNonQuery();
                foreach(var p in filteredWHProductList)
                {
                    var index = 0;
                    for(int i = 0; i < products.Count; i++)
                    {
                        if (p.Name == products[i]) index = i;
                    }
                    Command = new SQLiteCommand
                    {
                        Connection = Connect,
                        CommandText = $"INSERT INTO Warehouse (ProductID, Amount) VALUES ({productList[index].ID}, {p.Amount})"
                    };
                    Command.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Збережено!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT Name, ID, s.SA FROM (SELECT ProductID, (Amount * s.CNT) SA FROM  Ingredient INNER JOIN (SELECT RecipeID, COUNT(RecipeID) CNT FROM Statistics WHERE Time >= Date('now', '-1 month') GROUP BY RecipeID) s ON s.RecipeID = Ingredient.RecipeID) s LEFT JOIN Product ON ProductID = Product.ID;"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    message += sqlReader.GetString(0) + $" — {sqlReader.GetDouble(2)}\n";
                }
                Connect.Close();
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Текстові файли (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Продукти за місяць";
            saveFileDialog1.FileName ="Продукти за місяць";
            saveFileDialog1.ShowDialog();
            if(saveFileDialog1.FileName.Length > 0) File.WriteAllText(saveFileDialog1.FileName, message);
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Текстові файли (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Статистика рецептів";
            saveFileDialog1.FileName ="Статистика рецептів";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName.Length > 0) File.WriteAllText(saveFileDialog1.FileName, Controller.GetStatistics());
        }
    }
}
