using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lektion20.Models.Repositories.Abstract;
using Lektion20.Models.Entities;

namespace Lektion20.Controllers
{
    public class HomeController : Controller
    {
        private IRepository<User> _userRepo;
        public HomeController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
