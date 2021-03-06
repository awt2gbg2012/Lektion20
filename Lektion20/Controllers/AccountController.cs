﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Lektion20.Models;
using Lektion20.Models.Entities;
using Lektion20.Models.Repositories.Abstract;
using DotNetOpenAuth.ApplicationBlock;
using System.Configuration;
using DotNetOpenAuth.OAuth2;
using System.Net;
using DotNetOpenAuth.ApplicationBlock.Facebook;
using System.IO;

namespace Lektion20.Controllers
{
    public class AccountController : Controller
    {
        private IRepository<User> _userRepo;
        public AccountController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        private static readonly FacebookClient client = new FacebookClient
        {
            ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
            ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(
            ConfigurationManager.AppSettings["facebookAppSecret"]),
        };

        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            IAuthorizationState authorization = client.ProcessUserAuthorization();
            if (authorization == null)
            {
                // Kick off authorization request
                List<string> scope = new List<string> { "read_stream" };
                client.RequestUserAuthorization(scope);
                return View();
            }
            else
            {
                var request = WebRequest.Create("https://graph.facebook.com/me?access_token="
                        + Uri.EscapeDataString(authorization.AccessToken));
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        var graph = FacebookGraph.Deserialize(responseStream);
                        if (graph.Id.ToString() == ConfigurationManager.AppSettings["facebookAppOwnerID"])
                        {
                            PersistLongTermAccessToken(graph.Id, graph.Name, authorization.AccessToken);
                        }
                        FormsAuthentication.SetAuthCookie(graph.Name, false);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        public void PersistLongTermAccessToken(long facebookID, string name, string existingAccessToken)
        {
            var longTermAccessTokenEndPoint = string.Format(@"https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&grant_type=fb_exchange_token&fb_exchange_token={2}",
                ConfigurationManager.AppSettings["facebookAppID"],
                ConfigurationManager.AppSettings["facebookAppSecret"],
                existingAccessToken);
            var request = WebRequest.Create(longTermAccessTokenEndPoint);
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var streamReader = new StreamReader(responseStream);
                    string[] streamResult = streamReader.ReadToEnd().Split('&');
                    string longTermAccessToken = streamResult[0]
                        .Substring(streamResult[0].IndexOf("=") + 1);
                    var user = _userRepo.FindAll(u => u.FacebookID == facebookID).FirstOrDefault();
                    if (null == user)
                        user = new User
                        {
                            ID = Guid.NewGuid(),
                            FacebookID = facebookID,
                            FullName = name,
                            LongTermAccessToken = longTermAccessToken
                        };
                    else
                        user.LongTermAccessToken = longTermAccessToken;
                    _userRepo.Save(user);
                }
            }
}

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
