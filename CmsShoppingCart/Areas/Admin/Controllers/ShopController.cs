using CmsShoppingCart.Areas.Admin.Data;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        public WebImage WebImage { get; private set; }

        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Declare a list of models
            List<CategoryVM> CategoryVMList;
            using (Db db=new Db ())
            {
                //Init the listx
                CategoryVMList = db.Categories.
                                    ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x)).ToList();
                //Return View with list
            }


            return View(CategoryVMList);
        }
        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string  AddNewCategory(string catName)
        {
            // Declare id
            string id;
            using (Db db=new Db ())
            {
                // Check the category name is unique
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";
                // Init DTO
                CategoryDTO dto = new CategoryDTO();
                // Add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                // Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();
                // Get the id
                id = dto.Id.ToString();
            }

            // Return id
            return id;
        }
        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Set initial count
                int count = 1;
                //Declare PageDTO
                CategoryDTO dto;
                //Set sorting for each category
                foreach (var catid in id)
                {
                    dto = db.Categories.Find(catid);
                    dto.Sorting = count;
                    db.SaveChanges();

                    count++;
                }
            }

        }
        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Get the Category
                CategoryDTO dto = db.Categories.Find(id);
                //Remove the Category
                db.Categories.Remove(dto);
                //Save
                db.SaveChanges();

            }
            //Redirect
            return RedirectToAction("Categories");
        }
        // POST: Admin/Shop/RenameCategory/id
        public string RenameCategory(string newCatName,int id)
        {
            using (Db db=new Db ())
            {
                //check category name is unique
                if (db.Categories.Any(x=>x.Name==newCatName))
                {
                    return "titletaken";
                }
                //Get DTO
                CategoryDTO dto = db.Categories.Find(id);
                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                //Save
                db.SaveChanges();
            }
            //Returns
            return "ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Init model
            ProductVM model = new ProductVM();
            //Add Select List of Categories to model
            using(Db db=new Db ())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //Return View with model
            return View(model);
        }
        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model,HttpPostedFileBase file)
        {
            //check model state
            if(! ModelState.IsValid)
            {
                using (Db db=new Db ())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
                
            }

            //Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x=>x.Name==model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That Product Name is Taken!");
                    return View(model);
                }
               
            }
            //Declare Product Id
            int id;
            //Init and save productID
            using(Db db=new Db ())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // Get the id
                id = product.Id;
            }
            // Set TempData message
            TempData["SM"] = "You Have added a Product!";
            //Set TempData Message
            #region Upload Image
            // Create Necessary  Directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));
            //Check if a file was uploaded
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString() );
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString()+"\\Thumbs" );
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString()+"\\Gallery" );
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString()+"\\Gallery\\Thumbs" );

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Check if a file was uploaded
            if (file !=null && file.ContentLength>0)
            {
                //Get file extension
                string ext = file.ContentType.ToLower();

                //verify extension
                if (ext !="image/jpg"&&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x.png" &&
                    ext != "image/x.png")
                {
                    using (Db db = new Db())
                    {
                        {
                            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            ModelState.AddModelError("", "The Image was not uploded - Wrong image extension.");
                            return View(model);
                        }

                    }
                }
                //Init image name
                string imageName = file.FileName;
                //Save image name to DTO
                using (Db db=new Db ())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                //Set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                //Save oiginal
                file.SaveAs(path);
                //Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Redirect
            return RedirectToAction("AddProduct");
        }
        
        // GET: Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            //Declare a list of ProductsVM
            List<ProductVM> listofProductVM;

            //Set page number
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                //Init the list
                listofProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                //Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //Set Selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            //set pagination
            var onePageOfProducts = listofProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;
            //return view with list
            return View(listofProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Declare productVM
            ProductVM model;

            using(Db db=new Db ())
            {
                // Get the product
                ProductDTO dto = db.Products.Find(id);
                //Make sure product exists
                if (dto==null)
                {
                    return Content("That Product does not exists.");
                }
                // init model
                model = new ProductVM(dto);

                //Make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                // Get all gallery Images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn =>Path.GetFileName(fn));
            }


            // Return view with model
            return View(model);
        }

        // POST: Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model,HttpPostedFileBase file)
        {
            // Get product id
            int id = model.Id;

            //Populate Categories select list and gallery Images
            using(Db db=new Db ())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));
            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Make sure product name is unique
            using (Db db=new Db ())
            {
                if (db.Products.Where(x=>x.Id!=id).Any(x=>x.Name==model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Update Product
            using(Db db=new Db ())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited the product!";

            #region Image Upload

            // Check for file upload
            if (file !=null && file.ContentLength >0)
            {
                // Get Extension
                string ext = file.ContentType.ToLower();

                // Verify extension
                  if (ext !="image/jpg"&&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x.png" &&
                    ext != "image/x.png")
                {
                    using (Db db = new Db())
                    {
                        {                           
                            ModelState.AddModelError("", "The Image was not uploded - Wrong image extension.");
                            return View(model);
                        }

                    }
                }
                // Set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                //Check if a file was uploaded

                
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // Delete files from directories

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);
                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();
                foreach (FileInfo file3 in di2.GetFiles())
                {
                    file3.Delete();
                }
                // Save image name

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                // Save original and thumb Images

                    var path = string.Format("{0}\\{1}", pathString1, imageName);
                    var path2 = string.Format("{0}\\{1}", pathString2, imageName);
    
                    file.SaveAs(path);
                       
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);         
            }

            #endregion

            //Redirect

            return RedirectToAction("EditProduct");

        }

        // GET: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            // Delete product from DB
            using(Db db=new Db ())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            // Delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString,true);

            
            // Redirect
            return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Loop through file
            foreach (string fileName in Request.Files)
            {

                // Init the file
                HttpPostedFileBase file = Request.Files[fileName];

                // Check it's not null
                if (file != null && file.ContentLength > 0) {

                    // Set Directory Paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                     
                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");
                    // Set image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName); 

                    // Save original and thumb

                    file.SaveAs(path);  
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);
            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            // Init list of OrdersForAdmin
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();
            using (Db db=new Db ())
            {
                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init product dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    // Declare total
                    decimal total = 0m;
                    // Init list of OrderDetailDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    // Get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;
                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;
                        // Get product Name
                        string productname = product.Name;
                        // Add to product dict
                        productsAndQty.Add(productname,orderDetails.Quantity);
                        // Get total
                        total += orderDetails.Quantity * price;

                    }
                    // Add to ordersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                        {
                        ordernumber=order.OrderId,
                        Username=username,
                        Total=total,
                        ProductsAndQty=productsAndQty,
                        CreatedAt=order.CreatedAt,
                    });
                }


            }
            // return view with OrdersForAdmin list
            return View(ordersForAdmin);
        }

    }
}