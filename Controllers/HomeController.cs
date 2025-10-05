using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        { // Clear server-side session
            Session.Clear();
            Session.Abandon();
            return View();
        }
    }
}