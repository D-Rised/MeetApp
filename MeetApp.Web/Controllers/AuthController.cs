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
using MeetApp.Models;

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
            AuthViewModel authVM = new AuthViewModel();
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("MainMenu", "Meet");
            }
            else
            {
                return View(authVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string login, string password, string action)
        {
            AuthViewModel authVM = new AuthViewModel();
            if (action == "login" && login != null && password != null)
            {
                var user = await _userManager.FindByNameAsync(login);
                if (user == null)
                {
                    authVM.alertMessage = "User not found!";
                    return View(authVM);
                }

                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("MainMenu", "Meet");
                }
                else
                {
                    authVM.alertMessage = "Login or password invalid!";
                    return View(authVM);
                }
            }
            else if (action == "register")
            {
                var user = await _userManager.FindByNameAsync(login);
                if (user != null)
                {
                    authVM.alertMessage = "User already exist!";
                    return View(authVM);
                }

                User newUser = new User();
                newUser.UserName = login;
                var result = await _userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser, false);
                    return RedirectToAction("MainMenu", "Meet");
                }
                else
                {
                    authVM.alertMessage = result.ToString();
                    return View(authVM);
                }
            }
            return View(authVM);
        }
        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Auth");
        }
    }
}
