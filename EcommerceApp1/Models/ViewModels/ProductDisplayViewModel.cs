﻿using System.Collections.Generic;

namespace EcommerceApp1.Models.ViewModels
{
    public class ProductDisplayViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public List<Company> Companies { get; set; }
    }
}
