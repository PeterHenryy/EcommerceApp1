﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp1.Models
{
    public class TransactionItem
    {
        [Key]
        public int ID { get; set; }
        public int Quantity { get; set; }
        public double ShippingCost { get; set; }
        public string ShippingOption { get; set; }

        [ForeignKey("Transactions")]
        public int TransactionID { get; set; }
        public Transaction Transaction { get; set; }
        [ForeignKey("Products")]
        public int ProductID { get; set; }
        public Product Product { get; set; }

    }
}
