using System;
using System.Data.SQLite;
using System.Windows;

namespace Course_BD
{
    public partial class ProductCategoryWindow
    {
        private readonly ProductWindow _mParent;

        public ProductCategoryWindow(ProductWindow parent)
        {
            _mParent = parent;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connect = new SQLiteConnection($"Data Source={Controller.DbPath}"))
            {
                connect.Open();
                try
                {
                    var updateSql =
                        new SQLiteCommand(
                            $"INSERT INTO ProductCategory (ID, Name) VALUES ((SELECT MAX(ID)+1 FROM ProductCategory), '{NameBox.Text}')",
                            connect);
                    updateSql.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            MessageBox.Show("Готово!");
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mParent.LoadCategories();
            _mParent.Show();
        }
    }
}