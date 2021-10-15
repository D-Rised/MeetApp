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
        //private readonly AuthService _authService;

        //public AuthController(AuthService authService)
        //{
        //    _authService = authService;
        //}

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

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
        public async Task<IActionResult> SignIn(string _login, string _password, string action)
        {
            if (action == "login")
            {
                //User user = _authService.GetUserByLogin(_login);
                var user = await _userManager.FindByNameAsync(_login);
                if (user == null)
                {
                    ViewBag.Message = string.Format("User not found!");
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(user, _password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("MainMenu", "Meet");
                }
                else
                {
                    ViewBag.Message = string.Format("Login or password invalid!");
                    return View();
                }

                //if (_signInManager.Sign == _password)
                //{
                //    await Authenticate(user);
                //    return RedirectToAction("MainMenu", "Meet");
                //}
                //else
                //{
                //    ViewBag.Message = string.Format("Login or password invalid!");
                //}

            }
            else if (action == "register")
            {
                var user = await _userManager.FindByNameAsync(_login);
                if (user != null)
                {
                    ViewBag.Message = string.Format("User already exist!");
                    return View();
                }

                User newUser = new User();
                newUser.UserName = _login;
                var result = await _userManager.CreateAsync(newUser, _password);
                Debug.WriteLine(result.Succeeded);
                Debug.WriteLine(result);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser, false);
                    return RedirectToAction("MainMenu", "Meet");
                    //await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Name, _login));
                }
                else
                {
                    ViewBag.Message = string.Format(result.ToString());
                }

                //User user = _authService.GetUserByLogin(_login);
                //if (user == null)
                //{
                //    user = new User() { Id = Guid.NewGuid(), login = _login, password = _password };
                //    _authService.CreateNewUser(user);
                //}
                //else
                //{
                //    ViewBag.Message = string.Format("User already exist!");
                //}
            }
            return View();
        }

        //private async Task Authenticate(User user)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName)
        //    };

        //    ClaimsIdentity id = new ClaimsIdentity(claims, "Cookie");

        //    await HttpContext.SignInAsync("Cookie", new ClaimsPrincipal(id));
        //}

        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Auth");
        }
    }
}
