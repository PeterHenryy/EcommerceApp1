﻿using EcommerceApp1.Models;
using EcommerceApp1.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceApp1.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepos;

        public ProductService(ProductRepository productRepos)
        {
            _productRepos = productRepos;
        }

        public bool Create(Product product)
        {
            bool createdProduct = _productRepos.Create(product);
            return createdProduct;
        }

        public bool Delete(int productID)
        {
            var deletedProduct = _productRepos.Delete(productID);
            return deletedProduct;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            var products = _productRepos.GetAllProducts().Where(x => !x.Archived);
            return products;
        }

        public Product GetProductByID(int productID)
        {
            var product = _productRepos.GetProductByID(productID);
            return product;
        }

        public bool Update(Product product)
        {
            var updatedProduct = _productRepos.Update(product);
            return updatedProduct;
        }

        public List<Category> GetAllCategories()
        {
            var categories = _productRepos.GetAllCategories();
            return categories;
        }

        public List<Company> GetAllCompanies()
        {
            var companies = _productRepos.GetAllCompanies();
            return companies;
        }

        public IEnumerable<Review> GetReviewsOfSpecificProduct(int productID)
        {
            var productReviews = _productRepos.GetReviews().Where(x => x.ProductID == productID);
            return productReviews;
        }

        public bool HasUserBoughtProduct(int productID, int userID)
        {
            var productTransactions = _productRepos.GetTransactions().Where(x => x.UserID == userID)
                                                                        .Where(x => x.ProductID == productID);
            return productTransactions.Any();
        }

        public IEnumerable<Comment> GetAllComments()
        {
            var comments = _productRepos.GetAllComments();
            return comments;
        }

        public IEnumerable<Like> GetLikes()
        {
            var likes = _productRepos.GetLikes();
            return likes;
        }

        public IEnumerable<Dislike> GetDislikes()
        {
            var dislikes = _productRepos.GetDislikes();
            return dislikes;
        }

        public bool HasUserReviewedProduct(int productID, int userID)
        {
            var userReview = GetReviewsOfSpecificProduct(productID).Where(x => x.UserID == userID);
            return userReview.Any();
        }
      
        public double CalculateProductAverageRating(int productID)
        {
            var productReviews = GetReviewsOfSpecificProduct(productID);
            double rating = productReviews.Sum(x => x.Rating) / (double)productReviews.Count();
            double roundedRating = Math.Round(rating, 1);
            return roundedRating;
        }

        public IEnumerable<Product> GetCompanyProducts(int companyID)
        {
            var companyProducts = _productRepos.GetAllProducts().Where(x => x.CompanyID == companyID);
            return companyProducts;
        }

        public void ManageProductArchiving(int productID, int option)
        {
            Product product = GetProductByID(productID);
            if (option == 1)
            {
                product.Archived = true;
            }
            else
            {
                product.Archived = false;
            }
            _productRepos.Update(product);
        }

    }
}
