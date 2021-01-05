using System;
using System.Collections.Generic;
using System.Text;

namespace Course_BD
{
    public class Recipe
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Instruction { get; set; }
        public int Time { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
        public double Calories { get; set; }

    }
}
