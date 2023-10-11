﻿using EcommerceApp1.Models;

namespace EcommerceApp1.Helpers
{
    public class CouponValidator
    {
        public double Total { get; set; }
        public bool CouponValid { get; set; }
        public double CouponPercentage { get; set; }
        public bool Validate(Coupon coupon, Product product)
        {
            if (coupon == null) return false;

            if (coupon.ProductID == null && coupon.CategoryID == null) return true;
            if (coupon.ProductID == product.ID) return true;
            if (coupon.CategoryID == product.CategoryID) return true;

            return false;
        }
    }
}
