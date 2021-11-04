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
        public IActionResult SignIn(AuthViewModel authViewModel)
        {
            if (User.Identity is { IsAuthenticated: true })
                return RedirectToAction("Index", "Meet");

            return View(authViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel authViewModel)
        {
            var login = authViewModel.Login;
            var password = authViewModel.Password;

            if (login == null || password == null)
            {
                authViewModel.alertMessage = "Incorrect login or password!";
                return RedirectToAction("SignIn", "Auth", authViewModel);
            }

            var user = await _userManager.FindByNameAsync(login);

            if (user == null)
            {
                authViewModel.alertMessage = "User not found!";
                return RedirectToAction("SignIn", "Auth", authViewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Meet");
            }

            authViewModel.alertMessage = "Login or password invalid!";
            return RedirectToAction("SignIn", "Auth", authViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(AuthViewModel authViewModel)
        {
            var login = authViewModel.Login;
            var password = authViewModel.Password;

            var user = await _userManager.FindByNameAsync(login);

            if (user != null)
            {
                authViewModel.alertMessage = "User already exist!";
                return RedirectToAction("SignIn", "Auth", authViewModel);
            }

            var newUser = new User
            {
                UserName = login
            };

            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(newUser, false);
                return RedirectToAction("Index", "Meet");
            }

            authViewModel.alertMessage = result.ToString();
            return RedirectToAction("SignIn", "Auth", authViewModel);
        }

        [HttpGet]
        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Auth");
        }
    }
}
