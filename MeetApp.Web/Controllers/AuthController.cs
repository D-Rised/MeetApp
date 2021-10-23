using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MeetApp.DAL.Models;
using Microsoft.AspNetCore.Identity;
using MeetApp.Web.Models;

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
            var authViewModel = new AuthViewModel();
            if (User.Identity is {IsAuthenticated: true})
            {
                return RedirectToAction("MainMenu", "Meet");
            }
            return View(authViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string login, string password, string action)
        {
            var authViewModel = new AuthViewModel();
            
            if (action == "login" && login != null && password != null)
            {
                var user = await _userManager.FindByNameAsync(login);
                if (user == null)
                {
                    authViewModel.alertMessage = "User not found!";
                    return View(authViewModel);
                }

                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("MainMenu", "Meet");
                }

                authViewModel.alertMessage = "Login or password invalid!";
                return View(authViewModel);
            }

            if (action == "register")
            {
                var user = await _userManager.FindByNameAsync(login);
                if (user != null)
                {
                    authViewModel.alertMessage = "User already exist!";
                    return View(authViewModel);
                }

                var newUser = new User
                {
                    UserName = login
                };
                var result = await _userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser, false);
                    return RedirectToAction("MainMenu", "Meet");
                }

                authViewModel.alertMessage = result.ToString();
                return View(authViewModel);
            }
            return View(authViewModel);
        }
        
        [HttpGet]
        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Auth");
        }
    }
}
