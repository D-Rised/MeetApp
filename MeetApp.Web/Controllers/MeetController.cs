using MeetApp.DAL.Models;
using MeetApp.Web.Models;
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

        [HttpGet]
        public IActionResult MainMenu()
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            if (User.Identity is {IsAuthenticated: true})
            {
                var mainMenuViewModel = new MainMenuViewModel
                {
                    OwnedMeets = _meetService.GetAllOwnedMeetsForUser(user),
                    MemberMeets = _meetService.GetAllMemberMeetsForUser(user),
                    UserLogin = user.UserName
                };
                return View(mainMenuViewModel);
            }

            return RedirectToAction("SignIn", "Auth");
        }

        [HttpPost]
        public IActionResult MainMenu(MainMenuViewModel mainMenuViewModel, ActionType actionType, string meetId)
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);

            if (User.Identity is not {IsAuthenticated: true}) return RedirectToAction("SignIn", "Auth");

            if (actionType == ActionType.Join)
            {
                if (Guid.TryParse(meetId, out Guid guidMeetId))
                {
                    if (_meetService.JoinMeet(user, guidMeetId) == "joined")
                    {
                        mainMenuViewModel.AlertMessage = "You joined to meet!";
                    }
                    else if (_meetService.JoinMeet(user, guidMeetId) == "user already exist")
                    {
                        mainMenuViewModel.AlertMessage = "You already joined to this meet!";
                    }
                    else
                    {
                        mainMenuViewModel.AlertMessage = "No meet found for this invite key!";
                    }

                    mainMenuViewModel.OwnedMeets = _meetService.GetAllOwnedMeetsForUser(user);
                    mainMenuViewModel.MemberMeets = _meetService.GetAllMemberMeetsForUser(user);
                    mainMenuViewModel.UserLogin = user.UserName;
                }
                else
                {
                    mainMenuViewModel.AlertMessage = "No meet found for this invite key!";
                }

                mainMenuViewModel.OwnedMeets = _meetService.GetAllOwnedMeetsForUser(user);
                mainMenuViewModel.MemberMeets = _meetService.GetAllMemberMeetsForUser(user);
                mainMenuViewModel.UserLogin = user.UserName;
            }

            return View(mainMenuViewModel);

        }

        [HttpGet]
        public IActionResult CreateMeet()
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            if (login != "")
            {
                var createMeetViewModel = new CreateMeetViewModel
                {
                    DatesList = new List<Dates>(),
                    Title = "Dune the board game",
                    UserLogin = user.UserName,
                    FixedDate = false
                };
                var dates = new Dates() {
                    DateStart = DateTime.Now.AddDays(1),
                    DateEnd = DateTime.Now.AddDays(1).AddHours(2),
                };
                createMeetViewModel.DatesList.Add(dates);
                return View(createMeetViewModel);
            }

            return RedirectToAction("MainMenu", "Meet");
        }

        [HttpPost]
        public IActionResult CreateMeet(CreateMeetViewModel createMeetViewModel, ActionType actionType)
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            if (actionType == ActionType.CreateMeet)
            {
                if (createMeetViewModel.FixedDate && createMeetViewModel.DatesList.Count > 1)
                {
                    createMeetViewModel.AlertMessage = "Select only one date with fixed date parameter!";
                    return View(createMeetViewModel);
                }

                foreach (var dates in createMeetViewModel.DatesList)
                {
                    if (dates.DateStart >= dates.DateEnd || dates.DateStart < DateTime.Now)
                    {
                        createMeetViewModel.AlertMessage = "Incorrect date!";
                        return View(createMeetViewModel);
                    }
                }

                var newMeet = new Meet
                {
                    Id = Guid.NewGuid(),
                    Title = createMeetViewModel.Title,
                    DatesList = createMeetViewModel.DatesList,
                    FixedDate = createMeetViewModel.FixedDate
                };

                foreach (var dates in newMeet.DatesList.ToList())
                {
                    dates.UserId = user.Id;
                }
                newMeet.MembersList = new List<Member>() { new Member() {
                    UserId = user.Id,
                    MeetId = newMeet.Id,
                    IsOwner = true,
                    State = "Ready",
                    DatesList = createMeetViewModel.DatesList
                }};
                newMeet.State = "waiting";

                _meetService.CreateNewMeet(newMeet);

                return RedirectToAction("CreatedMeet", "Meet", new { @meetId = newMeet.Id });
            }
            
            if (actionType == ActionType.AddDate)
            {
                var dates = new Dates()
                {
                    DateStart = DateTime.Now.AddDays(1),
                    DateEnd = DateTime.Now.AddDays(1).AddHours(2),
                };
                createMeetViewModel.DatesList.Add(dates);
                return View(createMeetViewModel);
            }
            
            if (actionType == ActionType.DeleteDate && createMeetViewModel.DateIndexToDelete > 0)
            {
                createMeetViewModel.DatesList.RemoveAt(createMeetViewModel.DateIndexToDelete);
                return View(createMeetViewModel);
            }
            
            return RedirectToAction("SignIn", "Auth");
        }

        [HttpGet]
        public IActionResult CreatedMeet(Guid meetId)
        {
            return View(meetId);
        }

        [HttpGet]
        public IActionResult SetupMeet(int index)
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllOwnedMeetsForUser(user);

            if (meets.Count <= index) return RedirectToAction("MainMenu", "Meet");
            
            var setupMeetViewModel = new SetupMeetViewModel();
            List<MemberViewModel> memberViewModels = new List<MemberViewModel>();

            foreach (var memberItem in meets[index].MembersList)
            {
                var memberViewModel = new MemberViewModel
                {
                    Login = _meetService.GetUserById(memberItem.UserId).UserName,
                    IsOwner = memberItem.IsOwner,
                    State = memberItem.State
                };
                memberViewModels.Add(memberViewModel);
                    
                if (memberItem.UserId == user.Id)
                {
                    setupMeetViewModel.MemberState = memberItem.State;
                }
            }
            for (int i = 0; i < memberViewModels.Count; i++)
            {
                if (memberViewModels[i].IsOwner && i != 0)
                {
                    (memberViewModels[0], memberViewModels[i]) = (memberViewModels[i], memberViewModels[0]);
                }
            }

            setupMeetViewModel.MeetId = meets[index].Id;
            setupMeetViewModel.Index = index;
            setupMeetViewModel.Title = meets[index].Title;
            setupMeetViewModel.UserLogin = user.UserName;
            setupMeetViewModel.State = meets[index].State;
            setupMeetViewModel.FixedDate = meets[index].FixedDate;

            List<DatesViewModel> datesViewModels = new List<DatesViewModel>();
            foreach (var dates in meets[index].DatesList)
            {
                var datesViewModel = new DatesViewModel
                {
                    Id = dates.Id,
                    MeetId = dates.MeetId,
                    MemberId = dates.UserId,
                    DateStart = dates.DateStart,
                    DateEnd = dates.DateEnd,
                    MemberLogin = _meetService.GetUserById(dates.UserId).UserName
                };
                datesViewModels.Add(datesViewModel);
            }
            setupMeetViewModel.DatesList = datesViewModels;
            setupMeetViewModel.MembersList = memberViewModels;
            return View(setupMeetViewModel);
        }

        [HttpPost]
        public IActionResult SetupMeet(SetupMeetViewModel setupMeetViewModel, ActionType actionType, string meetId, int memberId)
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllOwnedMeetsForUser(user);

            if (actionType == ActionType.UpdateMeet)
            {
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid && meet.State == "waiting")
                        {
                            meet.Title = setupMeetViewModel.Title;
                            _meetService.SaveAll();
                        }
                    }
                }
                return SetupMeet(setupMeetViewModel.Index);
            }

            if (actionType == ActionType.KickMember)
            {
                if (setupMeetViewModel.MembersList[memberId].IsOwner == false)
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

            if (actionType == ActionType.CalculateMeet)
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
                                Dates finalDate = new Dates
                                {
                                    MeetId = meet.Id,
                                    UserId = user.Id,
                                    DateStart = crossDates[0].DateStart,
                                    DateEnd = crossDates[0].DateEnd
                                };

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

            if (actionType == ActionType.LaunchMeet)
            {
                if (Guid.TryParse(meetId, out Guid meetIdGuid))
                {
                    foreach (var meet in meets)
                    {
                        if (meet.Id == meetIdGuid && meet.FixedDate)
                        {
                            Member ownerMember = _meetService.GetMemberByUserIdAndMeetId(user.Id, meet.Id);

                            Dates finalDate = new Dates
                            {
                                MeetId = meet.Id,
                                UserId = user.Id,
                                DateStart = meet.DatesList[0].DateStart,
                                DateEnd = meet.DatesList[0].DateEnd
                            };

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

            if (actionType == ActionType.DeleteMeet)
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

            return RedirectToAction("MainMenu", "Meet");
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
        
        [HttpGet]
        public IActionResult OpenMeet(int index)
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllMemberMeetsForUser(user);

            if (meets.Count <= index) return RedirectToAction("MainMenu", "Meet");
            
            List<MemberViewModel> memberViewModels = new List<MemberViewModel>();
            
            var setupMeetViewModel = new SetupMeetViewModel
            {
                Index = index
            };
            
            foreach (var memberItem in meets[index].MembersList)
            {
                var memberViewModel = new MemberViewModel
                {
                    Login = _meetService.GetUserById(memberItem.UserId).UserName,
                    IsOwner = memberItem.IsOwner,
                    State = memberItem.State
                };
                memberViewModels.Add(memberViewModel);

                if (memberItem.UserId == user.Id)
                {
                    setupMeetViewModel.MemberState = memberItem.State;
                }
            }

            for (int i = 0; i < memberViewModels.Count; i++)
            {
                if (memberViewModels[i].IsOwner && i != 0)
                {
                    (memberViewModels[0], memberViewModels[i]) = (memberViewModels[i], memberViewModels[0]);
                }
            }

            setupMeetViewModel.MeetId = meets[index].Id;
            setupMeetViewModel.Title = meets[index].Title;
            setupMeetViewModel.UserLogin = user.UserName;
            setupMeetViewModel.State = meets[index].State;

            List<DatesViewModel> datesViewModels = new List<DatesViewModel>();
            foreach (var dates in meets[index].DatesList)
            {
                var datesViewModel = new DatesViewModel
                {
                    Id = dates.Id,
                    MeetId = dates.MeetId,
                    MemberId = dates.UserId,
                    DateStart = dates.DateStart,
                    DateEnd = dates.DateEnd,
                    MemberLogin = _meetService.GetUserById(dates.UserId).UserName
                };
                datesViewModels.Add(datesViewModel);
            }

            setupMeetViewModel.DatesList = datesViewModels;
            setupMeetViewModel.MembersList = memberViewModels;
            return View(setupMeetViewModel);
        }

        [HttpPost]
        public IActionResult OpenMeet(SetupMeetViewModel setupMeetViewModel, ActionType actionType, string meetId, int dateId)
        {
            var login = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
            var user = _meetService.GetUserByLogin(login);
            List<Meet> meets = _meetService.GetAllMemberMeetsForUser(user);

            if (actionType == ActionType.Confirm)
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
                                        var dates = new Dates
                                        {
                                            Id = datesItem.Id,
                                            MeetId = datesItem.MeetId,
                                            UserId = user.Id,
                                            DateStart = datesItem.DateStart,
                                            DateEnd = datesItem.DateEnd
                                        };
                                        meet.DatesList.Add(dates);
                                        member.DatesList.Add(dates);
                                    }
                                    foreach (var memberViewModel in setupMeetViewModel.MembersList)
                                    {
                                        if (memberViewModel.Login == user.UserName)
                                        {
                                            memberViewModel.State = "Ready";
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

            if (actionType == ActionType.AddDate)
            {
                var datesViewModel = new DatesViewModel()
                {
                    DateStart = DateTime.Now.AddDays(1),
                    DateEnd = DateTime.Now.AddDays(1).AddHours(2),
                };
                setupMeetViewModel.DatesList.Add(datesViewModel);

                return View(setupMeetViewModel);
            }

            if (actionType == ActionType.DeleteDate)
            {
                setupMeetViewModel.DatesList.RemoveAt(dateId);

                return View(setupMeetViewModel);
            }

            return RedirectToAction("MainMenu", "Meet");
        }
    }
}
