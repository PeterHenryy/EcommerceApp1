﻿using EcommerceApp1.Helpers;
using EcommerceApp1.Models;
using EcommerceApp1.Models.Repositories;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceApp1.Services
{
    public class CouponService
    {
        private readonly CouponRepository _couponRepos;

        public CouponService(CouponRepository couponRepos)
        {
            _couponRepos = couponRepos;
        }

        public bool Create(Coupon coupon)
        {
            bool createdCoupon = _couponRepos.Create(coupon);
            return createdCoupon;
        }

        public bool Delete(int couponID)
        {
            bool deletedCoupon = _couponRepos.Delete(couponID);
            return deletedCoupon;
        }

        public IEnumerable<Coupon> GetCompanyCoupons(int? companyID)
        {
            var companyCoupons = _couponRepos.GetCompanyCoupons(companyID).ToList();
            return companyCoupons;
        }

        public IEnumerable<Category> GetCompanyCategories(int companyID)
        {
            var companyProducts = _couponRepos.GetProducts().Where(x => x.CompanyID == companyID);
            var categories = _couponRepos.GetCategories();
            List<Category> companyCategories = new List<Category>();
            foreach(var category in categories)
            {
                if(companyProducts.Any(x => x.CategoryID == category.ID))
                {
                    companyCategories.Add(category);
                }
            }
            return companyCategories;
        }

        public IEnumerable<Product> GetCompanyProducts(int? companyID)
        {
            var companyProducts = _couponRepos.GetProducts().Where(x => x.CompanyID == companyID);
            return companyProducts;
        }
        public Coupon GetCoupon(string code, int? companyID)
        {
            Coupon coupon = GetCompanyCoupons(companyID).SingleOrDefault(x => x.Code == code);
            return coupon;
        }

        public CouponValidator ValidateCoupon(Transaction transaction, IEnumerable<CartItem> cartItems, string couponCode)
        {
            CouponValidator couponValidator = new CouponValidator();

            foreach (var item in cartItems)
            {
                Coupon coupon = GetCoupon(couponCode, item.Product.CompanyID);
                bool isCouponValid = couponValidator.Validate(coupon, item.Product);
                if (isCouponValid)
                {
                    transaction.Total -= (transaction.Total * (coupon.DiscountPercentage / 100)) * 100 / 100;
                    couponValidator.CouponValid = isCouponValid;
                    couponValidator.CouponPercentage = coupon.DiscountPercentage;
                    break;
                }
            }
            couponValidator.Total = transaction.Total;
            return couponValidator;
        }

        public bool DecreaseCouponQuantity(string couponCode, IEnumerable<CartItem> cartItems)
        {
            foreach (var item in cartItems)
            {
                Coupon coupon = GetCoupon(couponCode, item.Product.CompanyID);
                if (coupon != null)
                {
                    coupon.Quantity--;
                    UpdateCoupon(coupon);
                    if (coupon.Quantity == 0) _couponRepos.Delete(coupon.ID);
                    return true;
                }
            }
            return false;
        }

        public bool UpdateCoupon(Coupon coupon)
        {
            bool updatedCoupon = _couponRepos.Update(coupon);
            return updatedCoupon;
        }
    }
}
