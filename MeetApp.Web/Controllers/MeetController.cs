using MeetApp.DAL.Models;
using MeetApp.Models;
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

        public MeetController(MeetService meetService)
        {
            _meetService = meetService;
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
                modelVM.OwnedMeets = _meetService.GetAllOwnedMeetsForUser(user);
                modelVM.MemberMeets = _meetService.GetAllMemberMeetsForUser(user);
                modelVM.UserLogin = user.UserName;
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
                MainMenuViewModel modelVM = new MainMenuViewModel();

                if (Guid.TryParse(Id, out Guid guidId))
                {

                    if (_meetService.JoinMeet(user, guidId) == "joined")
                    {
                        modelVM.AlertMessage = "You joined to meet!";
                    }
                    else if (_meetService.JoinMeet(user, guidId) == "user already exist")
                    {
                        modelVM.AlertMessage = "You already joined to this meet!";
                    }
                    else
                    {
                        modelVM.AlertMessage = "No meet found for this invite key!";
                    }

                    modelVM.OwnedMeets = _meetService.GetAllOwnedMeetsForUser(user);
                    modelVM.MemberMeets = _meetService.GetAllMemberMeetsForUser(user);
                    modelVM.UserLogin = user.UserName;
                }
                else
                {
                    modelVM.AlertMessage = "No meet found for this invite key!";
                }

                modelVM.OwnedMeets = _meetService.GetAllOwnedMeetsForUser(user);
                modelVM.MemberMeets = _meetService.GetAllMemberMeetsForUser(user);
                modelVM.UserLogin = user.UserName;
                return View(modelVM);
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
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddHours(1),
                };
                modelVM.DatesList = new List<Dates>();
                modelVM.Title = "Dune the board game";
                modelVM.UserLogin = user.UserName;
                modelVM.FixedDate = false;
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
                if (createEditDatesViewModel.FixedDate && createEditDatesViewModel.DatesList.Count > 1)
                {
                    createEditDatesViewModel.AlertMessage = "Select only one date with fixed date parameter!";
                    return View(createEditDatesViewModel);
                }

                foreach (var dates in createEditDatesViewModel.DatesList)
                {
                    if (dates.DateStart >= dates.DateEnd || dates.DateStart < DateTime.Now)
                    {
                        createEditDatesViewModel.AlertMessage = "Incorrect date!";
                        return View(createEditDatesViewModel);
                    }
                }

                Meet newMeet = new Meet();
                newMeet.Id = Guid.NewGuid();
                newMeet.Title = createEditDatesViewModel.Title;
                newMeet.DatesList = createEditDatesViewModel.DatesList;
                newMeet.FixedDate = createEditDatesViewModel.FixedDate;

                foreach (var dates in newMeet.DatesList.ToList())
                {
                    dates.UserId = user.Id;
                }
                newMeet.MembersList = new List<Member>() { new Member() {
                    UserId = user.Id,
                    MeetId = newMeet.Id,
                    Role = "owner",
                    State = "Ready",
                    DatesList = createEditDatesViewModel.DatesList
                }};
                newMeet.State = "waiting";

                _meetService.CreateNewMeet(newMeet);

                return RedirectToAction("CreatedMeet", "Meet", new { @meetId = newMeet.Id });
            }
            else if (action == "add row")
            {
                Dates dates = new Dates()
                {
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddHours(1),
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

                foreach (var memberItem in meets[id].MembersList)
                {
                    MemberViewModel memberVM = new MemberViewModel();
                    memberVM.Login = _meetService.GetUserById(memberItem.UserId).UserName;
                    memberVM.Role = memberItem.Role;
                    memberVM.State = memberItem.State;
                    memberVMList.Add(memberVM);
                    if (memberItem.UserId == user.Id)
                    {
                        setupMeetViewModel.MemberState = memberItem.State;
                    }
                }
                for (int i = 0; i < memberVMList.Count; i++)
                {
                    if (memberVMList[i].Role == "owner" && i != 0)
                    {
                        MemberViewModel temp = memberVMList[0];
                        memberVMList[0] = memberVMList[i];
                        memberVMList[i] = temp;
                    }
                }

                setupMeetViewModel.MeetId = meets[id].Id;
                setupMeetViewModel.Index = id;
                setupMeetViewModel.Title = meets[id].Title;
                setupMeetViewModel.UserLogin = user.UserName;
                setupMeetViewModel.State = meets[id].State;
                setupMeetViewModel.FixedDate = meets[id].FixedDate;

                List<DatesViewModel> datesVMList = new List<DatesViewModel>();
                foreach (var dates in meets[id].DatesList)
                {
                    DatesViewModel datesVM = new DatesViewModel();
                    datesVM.Id = dates.Id;
                    datesVM.MeetId = dates.MeetId;
                    datesVM.MemberId = dates.UserId;
                    datesVM.DateStart = dates.DateStart;
                    datesVM.DateEnd = dates.DateEnd;
                    datesVM.MemberLogin = _meetService.GetUserById(dates.UserId).UserName;
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
                        if (meet.Id == meetIdGuid && meet.State == "waiting")
                        {
                            Member ownerMember = _meetService.GetMemberByUserIdAndMeetId(user.Id, meet.Id);
                            meet.Title = setupMeetViewModel.Title;
                            foreach (var dates in meet.DatesList.ToList())
                            {
                                if (dates.UserId == user.Id)
                                {
                                    meet.DatesList.Remove(dates);
                                }
                            }
                            foreach (var datesItem in setupMeetViewModel.DatesList)
                            {
                                Dates dates = new Dates();
                                dates.Id = datesItem.Id;
                                dates.MeetId = datesItem.MeetId;
                                dates.UserId = user.Id;
                                dates.DateStart = datesItem.DateStart;
                                dates.DateEnd = datesItem.DateEnd;
                                ownerMember.DatesList.Add(dates);
                                meet.DatesList.Add(dates);
                            }
                            _meetService.SaveAll();
                        }
                    }
                }
                return SetupMeet(setupMeetViewModel.Index);
            }
            else if (decision == "add date")
            {
                DatesViewModel datesVM = new DatesViewModel()
                {
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddHours(1),
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
                if (setupMeetViewModel.MembersList[memberId].Role != "owner")
                {
                    User memberToDelete = _meetService.GetUserByLogin(setupMeetViewModel.MembersList[memberId].Login);

                    if (Guid.TryParse(meetId, out Guid meetIdGuid))
                    {
                        foreach (var meet in meets)
                        {
                            if (meet.Id == meetIdGuid)
                            {
                                foreach (var member in meet.MembersList.ToList())
                                {
                                    if (member.UserId == memberToDelete.Id)
                                    {
                                        _meetService.DeleteMember(member);
                                    }
                                }
                            }
                            _meetService.SaveAll();
                        }
                    }
                }
                return SetupMeet(setupMeetViewModel.Index);
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
                            List<Dates> crossDates = ownerMember.DatesList.ToList();

                            foreach (var member in meet.MembersList.ToList())
                            {
                                if (member.UserId != user.Id && member.DatesList != null)
                                {
                                    List<Dates> memberDates = member.DatesList.ToList();
                                    crossDates = CrossDates(crossDates, memberDates);
                                }
                            }
                            if (crossDates != null)
                            {
                                Dates finalDate = new Dates();
                                finalDate.MeetId = meet.Id;
                                finalDate.UserId = user.Id;
                                finalDate.DateStart = crossDates[0].DateStart;
                                finalDate.DateEnd = crossDates[0].DateEnd;

                                ownerMember.DatesList.Clear();
                                ownerMember.DatesList.Add(finalDate);
                                meet.DatesList.Clear();
                                meet.DatesList.Add(finalDate);
                                meet.State = "launched";
                            }
                            else
                            {
                                meet.State = "not match";
                            }
                            _meetService.SaveAll();
                        }
                    }
                }

                return SetupMeet(setupMeetViewModel.Index);
            }
            else if (decision == "launch meet")
            {
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid && meet.FixedDate)
                        {
                            Member ownerMember = _meetService.GetMemberByUserIdAndMeetId(user.Id, meet.Id);

                            Dates finalDate = new Dates();
                            finalDate.MeetId = meet.Id;
                            finalDate.UserId = user.Id;
                            finalDate.DateStart = meet.DatesList[0].DateStart;
                            finalDate.DateEnd = meet.DatesList[0].DateEnd;

                            ownerMember.DatesList.Clear();
                            ownerMember.DatesList.Add(finalDate);
                            meet.DatesList.Clear();
                            meet.DatesList.Add(finalDate);
                            meet.State = "launched";

                            _meetService.SaveAll();
                        }
                    }
                }

                return SetupMeet(setupMeetViewModel.Index);
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
                        }
                    }
                }

                return RedirectToAction("MainMenu", "Meet");
            }
            else
            {
                return RedirectToAction("MainMenu", "Meet");
            }
        }

        [HttpPost]
        public IActionResult KickMemberFromMeet(SetupMeetViewModel setupMeetViewModel, string meetId, int dateId, int memberId)
        {
            string login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            User user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllOwnedMeetsForUser(user);

            if (setupMeetViewModel.MembersList[memberId].Role != "owner")
            {
                User memberToDelete = _meetService.GetUserByLogin(setupMeetViewModel.MembersList[memberId].Login);

                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid)
                        {
                            foreach (var member in meet.MembersList.ToList())
                            {
                                if (member.UserId == memberToDelete.Id)
                                {
                                    _meetService.DeleteMember(member);
                                }
                            }
                        }
                        _meetService.SaveAll();
                    }
                }
            }
            return SetupMeet(setupMeetViewModel.Index);
        }

        public List<Dates> CrossDates(List<Dates> crossDates, List<Dates> memberDates)
        {
            List<Dates> newCrossDates = new List<Dates>();

            for (int i = 0; i < crossDates.Count; i++)
            {
                for (int j = 0; j < memberDates.Count; j++)
                {
                    Dates crossDate = new Dates();

                    var crossDateStart = crossDates[i].DateStart < memberDates[j].DateStart ? memberDates[j].DateStart : crossDates[i].DateStart;
                    var crossDateEnd = crossDates[i].DateEnd < memberDates[j].DateEnd ? crossDates[i].DateEnd : memberDates[j].DateEnd;
                    if (crossDateStart < crossDateEnd)
                    {
                        crossDate.MeetId = crossDates[i].MeetId;
                        crossDate.DateStart = crossDateStart;
                        crossDate.DateEnd = crossDateEnd;
                        newCrossDates.Add(crossDate);
                        return newCrossDates;
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
                setupMeetViewModel.Index = id;
                foreach (var memberItem in meets[id].MembersList)
                {
                    MemberViewModel memberVM = new MemberViewModel();
                    memberVM.Login = _meetService.GetUserById(memberItem.UserId).UserName;
                    memberVM.Role = memberItem.Role;
                    memberVM.State = memberItem.State;
                    memberVMList.Add(memberVM);

                    if (memberItem.UserId == user.Id)
                    {
                        setupMeetViewModel.MemberState = memberItem.State;
                    }
                }

                for (int i = 0; i < memberVMList.Count; i++)
                {
                    if (memberVMList[i].Role == "owner" && i != 0)
                    {
                        MemberViewModel temp = memberVMList[0];
                        memberVMList[0] = memberVMList[i];
                        memberVMList[i] = temp;
                    }
                }

                setupMeetViewModel.MeetId = meets[id].Id;
                setupMeetViewModel.Title = meets[id].Title;
                setupMeetViewModel.UserLogin = user.UserName;
                setupMeetViewModel.State = meets[id].State;

                List<DatesViewModel> datesVMList = new List<DatesViewModel>();
                foreach (var dates in meets[id].DatesList)
                {
                    DatesViewModel datesVM = new DatesViewModel();
                    datesVM.Id = dates.Id;
                    datesVM.MeetId = dates.MeetId;
                    datesVM.MemberId = dates.UserId;
                    datesVM.DateStart = dates.DateStart;
                    datesVM.DateEnd = dates.DateEnd;
                    datesVM.MemberLogin = _meetService.GetUserById(dates.UserId).UserName;
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
                    foreach (var dates in setupMeetViewModel.DatesList)
                    {
                        if (dates.DateStart >= dates.DateEnd || dates.DateStart < DateTime.Now)
                        {
                            setupMeetViewModel.AlertMessage = "Incorrect date!";
                            return View(setupMeetViewModel);
                        }
                    }
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid)
                        {
                            foreach (var member in meet.MembersList.ToList())
                            {
                                if (member.UserId == user.Id && member.State != "Ready")
                                {
                                    member.DatesList = new List<Dates>();
                                    foreach (var dates in meet.DatesList.ToList())
                                    {
                                        if (dates.UserId == user.Id)
                                        {
                                            meet.DatesList.Remove(dates);
                                            member.DatesList.Remove(dates);
                                        }
                                    }
                                    foreach (var datesItem in setupMeetViewModel.DatesList)
                                    {
                                        Dates dates = new Dates();
                                        dates.Id = datesItem.Id;
                                        dates.MeetId = datesItem.MeetId;
                                        dates.UserId = user.Id;
                                        dates.DateStart = datesItem.DateStart;
                                        dates.DateEnd = datesItem.DateEnd;
                                        meet.DatesList.Add(dates);
                                        member.DatesList.Add(dates);
                                    }
                                    foreach (var memberVM in setupMeetViewModel.MembersList)
                                    {
                                        if (memberVM.Login == user.UserName)
                                        {
                                            memberVM.State = "Ready";
                                        }
                                    }
                                    setupMeetViewModel.MemberState = "Ready";
                                    member.State = "Ready";
                                    _meetService.SaveAll();
                                    return OpenMeet(setupMeetViewModel.Index);
                                }
                            }
                        }
                    }
                }
                else
                {
                    setupMeetViewModel.AlertMessage = "Error!";
                }

                return View(setupMeetViewModel);
            }
            else if (decision == "add date")
            {
                DatesViewModel datesVM = new DatesViewModel()
                {
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddHours(1),
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
}
