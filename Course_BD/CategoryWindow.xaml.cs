using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CategoryWindow.xaml
    /// </summary>
    public partial class CategoryWindow : Window
    {
        private readonly MainWindow mParent;
        private readonly string CategoryName;
        private List<int> recipeId;
        private List<string> recipes;
        private readonly List<ComboBox> recipeCBs = new List<ComboBox>();
        private static readonly Regex Regex = new Regex("[^0-9.]");
        public CategoryWindow(MainWindow parent, string categoryName)
        {
            mParent = parent;
            CategoryName = categoryName;
            InitializeComponent();
            LoadAllRecipes();
            recipeCBs.Add(RecipeCBox);
            RecipeCBox.ItemsSource = recipes;
            if(categoryName != "") LoadCategory();
            MinusRecipeButton.IsEnabled = RecipeWrap.Children.Count > 3;
        }

        private void LoadCategory()
        {
            using SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}");
            Connect.Open();
            SQLiteCommand Command = new SQLiteCommand
            {
                Connection = Connect,
                CommandText = $"SELECT * FROM Category WHERE Name = '{CategoryName}'"
            };
            SQLiteDataReader sqlReader = Command.ExecuteReader();
            List<string> res = new List<string>();
            while (sqlReader.Read())
            {
                NameBox.Text = sqlReader.GetString(0);
                DescriptionBox.Text = sqlReader.GetString(1);
            }
            Command = new SQLiteCommand
            {
                Connection = Connect,
                CommandText = $"SELECT Recipe.RecipeID, Recipe.Name FROM Recipe LEFT JOIN WhereRecipeBelongs ON WhereRecipeBelongs.RecipeID = REcipe.RecipeID WHERE WhereRecipeBelongs.CategoryName = '{CategoryName}'"
            };
            sqlReader = Command.ExecuteReader();
            var p = false;
            while (sqlReader.Read())
            {
                int index = -1;
                for (int i = 0; i < recipeId.Count; i++)
                {
                    if (sqlReader.GetInt32(0) == recipeId[i])
                    {
                        index = i;
                        break;
                    }
                }
                if (p)
                {
                    var comboBox = new ComboBox
                    {
                        ItemsSource = recipes,
                        Width = 250,
                        Height = 28,
                        FontSize = 16,
                        Margin = new Thickness(10, 10, 10, 10),
                        SelectedIndex = index
                    };
                    recipeCBs.Add(comboBox);

                    RecipeWrap.Children.Add(comboBox);
                }
                else
                {
                    p = true;
                    recipeCBs[0].SelectedIndex = index;
                }

            }
            Connect.Close();
        }

        private void LoadAllRecipes()
        {
            recipes = new List<string>();
            recipeId = new List<int>();
            var t = Controller.GetRecipes("");
            foreach(var p in t)
            {
                recipes.Add(p.Name);
                recipeId.Add(p.ID);
            }
        }

        private void PlusRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            var comboBox = new ComboBox
            {
                ItemsSource = recipes,
                Width = 250,
                Height = 28,
                FontSize = 16,
                Margin = new Thickness(10, 10, 10, 10)
            };
            recipeCBs.Add(comboBox);
            RecipeWrap.Children.Add(comboBox);
            if (RecipeWrap.Children.Count > 3) MinusRecipeButton.IsEnabled = true;
        }

        private void MinusRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeWrap.Children.Remove(RecipeWrap.Children[^1]);
            recipeCBs.Remove(recipeCBs[^1]);
            if (RecipeWrap.Children.Count == 3) MinusRecipeButton.IsEnabled = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"DELETE FROM Category WHERE Name = '{CategoryName}'"
                };
                Command.ExecuteNonQuery();
            }
            MessageBox.Show("Видалено!");
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                try
                {
                    var updateSql = CategoryName == "" ? new SQLiteCommand($"INSERT INTO Category (Name, Description) VALUES ('{NameBox.Text}', '{DescriptionBox.Text}')", Connect) : new SQLiteCommand($"UPDATE Category SET Name = '{NameBox.Text}', Description = '{DescriptionBox.Text}' WHERE Name = '{CategoryName}'", Connect);
                    updateSql.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                var deleteSql = new SQLiteCommand($"DELETE FROM WhereRecipeBelongs WHERE CategoryName = '{CategoryName}'", Connect);
                deleteSql.ExecuteNonQuery();
                for (int i = 0; i < recipeCBs.Count; i++)
                {
                    try
                    {
                        if (recipeCBs[i].SelectedIndex > -1)
                        {
                            var updateSql = new SQLiteCommand($"INSERT INTO WhereRecipeBelongs (RecipeID, CategoryName) VALUES ({recipeId[recipeCBs[i].SelectedIndex]}, '{CategoryName}')", Connect);
                            updateSql.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            MessageBox.Show("Готово!");
            this.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            mParent.LoadCategoriesList();
            mParent.LoadCategories();
            mParent.Show();
        }
    }
}