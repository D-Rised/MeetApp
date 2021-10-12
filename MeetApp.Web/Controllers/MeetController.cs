using MeetApp.DAL.Models;
using MeetApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MeetApp.Web.Controllers
{
    [Authorize]
    public class MeetController : Controller
    {
        private readonly MeetService _meetService;

        public MeetController(MeetService authService)
        {
            _meetService = authService;
        }

        public IActionResult MainMenu()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            if (User.Identity.IsAuthenticated)
            {
                MainMenuViewModel modelVM = new MainMenuViewModel();
                modelVM.ownedMeets = _meetService.GetAllOwnedMeetsForUser(user);
                modelVM.memberMeets = _meetService.GetAllMemberMeetsForUser(user);
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
            User user = _meetService.GetUserByLogin(login);
            if (User.Identity.IsAuthenticated)
            {
                if (Guid.TryParse(Id, out Guid guidId))
                {
                    if (_meetService.JoinMeet(user, guidId) == "joined")
                    {
                        ViewBag.Message = string.Format("You joined to meet!");
                    }
                    else if (_meetService.JoinMeet(user, guidId) == "user already exist")
                    {
                        ViewBag.Message = string.Format("You already joined to this meet!");
                    }
                    else
                    {
                        ViewBag.Message = string.Format("No meet found for this invite key!");
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("No meet found for this invite key!");
                }
                return MainMenu();
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }

        public IActionResult CreateMeet()
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            if (login != "")
            {
                CreateMeetViewModel modelVM = new CreateMeetViewModel();
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
                return RedirectToAction("MainMenu", "Meet");
            }
        }

        [HttpPost]
        public IActionResult CreateMeet(CreateMeetViewModel createEditDatesViewModel, string action)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            if (action == "create meet")
            {
                Meet newMeet = new Meet();
                newMeet.Id = Guid.NewGuid();
                newMeet.title = createEditDatesViewModel.title;
                newMeet.datesList = createEditDatesViewModel.DatesList;
                foreach (var dates in newMeet.datesList.ToList())
                {
                    dates.memberId = user.Id;
                }
                newMeet.membersList = new List<Member>() { new Member() {
                    userId = user.Id,
                    meetId = newMeet.Id,
                    role = "owner",
                    state = "Ready",
                }};
                newMeet.state = "Selection";

                _meetService.CreateNewMeet(newMeet);

                return RedirectToAction("CreatedMeet", "Meet", new { @meetId = newMeet.Id });
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

        public IActionResult CreatedMeet(Guid meetId)
        {
            return View(meetId);
        }

        public IActionResult SetupMeet(int id)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllOwnedMeetsForUser(user);

            if (meets.Count > id)
            {
                List<User> users = _meetService.GetAllUsers();
                List<MemberViewModel> memberVMList = new List<MemberViewModel>();
                SetupMeetViewModel setupMeetViewModel = new SetupMeetViewModel();
                foreach (var userItem in users)
                {
                    foreach (var memberItem in meets[id].membersList)
                    {
                        if (userItem.Id == memberItem.userId)
                        {
                            MemberViewModel memberVM = new MemberViewModel();
                            memberVM.login = userItem.login;
                            memberVM.role = memberItem.role;
                            memberVM.state = memberItem.state;
                            memberVMList.Add(memberVM);
                            if (memberItem.userId == user.Id)
                            {
                                setupMeetViewModel.memberState = memberItem.state;
                            }
                        }
                    }
                }
                setupMeetViewModel.meetId = meets[id].Id;
                setupMeetViewModel.title = meets[id].title;
                setupMeetViewModel.userLogin = user.login;
                setupMeetViewModel.state = meets[id].state;
                setupMeetViewModel.DatesList = meets[id].datesList;
                setupMeetViewModel.MembersList = memberVMList;
                return View(setupMeetViewModel);
            }
            return RedirectToAction("MainMenu", "Meet");
        }

        [HttpPost]
        public IActionResult SetupMeet(SetupMeetViewModel setupMeetViewModel, string meetId, int dateId, int memberId, string decision)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllOwnedMeetsForUser(user);

            if (decision == "update meet")
            {
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid)
                        {
                            meet.title = setupMeetViewModel.title;
                            foreach (var dates in meet.datesList.ToList())
                            {
                                if (dates.memberId == user.Id)
                                {
                                    meet.datesList.Remove(dates);
                                }
                            }
                            foreach (var dates in setupMeetViewModel.DatesList)
                            {
                                meet.datesList.Add(dates);
                            }
                            foreach (var dates in meet.datesList.ToList())
                            {
                                dates.memberId = user.Id;
                            }
                            _meetService.SaveAll();
                            ViewBag.Message = string.Format("Meet successfully updated!");
                        }
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("Error!");
                }

                return View(setupMeetViewModel);
            }
            else if (decision == "add date")
            {
                Dates dates = new Dates()
                {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                setupMeetViewModel.DatesList.Add(dates);

                return View(setupMeetViewModel);
            }
            else if (decision == "remove date")
            {
                setupMeetViewModel.DatesList.RemoveAt(dateId);

                return View(setupMeetViewModel);
            }
            else if (decision == "kick member")
            {
                if (setupMeetViewModel.MembersList[memberId].role != "owner")
                {
                    User memberToDelete = _meetService.GetUserByLogin(setupMeetViewModel.MembersList[memberId].login);

                    if (Guid.TryParse(meetId, out Guid meetIdGuid))
                    {
                        foreach (var meet in meets)
                        {
                            if (meet.Id == meetIdGuid)
                            {
                                foreach (var member in meet.membersList.ToList())
                                {
                                    if (member.userId == memberToDelete.Id)
                                    {
                                        _meetService.DeleteMember(member);
                                        setupMeetViewModel.MembersList.RemoveAt(memberId);
                                    }
                                }
                            }

                            _meetService.SaveAll();
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
                return View(setupMeetViewModel);
            }
            else if (decision == "calculate meet")
            {
                _meetService.SaveAll();

                return View(setupMeetViewModel);
            }
            else if (decision == "delete meet")
            {
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid)
                        {
                            _meetService.DeleteMeet(meet);
                            ViewBag.Message = string.Format("Meet successfully deleted!");
                        }
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("Error!");
                }

                return RedirectToAction("MainMenu", "Meet");
            }
            else
            {
                return RedirectToAction("MainMenu", "Meet");
            }
        }

        public IActionResult OpenMeet(int id)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllMemberMeetsForUser(user);

            if (meets.Count > id)
            {
                List<User> users = _meetService.GetAllUsers();
                List<MemberViewModel> memberVMList = new List<MemberViewModel>();
                SetupMeetViewModel setupMeetViewModel = new SetupMeetViewModel();
                foreach (var userItem in users)
                {
                    foreach (var memberItem in meets[id].membersList)
                    {
                        if (userItem.Id == memberItem.userId)
                        {
                            MemberViewModel memberVM = new MemberViewModel();
                            memberVM.login = userItem.login;
                            memberVM.role = memberItem.role;
                            memberVM.state = memberItem.state;
                            memberVMList.Add(memberVM);

                            if (memberItem.userId == user.Id)
                            {
                                setupMeetViewModel.memberState = memberItem.state;
                            }
                        }
                    }
                }
                setupMeetViewModel.meetId = meets[id].Id;
                setupMeetViewModel.title = meets[id].title;
                setupMeetViewModel.userLogin = user.login;
                setupMeetViewModel.state = meets[id].state;
                setupMeetViewModel.DatesList = meets[id].datesList;
                setupMeetViewModel.MembersList = memberVMList;
                return View(setupMeetViewModel);
            }
            return RedirectToAction("MainMenu", "Meet");
        }

        [HttpPost]
        public IActionResult OpenMeet(SetupMeetViewModel setupMeetViewModel, string meetId, int dateId, string decision)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllMemberMeetsForUser(user);

            if (decision == "confirm")
            {
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid)
                        {
                            foreach (var member in meet.membersList.ToList())
                            {
                                if (member.userId == user.Id && member.state != "Ready")
                                {
                                    foreach (var dates in meet.datesList.ToList())
                                    {
                                        if (dates.memberId == user.Id)
                                        {
                                            meet.datesList.Remove(dates);
                                        }
                                    }
                                    foreach (var dates in setupMeetViewModel.DatesList)
                                    {
                                        dates.memberId = user.Id;
                                        meet.datesList.Add(dates);
                                    }
                                    foreach (var memberVM in setupMeetViewModel.MembersList)
                                    {
                                        if (memberVM.login == user.login)
                                        {
                                            memberVM.state = "Ready";
                                        }
                                    }
                                    setupMeetViewModel.memberState = "Ready";
                                    member.state = "Ready";
                                    _meetService.SaveAll();
                                    ViewBag.Message = string.Format("You select your dates successfully!");
                                }
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("Error!");
                }

                return View(setupMeetViewModel);
            }
            else if (decision == "add date")
            {
                Dates dates = new Dates()
                {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                setupMeetViewModel.DatesList.Add(dates);

                return View(setupMeetViewModel);
            }
            else if (decision == "remove date")
            {
                setupMeetViewModel.DatesList.RemoveAt(dateId);

                return View(setupMeetViewModel);
            }
            else
            {
                return RedirectToAction("MainMenu", "Meet");
            }
        }
    }

    public class MainMenuViewModel
    {
        public string userLogin { get; set; }
        public IList<Meet> ownedMeets { get; set; }
        public IList<Meet> memberMeets { get; set; }
    }
    public class CreateMeetViewModel
    {
        public string title { get; set; }
        public string userLogin { get; set; }
        public IList<Dates> DatesList { get; set; }
    }
    public class SetupMeetViewModel
    {
        public Guid meetId { get; set; }
        public string title { get; set; }
        public string userLogin { get; set; }
        public string state { get; set; }
        public string memberState { get; set; }
        public IList<Dates> DatesList { get; set; }
        public IList<MemberViewModel> MembersList { get; set; }
    }
    public class MemberViewModel
    {
        public string login { get; set; }
        public string role { get; set; }
        public string state { get; set; }
    }
}
