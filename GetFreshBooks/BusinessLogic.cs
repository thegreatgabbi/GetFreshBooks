﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace GetFreshBooks
{
    using GetFreshBooks.Models;
    public static class BusinessLogic
    {
        static Mybooks context = new Mybooks();

        public static List<Book> GetAllBooks
        {
            get
            {
                return context.Books.ToList<Book>();
            }
        }

        public static List<Category> GetAllCategories
        {
            get
            {
                return context.Categories.ToList<Category>();
            }
        }

        public static void AddToCart(string isbn)
        {

            List<CartBook> cBookList = (List<CartBook>)HttpContext.Current.Session[HttpContext.Current.User.Identity.GetUserId()];

            bool found = false;

            foreach (CartBook c in cBookList)
            {
                if (c.Isbn == isbn)
                {
                    c.Quantity++;
                    found = true;
                    CalculatePrice(c.Price);
                    break;
                }
            }

            if (!found)
            {
                var query = from c in context.Books where c.ISBN == isbn select c;
                Book book = query.First();
                CartBook cbook = new CartBook(book.ISBN, book.Title, book.Author, (double)book.Price);
                cBookList.Add(cbook);
                CalculatePrice(cbook.Price);

            }

            HttpContext.Current.Session[HttpContext.Current.User.Identity.GetUserId()] = cBookList;

        }

        public static void DeleteFromCart(string isbn)
        {
            List<CartBook> cBookList = (List<CartBook>)HttpContext.Current.Session[HttpContext.Current.User.Identity.GetUserId()];
            
            foreach (CartBook c in cBookList)
            {
                if (c.Isbn == isbn)
                {
                    cBookList.Remove(c);
                    HttpContext.Current.Session["total"+ HttpContext.Current.User.Identity.GetUserId()] = 
                        ((double)(HttpContext.Current.Session["total"+ HttpContext.Current.User.Identity.GetUserId()]) - (c.Price * c.Quantity)) - 0 < 0.00 ? 0 : ((double)HttpContext.Current.Session["total"+ HttpContext.Current.User.Identity.GetUserId()] - (c.Price * c.Quantity));
                
                    break;
                }
            }
            
             HttpContext.Current.Session[HttpContext.Current.User.Identity.GetUserId()] = cBookList;
        }

        public static void CheckoutCart()
        {
            List<CartBook> checkoutList = (List<CartBook>)HttpContext.Current.Session[HttpContext.Current.User.Identity.GetUserId()];
            foreach(var book in checkoutList)
            {
                var bookToCheckout = context.Books.Where(a => a.ISBN == book.Isbn).FirstOrDefault();
                if (bookToCheckout != null)
                {
                    bookToCheckout.Stock = bookToCheckout.Stock - book.Quantity;

                }

                context.SaveChanges();
            }

            HttpContext.Current.Session[HttpContext.Current.User.Identity.GetUserId()] = new List<CartBook>();
            HttpContext.Current.Session["total"+ HttpContext.Current.User.Identity.GetUserId()] = 0;
        }

        public static void CalculatePrice(double price)
        {
            HttpContext.Current.Session["total"+ HttpContext.Current.User.Identity.GetUserId()] = Convert.ToDouble(HttpContext.Current.Session["total"+ HttpContext.Current.User.Identity.GetUserId()]) + price;

        }
        
    }
}