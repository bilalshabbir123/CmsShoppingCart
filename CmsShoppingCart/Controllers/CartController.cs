using CmsShoppingCart.Areas.Admin.Data;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Init the Cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            // Check if Cart is Empty
            if (cart.Count==0 || Session["cart"]==null)
            {
                ViewBag.Message = "Your Cart is Empty";
                return View();
            }
            // Calcualte total and save to ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;
            // Return View with list
            return View(cart);
        }
        public ActionResult CartPartil()
        {
            // Init CartVM
            CartVM model = new CartVM();
            //Init Quantity
            int qyt = 0;
            // Init Price
            decimal price = 0m;
            // Check for Cart Session
            if (Session["cart"]!=null)
            {
                // Get Total Quantity and Price
                var list = (List<CartVM>)Session["cart"];
                foreach (var item in list)
                {
                    qyt += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = qyt;
                model.Price = price;
            }
            else
            {
                // Or Set Quantity and Price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            // Return Partial View with Model

            return PartialView(model);
        }
        public ActionResult AddToCartPartial(int id)
        {
            // Init CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            // Init CartVM
            CartVM model = new CartVM();

            using (Db db=new Db ())
            {
                // Get the  Product
                ProductDTO product = db.Products.Find(id);

                // Check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // If not, add new
                if (productInCart==null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName=product.Name,
                        Quantity=1,
                        Price=product.Price,
                        Image=product.ImageName
                    });
                }
                else
                {
                    // if it is, increament
                    productInCart.Quantity++;
                }

            }

            //Get total qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = qty;
            model.Price = price;
            //Save cart back to session
            Session["cart"] = cart;

            //return partial view with model
            return PartialView(model);



        }

        // GET: Cart/Increamentproduct
        public ActionResult Increamentproduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db=new Db ())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                // Increament qty
                model.Quantity++;
                // Store needed data
                var result = new {qty=model.Quantity,price=model.Price };
                // Return Json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Cart/Increamentproduct
        public ActionResult DecreamentProduct(int productId)
        {
            // Init cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db=new Db ())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                // Decreament qty
                if (model.Quantity>1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return Json 
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Get : /Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using(Db db=new Db ())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Remove model from list
                cart.Remove(model);
            }
        }
        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
                return PartialView(cart);
        }
        // POST : /Cart/Placeorder
        [HttpPost]
        public void Placeorder()
        {
            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            // Get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db=new Db ())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x=>x.Username==username);
                int userId = q.Id;
                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Init OrderdetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to orderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            // Email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("c62cc9fbb47955", "28e5fb359d2108"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order, order number"+orderId);
            // Reset session
            Session["cart"] = null;

        }
    }
}