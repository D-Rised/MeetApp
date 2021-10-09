using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using MeetingApp.Web.Services;
using MeetingApp.DAL.Models;

namespace MeetingApp.Web.Controllers
{
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly MeetingService _authService;

        public MeetingController(MeetingService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            if (login != "")
            {
                User user = _authService.GetUserByLogin(login);
                return View(user);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }
        public IActionResult CreateMeeting()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            if (login != "")
            {
                CreateEditDatesViewModel modelVM = new CreateEditDatesViewModel();
                Dates dates = new Dates() {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                modelVM.DatesList = new List<Dates>();
                modelVM.title = "Dune";
                modelVM.DatesList.Add(dates);
                return View(modelVM);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }

        [HttpPost]
        public IActionResult CreateMeeting(CreateEditDatesViewModel createEditDatesViewModel, string action)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _authService.GetUserByLogin(login);
            if (action == "create meeting")
            {
                Meeting newMeeting = new Meeting();
                newMeeting.Id = Guid.NewGuid();
                newMeeting.title = createEditDatesViewModel.title;
                newMeeting.datesList = createEditDatesViewModel.DatesList;
                newMeeting.user_Id = user.Id;
                newMeeting.state = "selection";
                return RedirectToAction("SignIn", "Auth");
            }
            else if (action == "add row")
            {
                Dates dates = new Dates()
                {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                createEditDatesViewModel.DatesList.Add(dates);
                return View(createEditDatesViewModel);
            }
            else if (int.TryParse(action, out int index) && index > 0)
            {
                createEditDatesViewModel.DatesList.RemoveAt(index);
                return View(createEditDatesViewModel);
            }
            return RedirectToAction("SignIn", "Auth");
        }
    }

    public class CreateEditMeetingViewModel
    {
        public int Id { get; set; }
        public IList<MeetingViewModel> MeetingList { get; set; }
    }
    public class MeetingViewModel
    {
        public Guid Id { get; set; }
        public string title { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public int user_Id { get; set; }
    }
    public class CreateEditDatesViewModel
    {
        public int Id { get; set; }
        public string title { get; set; }
        public IList<Dates> DatesList { get; set; }
    }
}
