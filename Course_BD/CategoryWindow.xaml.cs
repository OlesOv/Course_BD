using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace Course_BD
{
    /// <summary>
    /// Interaction logic for CategoryWindow.xaml
    /// </summary>
    public partial class CategoryWindow
    {
        private readonly MainWindow _mParent;
        private readonly string _categoryName;
        private List<int> _recipeId;
        private List<string> _recipes;
        private readonly List<ComboBox> _recipeCBs = new List<ComboBox>();

        public CategoryWindow(MainWindow parent, string categoryName)
        {
            _mParent = parent;
            _categoryName = categoryName;
            InitializeComponent();
            LoadAllRecipes();
            _recipeCBs.Add(RecipeCBox);
            RecipeCBox.ItemsSource = _recipes;
            if (categoryName != "") LoadCategory();
            MinusRecipeButton.IsEnabled = RecipeWrap.Children.Count > 3;
        }

        private void LoadCategory()
        {
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = $"SELECT * FROM Category WHERE Name = '{_categoryName}'"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                NameBox.Text = sqlReader.GetString(0);
                DescriptionBox.Text = sqlReader.GetString(1);
            }

            command = new SQLiteCommand
            {
                Connection = connect,
                CommandText =
                    $"SELECT Recipe.RecipeID, Recipe.Name FROM Recipe LEFT JOIN WhereRecipeBelongs ON WhereRecipeBelongs.RecipeID = REcipe.RecipeID WHERE WhereRecipeBelongs.CategoryName = '{_categoryName}'"
            };
            sqlReader = command.ExecuteReader();
            var p = false;
            while (sqlReader.Read())
            {
                int index = -1;
                for (int i = 0; i < _recipeId.Count; i++)
                {
                    if (sqlReader.GetInt32(0) == _recipeId[i])
                    {
                        index = i;
                        break;
                    }
                }

                if (p)
                {
                    var comboBox = new ComboBox
                    {
                        ItemsSource = _recipes,
                        Width = 250,
                        Height = 28,
                        FontSize = 16,
                        Margin = new Thickness(10, 10, 10, 10),
                        SelectedIndex = index
                    };
                    _recipeCBs.Add(comboBox);

                    RecipeWrap.Children.Add(comboBox);
                }
                else
                {
                    p = true;
                    _recipeCBs[0].SelectedIndex = index;
                }
            }

            connect.Close();
        }

        private void LoadAllRecipes()
        {
            _recipes = new List<string>();
            _recipeId = new List<int>();
            var t = Controller.GetRecipes("");
            foreach (var p in t)
            {
                _recipes.Add(p.Name);
                _recipeId.Add(p.Id);
            }
        }

        private void PlusRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            var comboBox = new ComboBox
            {
                ItemsSource = _recipes,
                Width = 250,
                Height = 28,
                FontSize = 16,
                Margin = new Thickness(10, 10, 10, 10)
            };
            _recipeCBs.Add(comboBox);
            RecipeWrap.Children.Add(comboBox);
            if (RecipeWrap.Children.Count > 3) MinusRecipeButton.IsEnabled = true;
        }

        private void MinusRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeWrap.Children.Remove(RecipeWrap.Children[^1]);
            _recipeCBs.Remove(_recipeCBs[^1]);
            if (RecipeWrap.Children.Count == 3) MinusRecipeButton.IsEnabled = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText = $"DELETE FROM Category WHERE Name = '{_categoryName}'"
                };
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Видалено!");
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    var updateSql = _categoryName == ""
                        ? new SQLiteCommand(
                            $"INSERT INTO Category (Name, Description) VALUES ('{NameBox.Text}', '{DescriptionBox.Text}')",
                            connect)
                        : new SQLiteCommand(
                            $"UPDATE Category SET Name = '{NameBox.Text}', Description = '{DescriptionBox.Text}' WHERE Name = '{_categoryName}'",
                            connect);
                    updateSql.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                var deleteSql =
                    new SQLiteCommand($"DELETE FROM WhereRecipeBelongs WHERE CategoryName = '{_categoryName}'",
                        connect);
                deleteSql.ExecuteNonQuery();
                foreach (var t in _recipeCBs)
                {
                    try
                    {
                        if (t.SelectedIndex > -1)
                        {
                            var updateSql =
                                new SQLiteCommand(
                                    $"INSERT INTO WhereRecipeBelongs (RecipeID, CategoryName) VALUES ({_recipeId[t.SelectedIndex]}, '{_categoryName}')",
                                    connect);
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
            _mParent.LoadCategoriesList();
            _mParent.LoadCategories();
            _mParent.Show();
        }
    }
}