using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Meeting");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult SignIn(string name, string password, string action)
        {
            if (action == "login")
            {
                Debug.WriteLine(name);
                Debug.WriteLine(password);
                Debug.WriteLine(action);
            }
            else if (action == "register")
            {
                Debug.WriteLine(name);
                Debug.WriteLine(password);
                Debug.WriteLine(action);
            }
            return View();
        }
    }
}
