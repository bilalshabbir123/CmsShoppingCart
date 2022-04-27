﻿using CmsShoppingCart.Areas.Admin.Data;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare list of Pages
            List<PageVM> pageList;
           
            using(Db db=new Db())
            {
                //Init the List
                pageList = db.Pages.ToArray().OrderBy(x=>x.Sorting).Select(x=>new PageVM(x)).ToList();
            }
            //Return view list
            return View(pageList);
        }

        // GET: Admin/Pages//AddPage
        public ActionResult AddPage()
        {
            return View();
        }
        // POST: Admin/Pages//AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //Declare slug
                string slug;
                //Init pageDTO
                PageDTO dto = new PageDTO();
                //DTO title
                dto.Title = model.Title;
                //Check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                //Make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That Title or Slug Already Exists");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                //save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //Set TempData Message
            TempData["SM"] = "You have added a new page!";
            //Redirect
            return RedirectToAction("AddPage");
            
        }
        // GET: Admin/Pages//EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare pageVM
            PageVM model;
            using(Db db=new Db ())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);
                //Confirm page exits
                if (dto==null)
                {
                    return Content("The page does not exist.");
                }
                //Init pageVM
                model = new PageVM(dto);
            }

            //return view with model
            return View(model);
        }
        // POST: Admin/Pages//EditPage/id
        [HttpPost] 
        public ActionResult EditPage(PageVM model)
        {
            //Check model State
            if (! ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db=new Db ())
            {
                //Get page id
                int id = model.Id;

                //Init slug
                string slug="home";

                //Go the page
                PageDTO dto = db.Pages.Find(id);

                //DTO the title
                dto.Title = model.Title;

                //Check for slug and set it if need be
                if (model.Slug !="home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                //Make sure title and slug are unique
                if (db.Pages.Where(x=>x.Id !=id).Any(x=>x.Title==model.Title)||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                //Save the DTO
                db.SaveChanges();
                //Set TempData message
                TempData["SM"] = "You have edited the Page";
                //Redirect
                return RedirectToAction("EditPage");
            }
           
        }

        // POST: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Declare PageVM
            PageVM model;
            
            using (Db db=new Db ())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);
                //Confirm page exists
                if (dto==null)
                {
                    return Content("The Page does not exist.");
                }
                //Init PgaeVM
                model = new PageVM(dto);
                //Return View With Model
            }

            return View(model);
        }
        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db=new Db ())
            {
                //Get the Page
                PageDTO dto = db.Pages.Find(id);
                //Remove the Page
                db.Pages.Remove(dto);
                //Save
                db.SaveChanges();
              
            }
            //Redirect
            return RedirectToAction("Index");
        }
        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db=new Db ())
            {
                //Set initial count
                int count = 1;
                //Declare PageDTO
                PageDTO dto;
                //Set sorting for each page
                foreach (var pageid in id)
                {
                    dto = db.Pages.Find(pageid);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }
        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSideBar()
        {
            //Declare model
            SidebarVM model;
            using(Db db=new Db ())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);
                //Init model
                model = new SidebarVM(dto);
            }

            return View(model);
        }
        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSideBar(SidebarVM model)
        {
            using(Db db=new Db ())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }


            //Set TempData message
            TempData["SM"] = "You have Edited the Sidebar";

            //Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}