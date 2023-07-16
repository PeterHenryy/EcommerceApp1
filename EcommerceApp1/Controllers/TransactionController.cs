﻿using EcommerceApp1.Helpers.Enums;
using EcommerceApp1.Helpers.Payments;
using EcommerceApp1.Models;
using EcommerceApp1.Models.Identity;
using EcommerceApp1.Models.ViewModels;
using EcommerceApp1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EcommerceApp1.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;
        private readonly UserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public TransactionController(TransactionService transactionService, UserService userService, UserManager<AppUser> userManager)
        {
            _transactionService = transactionService;
            _userService = userService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var transactions = _transactionService.GetAllTransactions();
            return View(transactions);
        }

        [HttpGet]
        public IActionResult Create(int productID)
        {
            AppUser currentUser = _userService.GetCurrentUser();
            var transactionViewModel= new TransactionViewModel();
            transactionViewModel.Transaction = new Transaction();
            Transaction currentTransaction = transactionViewModel.Transaction;
            currentTransaction.ProductID = productID;
            currentTransaction.CurrentProduct = _transactionService.GetProductByID(productID);
            transactionViewModel.UserCards = _transactionService.GetSpecificUserCards(currentUser.Id);
            return View(transactionViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TransactionViewModel transactionViewModel)
        {
            AppUser currentUser = _userService.GetCurrentUser();
            Transaction currentTransaction = transactionViewModel.Transaction;
            currentTransaction.UserID = currentUser.Id;
            currentTransaction.Total = _transactionService.CalculateTransactionTotal(currentTransaction);
            bool pointsPayment = currentTransaction.PaymentType == PaymentTypes.RewardPoints.ToString();
            if (pointsPayment)
            {
                var pointsPaymentValidator = new PointsPayment();
                pointsPayment = pointsPaymentValidator.ValidatePointsForTransaction(currentUser, currentTransaction.Total);
                if (!pointsPayment)
                {
                    return View(transactionViewModel);
                }
            }
            else if (!currentUser.HasCreditCard)
            {
                return RedirectToAction("Create", "CreditCard");
            }

            bool createdTransaction = _transactionService.Create(currentTransaction);
            if (createdTransaction)
            {
                await UpdateUserRewardPoints(currentTransaction.Total, currentUser, pointsPayment);
                return RedirectToAction("Index", "Transaction");
            }
            return View(transactionViewModel);
        }

        public async Task UpdateUserRewardPoints(double transactionTotal, AppUser currentUser, bool paidWithPoints = false)
        {
            if (paidWithPoints)
            {
                currentUser.UserRewardPoints -= transactionTotal * 5;                
            }
            else
            {
                var rewardPoints = _userService.CalculateUserRewardPoints(transactionTotal);
                currentUser.UserRewardPoints += rewardPoints;
            }
            await _userManager.UpdateAsync(currentUser);
        }


    }
}
