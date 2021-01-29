using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Course_BD
{
    public static class Controller
    {
        public static readonly string DbPath = AppDomain.CurrentDomain.BaseDirectory + "\\recipe_book.db";

        public static List<Recipe> GetRecipes(string categoryName, int order, double from, double to)
        {
            var res = new List<Recipe>();
            var p = order == 0 ? "ASC" : "DESC";
            if (categoryName == "")
            {
                using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText = $"SELECT * FROM Recipe WHERE Calories >= {from} AND Calories <= {to} ORDER BY Name {p}"
                };
                SQLiteDataReader sqlReader = command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Recipe t = new Recipe
                    {
                        Id = sqlReader.GetInt32(0),
                        Name = sqlReader.GetString(1),
                        Instruction = sqlReader.GetString(2),
                        Time = sqlReader.GetInt32(3),
                        Proteins = sqlReader.GetDouble(4),
                        Fats = sqlReader.GetDouble(5),
                        Carbohydrates = sqlReader.GetDouble(6),
                        Calories = sqlReader.GetDouble(7)
                    };
                    res.Add(t);
                }

                connect.Close();
            }
            else
            {
                using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
                connect.Open();
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connect,
                    CommandText =
                        $"SELECT * FROM Recipe LEFT JOIN WhereRecipeBelongs ON Recipe.RecipeID = WhereRecipeBelongs.RecipeID WHERE WhereRecipeBelongs.CategoryName = '{categoryName}' AND Calories >= {from} AND Calories <= {to} ORDER BY Name {p}"
                };
                SQLiteDataReader sqlReader = command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Recipe t = new Recipe()
                    {
                        Id = sqlReader.GetInt32(0),
                        Name = sqlReader.GetString(1),
                        Instruction = sqlReader.GetString(2),
                        Time = sqlReader.GetInt32(3),
                        Proteins = sqlReader.GetDouble(4),
                        Fats = sqlReader.GetDouble(5),
                        Carbohydrates = sqlReader.GetDouble(6),
                        Calories = sqlReader.GetDouble(7)
                    };
                    res.Add(t);
                }

                connect.Close();
            }
            return res;
        }

        public static List<Category> GetCategories()
        {
            var res = new List<Category>();
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = @"SELECT * FROM Category"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                Category t = new Category()
                {
                    Name = sqlReader.GetString(0),
                    Description = sqlReader.GetString(1)
                };
                res.Add(t);
            }

            connect.Close();

            return res;
        }

        public static List<Recipe> SearchRecipes(string text)
        {
            var res = new List<Recipe>();
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = $"SELECT * FROM Recipe  WHERE INSTR(LOWER(Name), LOWER('{text}')) > 0"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                Recipe t = new Recipe()
                {
                    Id = sqlReader.GetInt32(0),
                    Name = sqlReader.GetString(1),
                    Instruction = sqlReader.GetString(2),
                    Time = sqlReader.GetInt32(3),
                    Proteins = sqlReader.GetDouble(4),
                    Fats = sqlReader.GetDouble(5),
                    Carbohydrates = sqlReader.GetDouble(6),
                    Calories = sqlReader.GetDouble(7)
                };
                res.Add(t);
            }

            connect.Close();

            return res;
        }

        public static List<Category> SearchCategories(string text)
        {
            var res = new List<Category>();
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = $"SELECT * FROM Category WHERE INSTR(LOWER(Name), LOWER('{text}')) > 0"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                Category t = new Category {Name = sqlReader.GetString(0), Description = sqlReader.GetString(1)};
                res.Add(t);
            }

            connect.Close();

            return res;
        }

        public static List<Product> GetProducts()
        {
            var res = new List<Product>();
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = @"SELECT * FROM Product"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                Product t = new Product
                {
                    Upcean = sqlReader.GetValue(0).ToString() == "0" ? 0 : sqlReader.GetInt64(0),
                    Id = sqlReader.GetInt32(1),
                    BrandId = sqlReader.GetInt32(2),
                    CategoryId = sqlReader.GetInt32(3),
                    Name = sqlReader.GetString(4)
                };
                res.Add(t);
            }

            connect.Close();

            return res;
        }

        public static List<WhProduct> GetWhProducts()
        {
            var res = new List<WhProduct>();
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText = @"SELECT * FROM Warehouse LEFT JOIN Product ON Product.ID = Warehouse.ProductID"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                WhProduct t = new WhProduct
                {
                    Id = sqlReader.GetInt32(0),
                    Amount = sqlReader.GetDouble(1),
                    Name = sqlReader.GetString(6)
                };
                res.Add(t);
            }

            connect.Close();

            return res;
        }

        public static List<Recipe> GetRecipesNow()
        {
            var res = new List<Recipe>();
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText =
                    "SELECT Recipe.* FROM Recipe LEFT JOIN Ingredient ON Recipe.RecipeID = Ingredient.RecipeID LEFT JOIN Warehouse ON Warehouse.ProductID = Ingredient.ProductID GROUP BY Recipe.RecipeID HAVING SUM(Ingredient.Amount <= Warehouse.Amount) == COUNT(Ingredient.Amount); "
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                Recipe t = new Recipe
                {
                    Id = sqlReader.GetInt32(0),
                    Name = sqlReader.GetString(1),
                    Instruction = sqlReader.GetString(2),
                    Time = sqlReader.GetInt32(3),
                    Proteins = sqlReader.GetDouble(4),
                    Fats = sqlReader.GetDouble(5),
                    Carbohydrates = sqlReader.GetDouble(6),
                    Calories = sqlReader.GetDouble(7)
                };
                res.Add(t);
            }

            connect.Close();

            return res;
        }

        public static string GetStatistics()
        {
            string res = "Статистика приготувань рецептів:";
            using SQLiteConnection connect = new SQLiteConnection($"Data Source={DbPath}");
            connect.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connect,
                CommandText =
                    "SELECT Recipe.Name, Statistics.Time FROM Statistics LEFT JOIN Recipe ON Statistics.RecipeID = Recipe.RecipeID ORDER BY Name;"
            };
            SQLiteDataReader sqlReader = command.ExecuteReader();
            string curRecipe = "";
            while (sqlReader.Read())
            {
                if (curRecipe != sqlReader.GetString(0))
                    res += $"\n{sqlReader.GetString(0)}:{sqlReader.GetString(1)}";
                else res += $", {sqlReader.GetString(1)}";
                curRecipe = sqlReader.GetString(0);
            }

            connect.Close();

            return res;
        }
    }
}