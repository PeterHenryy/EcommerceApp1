﻿using EcommerceApp1.Models;
using EcommerceApp1.Models.Identity;
using EcommerceApp1.Models.ViewModels;
using EcommerceApp1.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace EcommerceApp1.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly UserService _userService;
        private readonly IBlobService _blobService;
        private readonly AppUser _user;

        public ProductController(ProductService productService, UserService userService, IBlobService blobService)
        {
            _productService = productService;
            _userService = userService;
            _blobService = blobService;
            _user = userService.GetCurrentUser();
        }
        
        public IActionResult Index()
        {
            var productIndexViewModel = new ProductIndexViewModel();
            productIndexViewModel.Products = _productService.GetAllProducts().ToList();
            productIndexViewModel.User = _user;
            var reviews = _productService.GetReviews();
            return View(productIndexViewModel);
        }

        public IActionResult ProductsDisplay()
        {
            var productDisplayViewModel = new ProductDisplayViewModel();
            productDisplayViewModel.Companies = _productService.GetAllCompanies();
            productDisplayViewModel.Categories = _productService.GetAllCategories();

            if (TempData["FilteredProducts"] != null)
            {
                var filteredProductsJson = TempData["FilteredProducts"].ToString();
                productDisplayViewModel.Products = JsonConvert.DeserializeObject<List<Product>>(filteredProductsJson);
            }
            else
            {
                productDisplayViewModel.Products = _productService.GetAllProducts();
            }

            return View(productDisplayViewModel);
        }

        public IActionResult Delete(int productID)
        {
            _productService.Delete(productID);
            return RedirectToAction("CompanyProducts", "Product");
        }

        [HttpGet]
        public IActionResult Update(int productID)
        {
            Product product = _productService.GetProductByID(productID);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Product product)
        {
            await HandleBlob(product);
            _productService.CheckProductStockChange(product.ID, product.Stock);
            bool updatedProduct = _productService.Update(product);
            // _productService.HandleProductImages(product, files);
            if (updatedProduct)
            {
                return RedirectToAction("CompanyProducts", "Product");
            }
            return View(product);
        }

        public async Task HandleBlob(Product product)
        {
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                //product.Image = Guid.NewGuid().ToString() + Path.GetExtension(files[0].FileName);
                bool uploadedBlob = await _blobService.UploadBlob(files[0].FileName, files[0], new Blob());
                product.Image = _blobService.GetBlob(files[0].FileName);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductCreateViewModel createViewModel = new ProductCreateViewModel();
            createViewModel.Categories = _productService.GetAllCategories();
            createViewModel.User = _user;
            return View(createViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel productVM)
        {
            var files = HttpContext.Request.Form.Files;
            productVM.Product.UserID = _user.Id;
            await HandleBlob(productVM.Product);
            bool createdProduct = _productService.Create(productVM.Product);
            if (createdProduct)
            {
                bool updatedStock = _productService.UpdateCompanyProductStock(productVM.Product.CompanyID, productVM.Product.Stock, "increase");
                return RedirectToAction("CompanyProducts", "Product");
            }

            return View(productVM.Product);
        }

        public IActionResult Details(int productID)
        {
            Product product = _productService.GetProductByID(productID);
            product.AverageRating = _productService.CalculateProductAverageRating(productID);
            var detailsViewModel = new ProductDetailsViewModel();
            if(_user != null)
            {
                detailsViewModel.HasUserBoughtProduct = _productService.HasUserBoughtProduct(productID, _user.Id);
                detailsViewModel.CurrentUser = _user;
                detailsViewModel.HasUserReviewedProduct = _productService.HasUserReviewedProduct(productID, _user.Id);
            }
            detailsViewModel.Product = product;
            detailsViewModel.Reviews = _productService.GetReviewsOfSpecificProduct(productID).ToList();
            detailsViewModel.Comments = _productService.GetAllComments().ToList();
            detailsViewModel.Likes = _productService.GetLikes().ToList();
            detailsViewModel.Dislikes = _productService.GetDislikes().ToList();
            detailsViewModel.ProductID = productID;
            detailsViewModel.ProductImages = _productService.GetProductImages(productID);
            detailsViewModel.ProductSales = _productService.GetProductSales(productID);
            return View(detailsViewModel);
        }

        public IActionResult CompanyProducts()
        {
            var companyProducts = _productService.GetCompanyProducts(_user.CompanyID);
            var companyProductsDisplay = new ProductDisplayViewModel();
            companyProductsDisplay.Categories = _productService.GetAllCategories();
            if (TempData["FilteredProducts"] != null)
            {
                var filteredProductsJson = TempData["FilteredProducts"].ToString();
                companyProductsDisplay.Products = JsonConvert.DeserializeObject<List<Product>>(filteredProductsJson).Where(x => x.CompanyID == _user.CompanyID);
            }
            else
            {
                companyProductsDisplay.Products = companyProducts;
            }
            return View(companyProductsDisplay);
        }

        public IActionResult ManageProductArchiving(int productID, int option)
        {
            _productService.ManageProductArchiving(productID, option);
            return RedirectToAction("CompanyProducts", "Product");
        }

        public IActionResult CategoryFilter(int categoryID, bool companyProductsFilter = false)
        {
            var filteredProducts = _productService.CategoryFilter(categoryID);
            TempData["FilteredProducts"] = JsonConvert.SerializeObject(filteredProducts); 
            return companyProductsFilter ? RedirectToAction("CompanyProducts", "Product") : RedirectToAction("ProductsDisplay", "Product");
        }

        public IActionResult CompanyFilter(int companyID)
        {
            var filteredProducts = _productService.GetCompanyProducts(companyID);
            TempData["FilteredProducts"] = JsonConvert.SerializeObject(filteredProducts); 
            return RedirectToAction("ProductsDisplay", "Product");
        }

        public IActionResult PriceFilter(string order, bool companyProductsFilter = false)
        {
            var filteredProducts = _productService.PriceFilter(order);
            TempData["FilteredProducts"] = JsonConvert.SerializeObject(filteredProducts); 
            return companyProductsFilter ? RedirectToAction("CompanyProducts", "Product") : RedirectToAction("ProductsDisplay", "Product");
        }

        public IActionResult RatingFilter(string order, bool companyProductsFilter = false)
        {
            var filteredProducts = _productService.RatingFilter(order);
            TempData["FilteredProducts"] = JsonConvert.SerializeObject(filteredProducts);
            return companyProductsFilter ? RedirectToAction("CompanyProducts", "Product") : RedirectToAction("ProductsDisplay", "Product");
        }
    }
}
