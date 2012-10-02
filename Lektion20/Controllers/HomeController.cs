using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lektion20.Models.Repositories.Abstract;
using Lektion20.Models.Entities;
using System.Configuration;
using System.Net;
using DotNetOpenAuth.ApplicationBlock.Facebook;

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
            string result = "";
            var facebookAppOwnerID = ConfigurationManager.AppSettings["facebookAppOwnerID"];
            var appOwner = _userRepo.FindAll(u => u.FacebookID.ToString() == facebookAppOwnerID)
                .FirstOrDefault();
            if (null != appOwner)
            {
                var longTermAccessToken = appOwner.LongTermAccessToken;
                var request = WebRequest.Create(
                    string.Format(@"https://graph.facebook.com/{0}/feed?access_token={1}",
                    facebookAppOwnerID,
                    Uri.EscapeDataString(longTermAccessToken)));
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        var graph = FacebookFeedGraph.Deserialize(responseStream);
                        result = HttpUtility.HtmlEncode(graph.Posts
                            .Where(p => p.From.ID == facebookAppOwnerID)
                            .FirstOrDefault().Message);
                    }
                }
            }
            return View((object)result);
        }
    }
}
