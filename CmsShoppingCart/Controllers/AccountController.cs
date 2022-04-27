using CmsShoppingCart.Areas.Admin.Data;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using CmsShoppingCart.Views.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/Account/Login");
        }
        // GET: Account/login
        [HttpGet]
        public ActionResult Login()
        {
            // Confirm user is logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }

            //return View
            return View();
        }
        // GET: Account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Confirm user is logged in
            if (! ModelState.IsValid)
            {
                return View(model);
            }
            // check if user is valid
            bool isvalid = false;
            using (Db db=new Db ())
            {
                if (db.Users.Any(x=>x.Username.Equals(model.Username)&&x.Password.Equals(model.Password)))
                {
                    isvalid = true;
                }
            }
            if (! isvalid)
            {
                ModelState.AddModelError("", "Invalid username or password"); ;
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }
        // GET: Account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // GET: Account/create-account
        [ActionName("create-account")]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateAccount(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model); 
            }

            // Check if password match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("","Passwords do not match");
                return View("CreateAccount", model);
            }
            using(Db db=new Db ())
            {
                // Makesure username is unique
                if (db.Users.Any(x=>x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "UserName"+model.Username+" is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName=model.FirstName,
                    LastName=model.LastName,
                    EmailAddress=model.EmailAddress,
                    Username=model.Username,
                    Password=model.Password
                };
                // Add the DTO
                db.Users.Add(userDTO);
                // Save
                db.SaveChanges();
                // Add to userRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            // Create a TempData message
            TempData["SM"] = "You are now Registered and can login";

            // Redirect
            return View("Login");
        }

        // GET: Account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }


        [Authorize]

        public ActionResult UserNavPartial()
        {
            // Get username
            string username = User.Identity.Name;
            // Declare model
            UserNavPartial model;
            using (Db db=new Db ())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);
                // Build the model
                model = new UserNavPartial()
                {
                    FirstName=dto.FirstName,
                    LastName=dto.LastName
                };
            }

            // Return partial view with model
            return PartialView(model);
        }
        // Get: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model;
            UserProfileVM model;

            using (Db db = new Db() )
            {
                // Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);
                //Build model
                model = new UserProfileVM(dto);
            }

            // Return view with model

            return View("UserProfile", model);
        }
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        [Authorize(Roles="User")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Check model state
            if (! ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("","Passwords do not match");
                    return View("UserProfile",model);
                }
            }
            using (Db db=new Db ())
            {
                // Get username
                string username = User.Identity.Name;
                // Make sure username is unique
                if (db.Users.Where(x=>x.Id!=model.Id).Any(x=>x.Username==username))
                {
                    ModelState.AddModelError("","Username"+model.Username+"already exists");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Edit DTO
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                    // Save
                    db.SaveChanges();
                }
                // Set TempData message
                TempData["SM"] = "You have edited you profile";
            }
            // Redirect
            return Redirect("~/account/user-profile");
        }
        // Get: /account/Orders
        public ActionResult Orders()
        {
            // Init list of OrdersForUserVN
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();
            using (Db db=new Db  ())
            {
                // Get user id
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;
                // Init list of orderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init products dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    // Declare total
                    decimal total = 0m;
                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTOs = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    // Loop through list of orderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTOs)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get Product Price
                        decimal price = product.Price;

                        // Get Product Name
                        string productName = product.Name;

                        // Add to Products dict
                        productsAndQty.Add(productName, orderDetails.Quantity);
                        // Get total
                        total += orderDetails.Quantity * price;

                    }
                    // Add to OrdersForuserVM list
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        ordernumber=order.OrderId,
                        Total=total,
                        ProductsAndQty=productsAndQty,
                        CreatedAt=order.CreatedAt
                    });
                }
            }
            // Return View with list of ordersForuservm
            return View(ordersForUser);
        }
    }
}