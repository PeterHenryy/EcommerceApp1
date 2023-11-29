﻿using EcommerceApp1.Models;
using EcommerceApp1.Models.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EcommerceApp1.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepos;
        private readonly IWebHostEnvironment _environment;

        public ProductService(ProductRepository productRepos, IWebHostEnvironment environment)
        {
            _productRepos = productRepos;
            _environment = environment;
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

        public IEnumerable<Review> GetReviews()
        {
            var reviews = _productRepos.GetReviews();
            return reviews;
        }

        public bool HasUserBoughtProduct(int productID, int userID)
        {
            var transactionItems = _productRepos.GetTransactionItems();
            bool hasBought = transactionItems.Any(x => x.Transaction.UserID == userID && x.ProductID == productID);
            return hasBought;
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
            if (productReviews.Any())
            {
                double rating = productReviews.Sum(x => x.Rating) / (double)productReviews.Count();
                double roundedRating = Math.Round(rating, 1);
                return roundedRating;
            }
            else
            {
                return 0;
            }
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

        public bool UpdateCompanyProductStock(int? companyID, int quantity, string action)
        {
            Company company = _productRepos.GetCompanyByID(companyID);
            if(action == "increase")
            {
                company.ProductsInStock += quantity;
            }
            else
            {
                company.ProductsInStock -= quantity;
            }
            bool updatedCompany = _productRepos.UpdateCompanyProductStock(company);
            return updatedCompany;
        }

        public void CheckProductStockChange(int productID, int productNewStock)
        {
            Product product = GetProductByID(productID);
            int quantity;
            if (product.Stock == productNewStock) return;
            else if(product.Stock > productNewStock)
            {
                quantity = product.Stock - productNewStock;
                UpdateCompanyProductStock(product.CompanyID, quantity, "decrease");
            }
            else
            {
                quantity = productNewStock - product.Stock;
                UpdateCompanyProductStock(product.CompanyID, quantity, "increase");

            }

        }

        public bool CreateImage(Image image)
        {
            bool createdImage = _productRepos.CreateImage(image);
            return createdImage;
        }

        public void HandleProductImages(Product product, IFormFileCollection files)
        {
            string fileName;
            string path;

            if (files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var extension = Path.GetExtension(files[i].FileName);
                    fileName = (i == 0) ? product.Image : Guid.NewGuid().ToString() + extension;
                    path = Path.Combine(_environment.WebRootPath, "Img") + "/" + fileName;

                    Image img = new Image();
                    img.Name = fileName;
                    img.ProductID = product.ID;

                    bool createdImage = CreateImage(img);

                    using (FileStream fs = System.IO.File.Create(path))
                    {
                        files[i].CopyTo(fs);
                        fs.Flush();
                    }
                }
            }
        }

        public List<Image> GetProductImages(int productID)
        {
            var images = _productRepos.GetImages().Where(x => x.ProductID == productID).ToList();
            return images;
        }

        public int GetProductSales(int productID)
        {
            var transactions = _productRepos.GetTransactions();
            //int sales = transactions.Count(x => x.ProductID == productID);
            return 0;
        }

        public IEnumerable<Product> CategoryFilter(int categoryID)
        {
            var filteredProducts = GetAllProducts().Where(x => x.CategoryID == categoryID);
            return filteredProducts;
        }

        public IEnumerable<Product> PriceFilter(string order)
        {
            IEnumerable<Product> filteredProducts;
            if (order.Equals("ascending"))
            {
                filteredProducts = GetAllProducts().OrderBy(x => x.Price);
                return filteredProducts;
            }
            filteredProducts = GetAllProducts().OrderByDescending(x => x.Price);
            return filteredProducts;
        }

        public IEnumerable<Product> RatingFilter(string order)
        {
            IEnumerable<Product> filteredProducts;
            if (order.Equals("ascending"))
            {
                filteredProducts = GetAllProducts().OrderBy(x => x.AverageRating);
                return filteredProducts;
            }
            filteredProducts = GetAllProducts().OrderByDescending(x => x.AverageRating);
            return filteredProducts;
        }

    }
}
