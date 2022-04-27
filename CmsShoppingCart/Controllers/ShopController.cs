using CmsShoppingCart.Areas.Admin.Data;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index","Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            // Declare list of CategoryVM
            List<CategoryVM> categoryVMList;
            // Init the list
            using (Db db=new Db ())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            // Return partial with list
            return PartialView(categoryVMList);
        }

        // GET: /SHop/Category/Name
        public ActionResult Category(string name)
        {
            // Declare a list of ProductVM
            List<ProductVM> ProductVMLists;
            using (Db db=new Db ())
            {
                // Get Category id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;
                // Init the list
                ProductVMLists = db.Products.ToArray().Where(x => x.CategoryId == catId).
                    Select(x => new ProductVM(x)).ToList();
                // Get Category Name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }
            // Return View with lists
            return View(ProductVMLists);
        }

        // GET: /Shop/Product-details/Name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Declare the VM and DTO
            ProductVM model;
            ProductDTO dto;
            // Init product id
            int id = 0;
            
            using (Db db=new Db ())
            {
                // Check if product exists
                if (! db.Products.Any(x=>x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                // Init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
                // Get Inserted id
                id = dto.Id;
                // Init model
                model = new ProductVM(dto);
            }

            // Get Gallery Images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                   .Select(fn => Path.GetFileName(fn));

            // Return View with model

            return View("ProductDetails", model);
        }
    }
}