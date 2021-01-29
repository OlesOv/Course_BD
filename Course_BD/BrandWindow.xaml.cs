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
    /// Interaction logic for BrandWindow.xaml
    /// </summary>
    public partial class BrandWindow : Window
    {
        private readonly ProductWindow _mParent;

        public BrandWindow(ProductWindow parent)
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
                            $"INSERT INTO Brand (ID, Name) VALUES ((SELECT MAX(ID)+1 FROM Brand), '{NameBox.Text}')",
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
            _mParent.LoadBrands();
            _mParent.Show();
        }
    }
}