using System;
using System.Collections.Generic;
using System.Text;

namespace Course_BD
{
    public class Product
    {
        public int ID { get; set; }
        public long UPCEAN { get; set; }
        public string Name { get; set; }
        public int BrandID { get; set; }
        public int CategoryID { get; set; }
    }
}
