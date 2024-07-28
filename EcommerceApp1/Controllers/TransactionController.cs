﻿using EcommerceApp1.Helpers;
using EcommerceApp1.Helpers.Enums;
using EcommerceApp1.Models;
using EcommerceApp1.Models.Identity;
using EcommerceApp1.Models.ViewModels;
using EcommerceApp1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApp1.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;
        private readonly UserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly CouponService _couponService;
        private readonly AppUser _currentUser;

        public TransactionController(TransactionService transactionService, UserService userService, UserManager<AppUser> userManager, ShoppingCartService shoppingCartService, CouponService couponService)
        {
            _transactionService = transactionService;
            _userService = userService;
            _userManager = userManager;
            _shoppingCartService = shoppingCartService;
            _couponService = couponService;
            _currentUser = userService.GetCurrentUser();
        }

        public IActionResult UserTransactions(int userID)
        {
            var userTransactionsViewModel = new UserTransactionsViewModel();
            userTransactionsViewModel.UserTransactions = _transactionService.GetTransactionsByUserID(userID);
            userTransactionsViewModel.UserRefunds = _transactionService.GetAllUserRefunds(userID);
            return View(userTransactionsViewModel);
        }

        [HttpGet]
        public IActionResult Create(string couponCode = null)
        {
            var transactionViewModel= new TransactionViewModel();
            transactionViewModel.Transaction = new Transaction();
            Transaction currentTransaction = transactionViewModel.Transaction;
            currentTransaction.CouponCode = couponCode;
            double cartTotal = _shoppingCartService.CalculateCartTotal();
            IEnumerable<CartItem> cartItems = _shoppingCartService.GetCartItems();
            _transactionService.CalculateTransactionTotal(cartTotal, currentTransaction, cartItems);
            _couponService.ValidateCoupon(currentTransaction, cartItems, couponCode);
            transactionViewModel.UserCards = _transactionService.GetSpecificUserCards(_currentUser.Id);
            transactionViewModel.CartItems = cartItems;
            transactionViewModel.Categories = _transactionService.GetCategories();
            transactionViewModel.ItemsBought = _shoppingCartService.GetItemsBoughtQuantity();
            transactionViewModel.TransactionTax = _transactionService.CalculateTransactionTax(cartTotal);
            currentTransaction.Total += transactionViewModel.TransactionTax;
            return View(transactionViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TransactionViewModel transactionViewModel)
        {
            Transaction currentTransaction = transactionViewModel.Transaction;
            currentTransaction.UserID = _currentUser.Id;
            transactionViewModel.UserCards = _transactionService.GetSpecificUserCards(_currentUser.Id);
            if (!_currentUser.HasCreditCard)
            {
                return RedirectToAction("Create", "CreditCard");
            }
            currentTransaction.ItemsBought = _shoppingCartService.GetItemsBoughtQuantity();
            bool createdTransaction = _transactionService.Create(currentTransaction);
            if (createdTransaction)
            {
                if(currentTransaction.CouponCode != null)
                {
                   bool decreasedCoupon = _couponService.DecreaseCouponQuantity(currentTransaction.CouponCode, _shoppingCartService.GetCartItems());
                }
                IEnumerable<CartItem> cartItems = _shoppingCartService.GetCartItems();
                for(int i = 0; i < cartItems.Count(); i++)
                {
                    CartItem cartItem = cartItems.ElementAt(i);
                    double itemCost = cartItem.Quantity * cartItem.Product.Price;
                    _transactionService.UpdateProductStock(cartItem.ProductID, cartItem.Quantity);
                    _transactionService.UpdateCompanyProperties(cartItem.Product.CompanyID, itemCost, cartItem.Quantity);
                    _transactionService.CreateTransactionItem(cartItem, currentTransaction.ID);
                }
                bool clearedCart = _shoppingCartService.ClearCart();
                bool validRewardPoints = false;
                if(currentTransaction.PaymentType == PaymentTypes.RewardPoints.ToString())
                {
                    validRewardPoints = _transactionService.ValidatePointsForTransaction(_currentUser.UserRewardPoints,currentTransaction.Total);
                }
                _transactionService.UpdateUserRewardPoints(currentTransaction.Total, _currentUser, validRewardPoints);
                await _userManager.UpdateAsync(_currentUser);
                return RedirectToAction("UserTransactions", "Transaction", new {userID = _currentUser.Id});
            }
            return View(transactionViewModel);
        }

        [HttpGet]
        public IActionResult ValidateCoupon(string couponCode, Transaction transaction = null)
        {
            double cartTotal = _shoppingCartService.CalculateCartTotal();
            transaction.Total = cartTotal;
            IEnumerable<CartItem>cartItems = _shoppingCartService.GetCartItems();
            CouponValidator validatedCoupon = _couponService.ValidateCoupon(transaction, cartItems, couponCode);

            return Json(validatedCoupon);
        }

        public IActionResult TransactionItems(int transactionID)
        {
            List<TransactionItem> transactionItems = _transactionService.GetTransactionItems(transactionID);
            var transaction = _transactionService.GetTransactionByID(transactionID);
            var transactionItemsVM = new TransactionItemsViewModel();
            transactionItemsVM.Transactionitems = transactionItems;
            transactionItemsVM.TransactionTotal = "$" +transaction.Total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            transactionItemsVM.Refunds = _transactionService.GetAllUserRefunds(_currentUser.Id);
            transactionItemsVM.TransactionQuantityBought = transaction.ItemsBought;
            return View(transactionItemsVM);
        }

        [HttpGet]
        public IActionResult CheckRewardPoints(double transactionTotal)
        {
            bool result = _transactionService.ValidatePointsForTransaction(_currentUser.UserRewardPoints, transactionTotal);
            return Json(new { success = result });
        }
    }
}
