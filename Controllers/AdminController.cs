using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class AdminController : Controller
    {
        private SMSdatabase db = new SMSdatabase();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
       
        // POST: Admin/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public ActionResult Register(AdminRegistration model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    if (model.PhotoFile != null && model.PhotoFile.ContentLength > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            model.PhotoFile.InputStream.CopyTo(ms);
                            model.Photo = ms.ToArray();
                        }
                    }

                    // Check if email exists
                    if (db.CheckEmailExists(model.Email))
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View(model);
                    }

                    // Save to database
                    db.InsertAdmin(model);
                    TempData["Success"] = "Registration successful!";
                    return RedirectToAction("AdminLogin");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred during registration: " + ex.Message);
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: Admin/login
        public ActionResult AdminLogin()
        { 
            return View();
        }
        public ActionResult AdminLoginF(AdminLoginForm model)
        {
            if (ModelState.IsValid)
            {
                // Validate login credentials
                bool isValid = db.ValidateAdminLogin(model.CNIC, model.Password);

                if (isValid)
                {
                    // Store admin CNIC in session for future use
                    Session["AdminCNIC"] = model.CNIC;
                    Session["AdminPass"] = model.Password;
                    
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else
                {

                    TempData["LoginFlag"]="Invalid username and password";
                    return RedirectToAction("AdminLogin");
                }
            }
            return View(model);
        }

    }
}