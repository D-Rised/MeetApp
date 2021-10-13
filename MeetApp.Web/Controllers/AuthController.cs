﻿using Microsoft.AspNetCore.Mvc;
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

namespace MeetApp.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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
                User user = _authService.GetUserByLogin(_login);
                if (user != null)
                {
                    if (user.password == _password)
                    {
                        await Authenticate(user);
                        return RedirectToAction("MainMenu", "Meet");
                    }
                    else
                    {
                        ViewBag.Message = string.Format("Login or password invalid!");
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("User not found!");
                }
            }
            else if (action == "register")
            {
                User user = _authService.GetUserByLogin(_login);
                if (user == null)
                {
                    user = new User() { Id = Guid.NewGuid(), login = _login, password = _password };
                    _authService.CreateNewUser(user);
                }
                else
                {
                    ViewBag.Message = string.Format("User already exist!");
                }
            }
            return View();
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.login)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "Cookie");

            await HttpContext.SignInAsync("Cookie", new ClaimsPrincipal(id));
        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync("Cookie");
            return RedirectToAction("SignIn", "Auth");
        }
    }
}