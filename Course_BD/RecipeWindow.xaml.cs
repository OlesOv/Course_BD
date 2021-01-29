using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Course_BD
{
    /// <summary>
    /// Interaction logic for RecipeWindow.xaml
    /// </summary>
    public partial class RecipeWindow
    {
        private readonly MainWindow _mParent;
        private readonly int _recipeId;
        private readonly int _realRecipeId;
        private List<int> _productId;
        private readonly List<ComboBox> _productCBs;
        private readonly List<TextBox> _amountBs;
        private static readonly Regex Regex = new Regex("[^0-9.]");
        private List<string> _productList = new List<string>();

        public RecipeWindow(MainWindow parent, int recipeId)
        {
            _mParent = parent;
            _recipeId = recipeId;
            InitializeComponent();
            _productId = new List<int>();
            LoadProducts();
            _productCBs = new List<ComboBox> {ProductCBox};
            _amountBs = new List<TextBox> {Amount};
            LoadRecipe();
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                if (_recipeId == -1)
                {
                    string selectMaxId = "Select Max(RecipeID) From Recipe";
                    SQLiteCommand selectMaxCmd = new SQLiteCommand(selectMaxId, connect);
                    object val = selectMaxCmd.ExecuteScalar();
                    if (val.ToString() == "") _realRecipeId = 0;
                    else _realRecipeId = int.Parse(val.ToString() ?? throw new InvalidOperationException()) + 1;
                }

                connect.Close();
            }

            MinusProductButton.IsEnabled = IngredientWrap.Children.Count > 4;
        }

        private void LoadProducts()
        {
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = "SELECT ID, Name FROM Product"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            List<string> res1 = new List<string>();
            List<int> res2 = new List<int>();
            while (sqlReader.Read())
            {
                res2.Add(sqlReader.GetInt32(0));
                res1.Add(sqlReader.GetString(1));
            }

            _productId = res2;
            _productList = res1;
            ProductCBox.DataContext = _productList;
            connect.Close();
        }

        private static bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText = $"DELETE FROM RECIPE WHERE RECIPEID = {_recipeId}"
                };
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Видалено!");
            Close();
        }

        private void Amount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }

        private void MinusProductButton_Click(object sender, RoutedEventArgs e)
        {
            IngredientWrap.Children.Remove(IngredientWrap.Children[^1]);
            _productCBs.Remove(_productCBs[^1]);
            IngredientWrap.Children.Remove(IngredientWrap.Children[^1]);
            _amountBs.Remove(_amountBs[^1]);
            if (IngredientWrap.Children.Count == 4) MinusProductButton.IsEnabled = false;
        }

        private void PlusProductButton_Click(object sender, RoutedEventArgs e)
        {
            var comboBox = new ComboBox
            {
                ItemsSource = _productList,
                Width = 210,
                Height = 28,
                FontSize = 16,
                Margin = new Thickness(10, 10, 10, 10)
            };
            _productCBs.Add(comboBox);

            var textBox = new TextBox();
            textBox.PreviewTextInput += Amount_PreviewTextInput;
            textBox.ToolTip = Amount.ToolTip;
            textBox.Width = 40;
            textBox.Height = 28;
            textBox.FontSize = 16;
            textBox.Margin = new Thickness(10, 10, 10, 10);

            _amountBs.Add(textBox);
            IngredientWrap.Children.Add(comboBox);
            IngredientWrap.Children.Add(textBox);

            if (IngredientWrap.Children.Count > 4) MinusProductButton.IsEnabled = true;
        }

        private void LoadRecipe()
        {
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = $"SELECT * FROM Recipe WHERE RecipeID = {_recipeId}"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                NameBox.Text = sqlReader.GetString(1);
                InstructionBox.Text = sqlReader.GetString(2);
                TimeBox.Text = sqlReader.GetInt32(3).ToString();
                ProteinsBox.Text = sqlReader.GetDouble(4).ToString(CultureInfo.InvariantCulture);
                FatsBox.Text = sqlReader.GetDouble(5).ToString(CultureInfo.InvariantCulture);
                CarbohydratesBox.Text = sqlReader.GetDouble(6).ToString(CultureInfo.InvariantCulture);
                CaloriesBox.Text = sqlReader.GetDouble(7).ToString(CultureInfo.InvariantCulture);
            }

            command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = $"SELECT ProductID, Amount FROM ingredient WHERE RecipeID = {_recipeId}"
            };
            sqlReader = command.ExecuteReader();
            var p = false;
            while (sqlReader.Read())
            {
                int index = -1;
                for (int i = 0; i < _productId.Count; i++)
                {
                    if (sqlReader.GetInt32(0) == _productId[i])
                    {
                        index = i;
                        break;
                    }
                }

                if (p)
                {
                    var comboBox = new ComboBox
                    {
                        ItemsSource = _productList,
                        Width = 210,
                        Height = 28,
                        FontSize = 16,
                        Margin = new Thickness(10, 10, 10, 10),
                        SelectedIndex = index
                    };
                    _productCBs.Add(comboBox);

                    var textBox = new TextBox();
                    textBox.PreviewTextInput += Amount_PreviewTextInput;
                    textBox.ToolTip = Amount.ToolTip;
                    textBox.Width = 40;
                    textBox.Height = 28;
                    textBox.FontSize = 16;
                    textBox.Margin = new Thickness(10, 10, 10, 10);
                    textBox.Text = sqlReader.GetDouble(1).ToString(CultureInfo.InvariantCulture);

                    _amountBs.Add(textBox);
                    IngredientWrap.Children.Add(comboBox);
                    IngredientWrap.Children.Add(textBox);
                }
                else
                {
                    p = true;
                    _productCBs[0].SelectedIndex = index;
                    _amountBs[0].Text = sqlReader.GetDouble(1).ToString(CultureInfo.InvariantCulture);
                }
            }

            connect.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    var updateSql = _recipeId == -1
                        ? new SQLiteCommand(
                            $"INSERT INTO Recipe (RecipeID, Name, Instruction, Time, Proteins, Fats, Carbohydrates) VALUES ({_realRecipeId}, '{NameBox.Text}', '{InstructionBox.Text}', {TimeBox.Text}, {ProteinsBox.Text}, {FatsBox.Text}, {CarbohydratesBox.Text})",
                            connect)
                        : new SQLiteCommand(
                            $"UPDATE Recipe SET Name = '{NameBox.Text}', Instruction = '{InstructionBox.Text}', Time = {TimeBox.Text}, Proteins = {ProteinsBox.Text}, Fats = {FatsBox.Text}, Carbohydrates = {CarbohydratesBox.Text} WHERE RecipeID = {_recipeId}",
                            connect);
                    updateSql.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                var deleteSql = new SQLiteCommand($"DELETE FROM Ingredient WHERE RecipeID =  {_realRecipeId}", connect);
                deleteSql.ExecuteNonQuery();
                for (int i = 0; i < _productCBs.Count; i++)
                {
                    try
                    {
                        if (_productCBs[i].SelectedIndex > -1)
                        {
                            var updateSql =
                                new SQLiteCommand(
                                    $"INSERT INTO Ingredient (ProductID, RecipeID, Amount) VALUES ({_productId[_productCBs[i].SelectedIndex]}, {_realRecipeId}, {_amountBs[i].Text})",
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mParent.FilteredRecipesList = Controller.GetRecipes("", 0, 0, Double.MaxValue);
            _mParent.LoadRecipes();
            _mParent.Show();
        }

        private void MadeButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    var updateSql =
                        new SQLiteCommand($"INSERT INTO Statistics (RecipeID, Time) VALUES ({_recipeId}, DATE())",
                            connect);
                    updateSql.ExecuteNonQuery();
                    var selectSql =
                        new SQLiteCommand($"SELECT ProductID, Amount FROM ingredient WHERE RecipeID = {_recipeId}",
                            connect);
                    var reader = selectSql.ExecuteReader();
                    while (reader.Read())
                    {
                        updateSql = new SQLiteCommand(
                            $"UPDATE Warehouse SET Amount = Amount - {reader.GetDouble(1)} WHERE ProductID = {reader.GetInt32(0)}",
                            connect);
                        updateSql.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            MessageBox.Show("Запис додано!");
            _mParent.LoadWarehouseList();
            _mParent.LoadRecipesNow();
            this.Close();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,
                FileName = NameBox.Text,
                Title = NameBox.Text
            };

            saveFileDialog1.ShowDialog();
            string message = $"Назва: {NameBox.Text}\nІнгредієнти: ";
            for (int i = 0; i < _productCBs.Count - 1; i++)
            {
                message += _productCBs[i].SelectedValue + $" ({_amountBs[i].Text}), ";
            }

            message += _productCBs[^1].SelectedValue + $" ({_amountBs[^1].Text})\n";
            message +=
                $"Інструкція: {InstructionBox.Text}\nЧас приготування: {TimeBox.Text}\nБілки: {ProteinsBox.Text}\nЖири: {FatsBox.Text}\nВуглеводи: {CarbohydratesBox.Text}\nКалорійність: {(CaloriesBox.Text == "" ? Convert.ToString(4 * (Convert.ToDouble(ProteinsBox.Text) + Convert.ToDouble(CarbohydratesBox.Text)) + 9 * Convert.ToDouble(FatsBox.Text), CultureInfo.InvariantCulture) : CaloriesBox.Text)}";
            if (saveFileDialog1.FileName.Length > 0) File.WriteAllText(saveFileDialog1.FileName, message);
        }
    }
}