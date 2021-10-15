using MeetApp.DAL.Models;
using MeetApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public string Get(string id)
        {
            return id;
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
                modelVM.userLogin = user.UserName;
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
                modelVM.userLogin = user.UserName;
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
                    dates.userId = user.Id;
                }
                newMeet.membersList = new List<Member>() { new Member() {
                    userId = user.Id,
                    meetId = newMeet.Id,
                    role = "owner",
                    state = "Ready",
                    datesList = createEditDatesViewModel.DatesList
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
                SetupMeetViewModel setupMeetViewModel = new SetupMeetViewModel();
                List<MemberViewModel> memberVMList = new List<MemberViewModel>();

                foreach (var memberItem in meets[id].membersList)
                {
                    MemberViewModel memberVM = new MemberViewModel();
                    memberVM.login = _meetService.GetUserById(memberItem.userId).UserName;
                    memberVM.role = memberItem.role;
                    memberVM.state = memberItem.state;
                    memberVMList.Add(memberVM);
                    if (memberItem.userId == user.Id)
                    {
                        setupMeetViewModel.memberState = memberItem.state;
                    }
                }
                for (int i = 0; i < memberVMList.Count; i++)
                {
                    if (memberVMList[i].role == "owner" && i != 0)
                    {
                        MemberViewModel temp = memberVMList[0];
                        memberVMList[0] = memberVMList[i];
                        memberVMList[i] = temp;
                    }
                }

                setupMeetViewModel.meetId = meets[id].Id;
                setupMeetViewModel.title = meets[id].title;
                setupMeetViewModel.userLogin = user.UserName;
                setupMeetViewModel.state = meets[id].state;

                List<DatesViewModel> datesVMList = new List<DatesViewModel>();
                foreach (var dates in meets[id].datesList)
                {
                    DatesViewModel datesVM = new DatesViewModel();
                    datesVM.Id = dates.Id;
                    datesVM.meetId = dates.meetId;
                    datesVM.memberId = dates.userId;
                    datesVM.dateStart = dates.dateStart;
                    datesVM.dateEnd = dates.dateEnd;
                    datesVM.memberLogin = _meetService.GetUserById(dates.userId).UserName;
                    datesVMList.Add(datesVM);
                }
                setupMeetViewModel.DatesList = datesVMList;
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
                                if (dates.userId == user.Id)
                                {
                                    meet.datesList.Remove(dates);
                                }
                            }
                            foreach (var datesItem in setupMeetViewModel.DatesList)
                            {
                                Dates dates = new Dates();
                                dates.Id = datesItem.Id;
                                dates.meetId = datesItem.meetId;
                                dates.userId = user.Id;
                                dates.dateStart = datesItem.dateStart;
                                dates.dateEnd = datesItem.dateEnd;
                                meet.datesList.Add(dates);
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
                DatesViewModel datesVM = new DatesViewModel()
                {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                setupMeetViewModel.DatesList.Add(datesVM);

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
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid)
                        {
                            Member ownerMember = _meetService.GetMemberByUserIdAndMeetId(user.Id, meet.Id);
                            List<Dates> crossDates = ownerMember.datesList.ToList();

                            foreach (var member in meet.membersList.ToList())
                            {
                                if (member.userId != user.Id && member.datesList != null)
                                {
                                    List<Dates> memberDates = member.datesList.ToList();
                                    //if (CrossDates(crossDates, memberDates) != null)
                                    //{
                                        crossDates = CrossDates(crossDates, memberDates);
                                    //}
                                }
                            }
                            if (crossDates != null)
                            {
                                Dates finalDate = new Dates();
                                finalDate.meetId = meet.Id;
                                finalDate.userId = user.Id;
                                finalDate.dateStart = crossDates[0].dateStart;
                                finalDate.dateEnd = crossDates[0].dateEnd;

                                ownerMember.datesList.Clear();
                                ownerMember.datesList.Add(finalDate);
                                meet.datesList.Clear();
                                meet.datesList.Add(finalDate);
                                meet.state = "launched";
                            }
                            else
                            {
                                meet.state = "not match";
                            }

                            setupMeetViewModel.state = meet.state;

                            _meetService.SaveAll();
                            ViewBag.Message = string.Format("Meet successfully calculated!");
                        }
                    }
                }
                else
                {
                    ViewBag.Message = string.Format("Error!");
                }

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

        public List<Dates> CrossDates(List<Dates> d1, List<Dates> d2)
        {
            List<Dates> crossDates = new List<Dates>();

            for (int i = 0; i < d1.Count; i++)
            {
                for (int j = 0; j < d2.Count; j++)
                {
                    Dates crossDate = new Dates();

                    var crossDateStart = d1[i].dateStart < d2[j].dateStart ? d2[j].dateStart : d1[i].dateStart;
                    var crossDateEnd = d1[i].dateEnd < d2[j].dateEnd ? d1[i].dateEnd : d2[j].dateEnd;
                    if (crossDateStart < crossDateEnd)
                    {
                        crossDate.meetId = d1[i].meetId;
                        crossDate.dateStart = crossDateStart;
                        crossDate.dateEnd = crossDateEnd;
                        crossDates.Add(crossDate);
                        return crossDates;
                    }
                }
            }
            return null;
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
                foreach (var memberItem in meets[id].membersList)
                {
                    MemberViewModel memberVM = new MemberViewModel();
                    memberVM.login = _meetService.GetUserById(memberItem.userId).UserName;
                    memberVM.role = memberItem.role;
                    memberVM.state = memberItem.state;
                    memberVMList.Add(memberVM);

                    if (memberItem.userId == user.Id)
                    {
                        setupMeetViewModel.memberState = memberItem.state;
                    }
                }

                for (int i = 0; i < memberVMList.Count; i++)
                {
                    if (memberVMList[i].role == "owner" && i != 0)
                    {
                        MemberViewModel temp = memberVMList[0];
                        memberVMList[0] = memberVMList[i];
                        memberVMList[i] = temp;
                    }
                }

                setupMeetViewModel.meetId = meets[id].Id;
                setupMeetViewModel.title = meets[id].title;
                setupMeetViewModel.userLogin = user.UserName;
                setupMeetViewModel.state = meets[id].state;

                List<DatesViewModel> datesVMList = new List<DatesViewModel>();
                foreach (var dates in meets[id].datesList)
                {
                    DatesViewModel datesVM = new DatesViewModel();
                    datesVM.Id = dates.Id;
                    datesVM.meetId = dates.meetId;
                    datesVM.memberId = dates.userId;
                    datesVM.dateStart = dates.dateStart;
                    datesVM.dateEnd = dates.dateEnd;
                    datesVM.memberLogin = _meetService.GetUserById(dates.userId).UserName;
                    datesVMList.Add(datesVM);
                }

                setupMeetViewModel.DatesList = datesVMList;
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
                                    member.datesList = new List<Dates>();
                                    foreach (var dates in meet.datesList.ToList())
                                    {
                                        if (dates.userId == user.Id)
                                        {
                                            meet.datesList.Remove(dates);
                                            member.datesList.Remove(dates);
                                        }
                                    }
                                    foreach (var datesItem in setupMeetViewModel.DatesList)
                                    {
                                        Dates dates = new Dates();
                                        dates.Id = datesItem.Id;
                                        dates.meetId = datesItem.meetId;
                                        dates.userId = user.Id;
                                        dates.dateStart = datesItem.dateStart;
                                        dates.dateEnd = datesItem.dateEnd;
                                        meet.datesList.Add(dates);
                                        member.datesList.Add(dates);
                                    }
                                    foreach (var memberVM in setupMeetViewModel.MembersList)
                                    {
                                        if (memberVM.login == user.UserName)
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
                DatesViewModel datesVM = new DatesViewModel()
                {
                    dateStart = DateTime.Now,
                    dateEnd = DateTime.Now.AddHours(1),
                };
                setupMeetViewModel.DatesList.Add(datesVM);

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
        public IList<DatesViewModel> DatesList { get; set; }
        public IList<MemberViewModel> MembersList { get; set; }
    }
    public class MemberViewModel
    {
        public string login { get; set; }
        public string role { get; set; }
        public string state { get; set; }
    }
    public class DatesViewModel
    {
        public int Id { get; set; }
        public Guid meetId { get; set; }
        public Guid memberId { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public string memberLogin { get; set; }
    }
}
