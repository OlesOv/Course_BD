using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Course_BD
{
    /// <summary>
    /// Interaction logic for RecipeWindow.xaml
    /// </summary>
    public partial class RecipeWindow : Window
    {
        private MainWindow mParent;
        private int RecipeID;
        private int RealRecipeID;
        private List<int> productId;
        private readonly List<ComboBox> productCBs;
        private readonly List<TextBox> amountBs;
        private static readonly Regex Regex = new Regex("[^0-9.]");
        private List<string> productList = new List<string>();
        public RecipeWindow(MainWindow parent, int recipeID)
        {
            mParent = parent;
            RecipeID = recipeID;
            InitializeComponent();
            productId = new List<int>();
            LoadProducts();
            productCBs = new List<ComboBox>();
            productCBs.Add(ProductCBox);
            amountBs = new List<TextBox>();
            amountBs.Add(Amount);
            LoadRecipe();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                if (RecipeID == -1)
                {
                    string selectMaxId = "Select Max(RecipeID) From Recipe";
                    SQLiteCommand selectMaxCmd = new SQLiteCommand(selectMaxId, Connect);
                    object val = selectMaxCmd.ExecuteScalar();
                    if (val.ToString() == "") RealRecipeID = 0;
                    else RealRecipeID = int.Parse(val.ToString()) + 1;
                }
                Connect.Close();
            }
            MinusProductButton.IsEnabled = IngredientWrap.Children.Count > 4;
        }

        private void LoadProducts()
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT ID, Name FROM Product"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                List<string> res1 = new List<string>();
                List<int> res2 = new List<int>();
                while (sqlReader.Read())
                {
                    res2.Add(sqlReader.GetInt32(0));
                    res1.Add(sqlReader.GetString(1));
                }
                productId = res2;
                productList = res1;
                ProductCBox.DataContext = productList;
                Connect.Close();
            }
        }
        private static bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text);
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"DELETE FROM RECIPE WHERE RECIPEID = {RecipeID}"
                };
                Command.ExecuteNonQuery();
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
            productCBs.Remove(productCBs[^1]);
            IngredientWrap.Children.Remove(IngredientWrap.Children[^1]);
            amountBs.Remove(amountBs[^1]);
            if (IngredientWrap.Children.Count == 4) MinusProductButton.IsEnabled = false;
        }
        private void PlusProductButton_Click(object sender, RoutedEventArgs e)
        {
            var comboBox = new ComboBox
            {
                ItemsSource = productList,
                Width = 210,
                Height = 28,
                FontSize = 16,
                Margin = new Thickness(10, 10, 10, 10)
            };
            productCBs.Add(comboBox);

            var textBox = new TextBox();
            textBox.PreviewTextInput += Amount_PreviewTextInput;
            textBox.ToolTip = Amount.ToolTip;
            textBox.Width = 40;
            textBox.Height = 28;
            textBox.FontSize = 16;
            textBox.Margin = new Thickness(10, 10, 10, 10);

            amountBs.Add(textBox);
            IngredientWrap.Children.Add(comboBox);
            IngredientWrap.Children.Add(textBox);

            if (IngredientWrap.Children.Count > 4) MinusProductButton.IsEnabled = true;
        }
        private void LoadRecipe()
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT * FROM Recipe WHERE RecipeID = {RecipeID}"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    NameBox.Text = sqlReader.GetString(1);
                    InstructionBox.Text = sqlReader.GetString(2);
                    TimeBox.Text = sqlReader.GetInt32(3).ToString();
                    ProteinsBox.Text = sqlReader.GetDouble(4).ToString();
                    FatsBox.Text = sqlReader.GetDouble(5).ToString();
                    CarbohydratesBox.Text = sqlReader.GetDouble(6).ToString();
                    CaloriesBox.Text = sqlReader.GetDouble(7).ToString();
                }
                Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT ProductID, Amount FROM ingredient WHERE RecipeID = {RecipeID}"
                };
                sqlReader = Command.ExecuteReader();
                var p = false;
                while (sqlReader.Read())
                {
                    int index = -1;
                    for (int i = 0; i < productId.Count; i++)
                    {
                        if (sqlReader.GetInt32(0) == productId[i])
                        {
                            index = i;
                            break;
                        }
                    }
                    if (p)
                    {
                        var comboBox = new ComboBox
                        {
                            ItemsSource = productList,
                            Width = 210,
                            Height = 28,
                            FontSize = 16,
                            Margin = new Thickness(10, 10, 10, 10),
                            SelectedIndex = index
                        };
                        productCBs.Add(comboBox);

                        var textBox = new TextBox();
                        textBox.PreviewTextInput += Amount_PreviewTextInput;
                        textBox.ToolTip = Amount.ToolTip;
                        textBox.Width = 40;
                        textBox.Height = 28;
                        textBox.FontSize = 16;
                        textBox.Margin = new Thickness(10, 10, 10, 10);
                        textBox.Text = sqlReader.GetDouble(1).ToString();

                        amountBs.Add(textBox);
                        IngredientWrap.Children.Add(comboBox);
                        IngredientWrap.Children.Add(textBox);
                    }
                    else
                    {
                        p = true;
                        productCBs[0].SelectedIndex = index;
                        amountBs[0].Text = sqlReader.GetDouble(1).ToString();
                    }
                }
                Connect.Close();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                try
                {
                    var updateSql = RecipeID == -1 ? new SQLiteCommand($"INSERT INTO Recipe (RecipeID, Name, Instruction, Time, Proteins, Fats, Carbohydrates) VALUES ({RealRecipeID}, '{NameBox.Text}', '{InstructionBox.Text}', {TimeBox.Text}, {ProteinsBox.Text}, {FatsBox.Text}, {CarbohydratesBox.Text})", Connect) : new SQLiteCommand($"UPDATE Recipe SET Name = '{NameBox.Text}', Instruction = '{InstructionBox.Text}', Time = {TimeBox.Text}, Proteins = {ProteinsBox.Text}, Fats = {FatsBox.Text}, Carbohydrates = {CarbohydratesBox.Text} WHERE RecipeID = {RecipeID}", Connect);
                    updateSql.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                var deleteSql = new SQLiteCommand($"DELETE FROM Ingredient WHERE RecipeID =  {RealRecipeID}", Connect);
                deleteSql.ExecuteNonQuery();
                for (int i = 0; i < productCBs.Count; i++)
                {
                    try
                    {
                        if (productCBs[i].SelectedIndex > -1)
                        {
                            var updateSql = new SQLiteCommand($"INSERT INTO Ingredient (ProductID, RecipeID, Amount) VALUES ({productId[productCBs[i].SelectedIndex]}, {RealRecipeID}, {amountBs[i].Text})", Connect);
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
            mParent.filteredRecipesList = Controller.GetRecipes("");
            mParent.LoadRecipes();
            mParent.Show();
        }

        private void MadeButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={Controller.DBPath}"))
            {
                Connect.Open();
                try
                {
                    var updateSql = new SQLiteCommand($"INSERT INTO Statistics (RecipeID, Time) VALUES ({RecipeID}, DATE())", Connect);
                    updateSql.ExecuteNonQuery();
                    var selectSql = new SQLiteCommand($"SELECT ProductID, Amount FROM ingredient WHERE RecipeID = {RecipeID}", Connect);
                    var reader = selectSql.ExecuteReader();
                    while (reader.Read())
                    {
                        updateSql = new SQLiteCommand($"UPDATE Warehouse SET Amount = Amount - {reader.GetDouble(1)} WHERE ProductID = {reader.GetInt32(0)}", Connect);
                        updateSql.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            MessageBox.Show("Запис додано!");
            mParent.LoadWarehouseList();
            mParent.LoadRecipesNow();
            this.Close();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = NameBox.Text;
            saveFileDialog1.Title = NameBox.Text;
            saveFileDialog1.ShowDialog();
            string message = $"Назва: {NameBox.Text}\nІнгредієнти: ";
            for (int i = 0; i < productCBs.Count - 1; i++)
            {
                message += productCBs[i].SelectedValue + $" ({amountBs[i].Text}), ";
            }
            message += productCBs[^1].SelectedValue + $" ({amountBs[^1].Text})\n";
            message += $"Інструкція: {InstructionBox.Text}\nЧас приготування: {TimeBox.Text}\nБілки: {ProteinsBox.Text}\nЖири: {FatsBox.Text}\nВуглеводи: {CarbohydratesBox.Text}\nКалорійність: {(CaloriesBox.Text == "" ? Convert.ToString(4 * (Convert.ToDouble(ProteinsBox.Text) + Convert.ToDouble(CarbohydratesBox.Text)) + 9 * Convert.ToDouble(FatsBox.Text)) : CaloriesBox.Text)}";
            if (saveFileDialog1.FileName.Length > 0) File.WriteAllText(saveFileDialog1.FileName, message);
        }
    }
}
