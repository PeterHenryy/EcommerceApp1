﻿using EcommerceApp1.Models.ViewModels;
using EcommerceApp1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;

namespace EcommerceApp1.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ShoppingCartService _shoppingCartService;

        public ShoppingCartController(ShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public IActionResult DisplayCartItems()
        {
            var shoppingCartViewModel = new ShoppingCartViewModel();
            shoppingCartViewModel.CartItems = _shoppingCartService.GetCartItems();
            shoppingCartViewModel.DeliveryOptions = _shoppingCartService.GetDeliveryOptions();
            return View(shoppingCartViewModel);
        }
        [HttpPost]
        public void AddItemToCart(int itemID, int quantity)
        {
            bool addedItem = _shoppingCartService.AddItemToCart(itemID, quantity);
        }

        public IActionResult ClearCart()
        {
            bool clearedCart = _shoppingCartService.ClearCart();
            return RedirectToAction("DisplayCartItems");
        }

        public IActionResult DeleteCartItem(int itemID)
        {
            bool deletedItem = _shoppingCartService.DeleteCartItem(itemID);
            return RedirectToAction("DisplayCartItems");
        }

        [HttpPost]
        public void UpdateCartItemQuantity(int itemID, int quantity)
        {
            bool updatedItem = _shoppingCartService.UpdateCartItemQuantity(itemID, quantity);
        }

        [HttpPost]
        public void RemoveFromCart(int itemID)
        {
            bool removedItem = _shoppingCartService.DeleteCartItem(itemID);
        }

        [HttpPost]
        public void UpdateCartItemShippingOption(int itemID, string newShippingCost, string newShippingOption)
        {
            double shippingCost = double.Parse(newShippingCost, CultureInfo.GetCultureInfo("en-US"));
            bool updatedShippingCost = _shoppingCartService.UpdateCartItemShippingOption(itemID, shippingCost, newShippingOption);
        }
    }
}
