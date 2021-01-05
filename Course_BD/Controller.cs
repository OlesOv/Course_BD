using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Course_BD
{
    public static class Controller
    {
        public static string DBPath = AppDomain.CurrentDomain.BaseDirectory + "\\recipe_book.db";
        public static List<Recipe> GetRecipes(string CategoryName)
        {
            var res = new List<Recipe>();
            if (CategoryName == "")
            {
                using SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}");
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Recipe"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Recipe t = new Recipe
                    {
                        ID = sqlReader.GetInt32(0),
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
                Connect.Close();
            }
            else
            {
                using SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}");
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT * FROM Recipe LEFT JOIN WhereRecipeBelongs ON Recipe.RecipeID = WhereRecipeBelongs.RecipeID WHERE WhereRecipeBelongs.CategoryName = '{CategoryName}'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Recipe t = new Recipe()
                    {
                        ID = sqlReader.GetInt32(0),
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
                Connect.Close();
            }
            return res;
        }
        public static List<Category> GetCategories()
        {
            var res = new List<Category>();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Category"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Category t = new Category()
                    {
                        Name = sqlReader.GetString(0),
                        Description = sqlReader.GetString(1)
                    };
                    res.Add(t);
                }
                Connect.Close();
            }
            return res;
        }
        public static List<Recipe> SearchRecipes(string text)
        {
            var res = new List<Recipe>();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT * FROM Recipe  WHERE INSTR(LOWER(Name), LOWER('{text}')) > 0"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Recipe t = new Recipe()
                    {
                        ID = sqlReader.GetInt32(0),
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
                Connect.Close();
            }
            return res;
        }
        public static List<Category> SearchCategories(string text)
        {
            var res = new List<Category>();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT * FROM Category WHERE INSTR(LOWER(Name), LOWER('{text}')) > 0"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Category t = new Category();
                    t.Name = sqlReader.GetString(0);
                    t.Description = sqlReader.GetString(1);
                    res.Add(t);
                }
                Connect.Close();
            }
            return res;
        }
        public static List<Product> GetProducts()
        {
            var res = new List<Product>();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Product"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Product t = new Product();
                    t.UPCEAN = sqlReader.GetInt64(0);
                    t.ID = sqlReader.GetInt32(1);
                    t.BrandID = sqlReader.GetInt32(2);
                    t.CategoryID = sqlReader.GetInt32(3);
                    t.Name = sqlReader.GetString(4);
                    res.Add(t);
                }
                Connect.Close();
            }
            return res;
        }
        public static List<WHProduct> GetWHProducts()
        {
            var res = new List<WHProduct>();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Warehouse LEFT JOIN Product ON Product.ID = Warehouse.ProductID"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    WHProduct t = new WHProduct();
                    t.ID = sqlReader.GetInt32(0);
                    t.Amount = sqlReader.GetDouble(1);
                    t.Name = sqlReader.GetString(6);
                    res.Add(t);
                }
                Connect.Close();
            }
            return res;
        }
        public static List<Recipe> GetRecipesNow()
        {
            var res = new List<Recipe>();
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT * FROM Recipe"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                while (sqlReader.Read())
                {
                    SQLiteCommand AnotherCommand = new SQLiteCommand
                    {
                        Connection = Connect,
                        CommandText = $"SELECT Ingredient.Amount IA, Warehouse.Amount WA FROM Ingredient LEFT JOIN Warehouse ON Warehouse.ProductID = Ingredient.ProductID WHERE Ingredient.RecipeID = {sqlReader.GetInt32(0)} "
                    };
                    SQLiteDataReader reader = AnotherCommand.ExecuteReader();
                    bool p = true;
                    while (reader.Read())
                    {
                        if(reader.GetDouble(0) > (reader.GetValue(1).ToString() == "" ? 0 : reader.GetDouble(1)))
                        {
                            p = false;
                            break;
                        }
                    }
                    if (p)
                    {
                        Recipe t = new Recipe();
                        t.ID = sqlReader.GetInt32(0);
                        t.Name = sqlReader.GetString(1);
                        t.Instruction = sqlReader.GetString(2);
                        t.Time = sqlReader.GetInt32(3);
                        t.Proteins = sqlReader.GetDouble(4);
                        t.Fats = sqlReader.GetDouble(5);
                        t.Carbohydrates = sqlReader.GetDouble(6);
                        t.Calories = sqlReader.GetDouble(7);
                        res.Add(t);
                    }
                }
                Connect.Close();
            }
            return res;
        }
        public static string GetStatistics()
        {
            string res = "Статистика приготувань рецептів:";
            using (SQLiteConnection Connect = new SQLiteConnection($"Data Source={DBPath}"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = $"SELECT Recipe.Name, Statistics.Time FROM Statistics LEFT JOIN Recipe ON Statistics.RecipeID = Recipe.RecipeID ORDER BY Name;"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();
                string curRecipe = "";
                while (sqlReader.Read())
                {
                    if (curRecipe != sqlReader.GetString(0)) res += $"\n{sqlReader.GetString(0)}:{sqlReader.GetString(1)}";
                    else res += $", {sqlReader.GetString(1)}";
                    curRecipe = sqlReader.GetString(0);
                }
                Connect.Close();
            }
            return res;
        }
    }
}
