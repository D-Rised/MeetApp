using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MeetApp.DAL.Models;
using MeetApp.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace MeetApp.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("MainMenu", "Meet");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string login, string password, string action)
        {
            if (action == "login" && login != null && password != null)
            {
                var user = await _userManager.FindByNameAsync(login);
                if (user == null)
                {
                    ViewBag.Message = string.Format("User not found!");
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("MainMenu", "Meet");
                }
                else
                {
                    ViewBag.Message = string.Format("Login or password invalid!");
                    return View();
                }
            }
            else if (action == "register")
            {
                var user = await _userManager.FindByNameAsync(login);
                if (user != null)
                {
                    ViewBag.Message = string.Format("User already exist!");
                    return View();
                }

                User newUser = new User();
                newUser.UserName = login;
                var result = await _userManager.CreateAsync(newUser, password);
                Debug.WriteLine(result.Succeeded);
                Debug.WriteLine(result);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser, false);
                    return RedirectToAction("MainMenu", "Meet");
                }
                else
                {
                    ViewBag.Message = string.Format(result.ToString());
                }
            }
            return View();
        }
        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Auth");
        }
    }
}
