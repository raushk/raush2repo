using System;
using System.Web.Mvc;
using System.Web.Security;
using SimpleBlog.Models;
using SimpleBlog.Helpers;
using SimpleBlog.Infrastructure.Repositories;
using SimpleBlog.Areas.Admin.ViewModels;

namespace SimpleBlog.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var actualUser = _userRepository.FindByLogin(model.UserName);
                if (actualUser == null)
                {
                    //user not exists
                    ModelState.AddModelError("", "The user name provided is incorrect.");
                }
                else
                {
                    string hashedPassword = PasswordHelper.GetHashedPassword(model.Password);

                    if (!actualUser.Password.Equals(hashedPassword))
                    {
                        //password not valid
                        ModelState.AddModelError("", "The password provided is incorrect.");
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                        SessionHelper.Authenticated = actualUser;

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Blog", new { area = string.Empty });
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            SessionHelper.Authenticated = null;
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Blog", new { area = string.Empty });
        }

        public ActionResult Register()
        {
#if DEBUG
            return View();
#endif
#if !DEBUG
            return RedirectToAction("Index", "Blog");
#endif
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
#if DEBUG
            ModelState.Remove("RegisterUser.Password");
            if (ModelState.IsValid)
            {
                string hashedPassword = PasswordHelper.GetHashedPassword(model.Password);
                model.RegisterUser.Password = hashedPassword;

                // Attempt to register the user
                User newUser = model.RegisterUser;

                _userRepository.Add(newUser);

                if (newUser != null && newUser.UserId != 0)
                {
                    FormsAuthentication.SetAuthCookie(newUser.Login, false /* createPersistentCookie */);

                    SessionHelper.Authenticated = newUser;

                    return RedirectToAction("Index", "Blog", new { area = string.Empty });
                }
                else
                {
                    ModelState.AddModelError("", "A user with this Login or Email exists yet.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
#endif
#if !DEBUG
            return RedirectToAction("Index", "Blog");
#endif
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedNewPassword = PasswordHelper.GetHashedPassword(model.NewPassword);
                string hashedOldPassword = PasswordHelper.GetHashedPassword(model.OldPassword);

                var actualUser = SessionHelper.Authenticated;

                if (actualUser.Password.Equals(hashedOldPassword))
                {
                    actualUser.Password = hashedNewPassword;
                    _userRepository.Modify(actualUser);

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

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
    }
}
