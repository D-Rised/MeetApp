﻿using Microsoft.AspNetCore.Authorization;
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
        private readonly MeetingService _meetingService;

        public MeetingController(MeetingService authService)
        {
            _meetingService = authService;
        }

        [Authorize]
        public IActionResult MainMenu()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            if (User.Identity.IsAuthenticated)
            {
                MainMenuViewModel modelVM = new MainMenuViewModel();
                modelVM.ownedMeetings = _meetingService.GetAllOwnedMeetingsForUser(user);
                modelVM.guestMeetings = _meetingService.GetAllMemberMeetingsForUser(user);
                modelVM.userLogin = user.login;
                return View(modelVM);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }
        public IActionResult CreateMeeting()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            if (login != "")
            {
                CreateMeetingViewModel modelVM = new CreateMeetingViewModel();
                Dates dates = new Dates() {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                modelVM.DatesList = new List<Dates>();
                modelVM.title = "Dune";
                modelVM.userLogin = user.login;
                modelVM.DatesList.Add(dates);
                return View(modelVM);
            }
            else
            {
                return RedirectToAction("MainMenu", "Meeting");
            }
        }

        [HttpPost]
        public IActionResult CreateMeeting(CreateMeetingViewModel createEditDatesViewModel, string action)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            if (action == "create meeting")
            {
                Meeting newMeeting = new Meeting();
                newMeeting.Id = Guid.NewGuid();
                newMeeting.title = createEditDatesViewModel.title;
                newMeeting.datesList = createEditDatesViewModel.DatesList;
                newMeeting.membersList = new List<Members>() { new Members() {
                    userId = user.Id,
                    meetingId = newMeeting.Id,
                    role = "owner"
                }};
                newMeeting.state = "selection";

                _meetingService.CreateNewMeeting(newMeeting);

                return RedirectToAction("CreatedMeeting", "Meeting", new { @meetingId = newMeeting.Id });
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

        public IActionResult CreatedMeeting(Guid meetingId)
        {
            return View(meetingId);
        }

        public IActionResult SetupMeeting(int id)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            List<Meeting> meetings = _meetingService.GetAllOwnedMeetingsForUser(user);

            SetupMeetingViewModel setupMeetingViewModel = new SetupMeetingViewModel();
            setupMeetingViewModel.meetingId = meetings[id].Id;
            setupMeetingViewModel.title = meetings[id].title;
            setupMeetingViewModel.userLogin = user.login;
            setupMeetingViewModel.DatesList = meetings[id].datesList;
            return View(setupMeetingViewModel);
        }

        [HttpPost]
        public IActionResult SetupMeeting(SetupMeetingViewModel setupMeetingViewModel, int id, string decision)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            List<Meeting> meetings = _meetingService.GetAllOwnedMeetingsForUser(user);

            if (decision == "update meeting")
            {
                meetings[id].title = setupMeetingViewModel.title;
                Debug.WriteLine(meetings[id].datesList.Count);
                Debug.WriteLine(setupMeetingViewModel.DatesList.Count);
                meetings[id].datesList = setupMeetingViewModel.DatesList;
                _meetingService.SaveAll();

                return View(setupMeetingViewModel);
            }
            else if (decision == "add date")
            {
                Dates dates = new Dates()
                {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                setupMeetingViewModel.DatesList.Add(dates);

                return View(setupMeetingViewModel);
            }
            else if (decision == "remove date")
            {
                setupMeetingViewModel.DatesList.RemoveAt(id);

                return View(setupMeetingViewModel);
            }
            else if (decision == "calculate meeting")
            {
                meetings[id].title = setupMeetingViewModel.title;
                meetings[id].datesList = setupMeetingViewModel.DatesList;
                _meetingService.SaveAll();

                return View(setupMeetingViewModel);
            }
            else if (decision == "delete meeting")
            {
                _meetingService.DeleteMeeting(meetings[id]);

                return RedirectToAction("MainMenu", "Meeting");
            }
            else
            {
                return RedirectToAction("MainMenu", "Meeting");
            }
        }
    }

    public class MainMenuViewModel
    {
        public string userLogin { get; set; }
        public IList<Meeting> ownedMeetings { get; set; }
        public IList<Meeting> guestMeetings { get; set; }
    }
    public class CreateMeetingViewModel
    {
        public string title { get; set; }
        public string userLogin { get; set; }
        public IList<Dates> DatesList { get; set; }
    }
    public class SetupMeetingViewModel
    {
        public Guid meetingId { get; set; }
        public string title { get; set; }
        public string userLogin { get; set; }
        public IList<Dates> DatesList { get; set; }
    }
}
