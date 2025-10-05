using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class StudentSignupController : Controller
    {
        // GET: StudentSignup
        private SMSdatabase db = new SMSdatabase();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // POST: Admin/Register
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Register(StudentSignup model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email exists
                    if (db.CheckStudentidExists(model.StudentID))
                    {
                        TempData["Success2"] = "This StudentID already exists";
                        return View("Index",model);
                    }

                    // Save to database
                    db.InsertStudentSignup(model);

                    ViewBag.StudentID5 = model.StudentID;
                    ViewBag.StudentPassw5 = model.Password;

                    TempData["Success"] = "Registration successful!";
                    return RedirectToAction("StudentLogin");
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
        public ActionResult StudentLogin()
        {
            return View();
        }
        public ActionResult StudentLoginF(StudentSignup model)
        {
            if (ModelState.IsValid)
            {
                // Validate login credentials
                bool isValid = db.ValidateStudentLogin(model.StudentID, model.Password);

                if (isValid)
                {
                    // Store admin CNIC in session for future use
                    Session["AdminCNIC"] = model.StudentID;
                    Session["AdminPass"] = model.Password;

                    return RedirectToAction("Index", "StudentDashboard");
                }
                else
                {

                    TempData["LoginFlag"] = "Invalid StudentID and password";
                    return RedirectToAction("StudentLogin");
                }
            }
            return View(model);
        }
    }
}