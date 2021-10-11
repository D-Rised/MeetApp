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
        private readonly MeetingService _meetingService;

        public MeetingController(MeetingService authService)
        {
            _meetingService = authService;
        }

        public IActionResult MainMenu()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            if (User.Identity.IsAuthenticated)
            {
                MainMenuViewModel modelVM = new MainMenuViewModel();
                modelVM.ownedMeetings = _meetingService.GetAllOwnedMeetingsForUser(user);
                modelVM.memberMeetings = _meetingService.GetAllMemberMeetingsForUser(user);
                modelVM.userLogin = user.login;
                return View(modelVM);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }

        [HttpPost]
        public IActionResult MainMenu(string action, string Id)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            if (User.Identity.IsAuthenticated)
            {
                if (Guid.TryParse(Id, out Guid guidId))
                {
                    if (_meetingService.JoinMeeting(user, guidId) == "joined")
                    {
                        ViewBag.Message = string.Format("You joined to meeting!");
                    }
                    else if (_meetingService.JoinMeeting(user, guidId) == "user already exist")
                    {
                        ViewBag.Message = string.Format("You already joined to this meeting!");
                    }
                    else
                    {
                        ViewBag.Message = string.Format("No meeting found for this invite key!");
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("No meeting found for this invite key!");
                }
                return MainMenu();
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
                newMeeting.membersList = new List<Member>() { new Member() {
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

            if (meetings.Count > id)
            {
                List<User> users = _meetingService.GetAllUsers();
                List<MemberViewModel> memberVMList = new List<MemberViewModel>();
                foreach (var userItem in users)
                {
                    foreach (var memberItem in meetings[id].membersList)
                    {
                        if (userItem.Id == memberItem.userId)
                        {
                            MemberViewModel memberVM = new MemberViewModel();
                            memberVM.login = userItem.login;
                            memberVM.role = memberItem.role;
                            memberVMList.Add(memberVM);
                        }
                    }
                }
                SetupMeetingViewModel setupMeetingViewModel = new SetupMeetingViewModel();
                setupMeetingViewModel.meetingId = meetings[id].Id;
                setupMeetingViewModel.title = meetings[id].title;
                setupMeetingViewModel.userLogin = user.login;
                setupMeetingViewModel.DatesList = meetings[id].datesList;
                setupMeetingViewModel.MembersList = memberVMList;
                return View(setupMeetingViewModel);
            }
            return RedirectToAction("MainMenu", "Meeting");
        }

        [HttpPost]
        public IActionResult SetupMeeting(SetupMeetingViewModel setupMeetingViewModel, string meetingId, int dateId, int memberId, string decision)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            List<Meeting> meetings = _meetingService.GetAllOwnedMeetingsForUser(user);

            if (decision == "update meeting")
            {
                if (Guid.TryParse(meetingId, out Guid meetingIdGuid))
                {
                    foreach (var meeting in meetings)
                    {
                        if (meeting.Id == meetingIdGuid)
                        {
                            meeting.title = setupMeetingViewModel.title;
                            meeting.datesList = setupMeetingViewModel.DatesList;
                            _meetingService.SaveAll();
                            ViewBag.Message = string.Format("Meeting successfully updated!");
                        }
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("Error!");
                }

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
                setupMeetingViewModel.DatesList.RemoveAt(dateId);

                return View(setupMeetingViewModel);
            }
            else if (decision == "kick member")
            {
                if (setupMeetingViewModel.MembersList[memberId].role != "owner")
                {
                    User memberToDelete = _meetingService.GetUserByLogin(setupMeetingViewModel.MembersList[memberId].login);

                    if (Guid.TryParse(meetingId, out Guid meetingIdGuid))
                    {
                        foreach (var meeting in meetings)
                        {
                            if (meeting.Id == meetingIdGuid)
                            {
                                foreach (var member in meeting.membersList.ToList())
                                {
                                    if (member.userId == memberToDelete.Id)
                                    {
                                        _meetingService.DeleteMember(member);
                                        setupMeetingViewModel.MembersList.RemoveAt(memberId);
                                    }
                                }
                            }

                            _meetingService.SaveAll();
                            ViewBag.Message = string.Format("Member removed!");
                        }
                    }
                    else
                    {
                        ViewBag.Message = string.Format("Error!");
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("You cant kick owner!");
                }
                return View(setupMeetingViewModel);
            }
            else if (decision == "calculate meeting")
            {
                _meetingService.SaveAll();

                return View(setupMeetingViewModel);
            }
            else if (decision == "delete meeting")
            {
                if (Guid.TryParse(meetingId, out Guid meetingIdGuid))
                {
                    foreach (var meeting in meetings)
                    {
                        if (meeting.Id == meetingIdGuid)
                        {
                            _meetingService.DeleteMeeting(meeting);
                            ViewBag.Message = string.Format("Meeting successfully deleted!");
                        }
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("Error!");
                }

                return RedirectToAction("MainMenu", "Meeting");
            }
            else
            {
                return RedirectToAction("MainMenu", "Meeting");
            }
        }

        public IActionResult OpenMeeting(int id)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetingService.GetUserByLogin(login);
            List<Meeting> meetings = _meetingService.GetAllMemberMeetingsForUser(user);

            if (meetings.Count > id)
            {
                List<User> users = _meetingService.GetAllUsers();
                List<MemberViewModel> memberVMList = new List<MemberViewModel>();
                foreach (var userItem in users)
                {
                    foreach (var memberItem in meetings[id].membersList)
                    {
                        if (userItem.Id == memberItem.userId)
                        {
                            MemberViewModel memberVM = new MemberViewModel();
                            memberVM.login = userItem.login;
                            memberVM.role = memberItem.role;
                            memberVMList.Add(memberVM);
                        }
                    }
                }
                SetupMeetingViewModel setupMeetingViewModel = new SetupMeetingViewModel();
                setupMeetingViewModel.meetingId = meetings[id].Id;
                setupMeetingViewModel.title = meetings[id].title;
                setupMeetingViewModel.userLogin = user.login;
                setupMeetingViewModel.DatesList = meetings[id].datesList;
                setupMeetingViewModel.MembersList = memberVMList;
                return View(setupMeetingViewModel);
            }
            return RedirectToAction("MainMenu", "Meeting");
        }
    }

    public class MainMenuViewModel
    {
        public string userLogin { get; set; }
        public IList<Meeting> ownedMeetings { get; set; }
        public IList<Meeting> memberMeetings { get; set; }
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
        public IList<MemberViewModel> MembersList { get; set; }
    }
    public class MemberViewModel
    {
        public string login { get; set; }
        public string role { get; set; }
    }
}
