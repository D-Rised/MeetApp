using MeetApp.DAL.Models;
using MeetApp.Web.Models;
using MeetApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MeetApp.Web.Controllers
{
    [Authorize]
    public class MeetController : Controller
    {
        private readonly UserService _userService;
        private readonly MeetService _meetService;

        private Guid _userGuid => new Guid(User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value);

        public MeetController(UserService userService, MeetService meetService)
        {
            _userService = userService;
            _meetService = meetService;
        }

        [HttpGet]
        public IActionResult Index(MeetListViewModel meetListViewModel)
        {
            if (User.Identity is not { IsAuthenticated: true })
                return RedirectToAction("SignIn", "Auth");

            var ownedMeets = _meetService.GetAllOwnedMeetsForUser(_userGuid);
            var memberMeets = _meetService.GetAllMemberMeetsForUser(_userGuid);

            meetListViewModel = new MeetListViewModel
            {
                OwnedMeets = ownedMeets,
                MemberMeets = memberMeets,
                AlertMessage = meetListViewModel.AlertMessage
            };

            return View(meetListViewModel);
        }

        [HttpPost]
        public IActionResult JoinMeet(Guid meetId)
        {
            var user = _userService.GetUserById(_userGuid);

            if (user == null)
                return RedirectToAction("Index", "Meet");

            if (User.Identity is not {IsAuthenticated: true})
                return RedirectToAction("SignIn", "Auth");

            var result = _meetService.JoinMeet(user, meetId);

            if (result.Code == ResultCode.OK)
                return RedirectToAction("OpenMeet", "Meet", new { guid = meetId });

            var mainMenuViewModel = new MeetListViewModel();
            mainMenuViewModel.AlertMessage = result.Text;

            return RedirectToAction("Index", "Meet", mainMenuViewModel);
        }
        
        [HttpGet]
        public IActionResult NewMeet(string serializedCreateMeetViewModel)
        {
            var user = _userService.GetUserById(_userGuid);
            
            if (user == null)
                return RedirectToAction("Index", "Meet");

            var createMeetViewModel = new CreateMeetViewModel();

            if (serializedCreateMeetViewModel == null)
            {
                createMeetViewModel = new CreateMeetViewModel
                {
                    DatesList = new List<Dates>(),
                    Title = "Dune the board game",
                    UserName = user.UserName,
                    FixedDate = false,
                    AlertMessage = createMeetViewModel.AlertMessage
                };

                var dates = new Dates()
                {
                    DateStart = DateTime.Now.AddDays(1),
                    DateEnd = DateTime.Now.AddDays(1).AddHours(2)
                };

                createMeetViewModel.DatesList.Add(dates);

                return View(createMeetViewModel);
            }
            else
            {
                try
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    createMeetViewModel = JsonConvert.DeserializeObject<CreateMeetViewModel>(serializedCreateMeetViewModel, jsonSettings);

                    return View(createMeetViewModel);
                }
                catch (JsonSerializationException ex)
                {
                    return RedirectToAction("Index", "Meet");
                }
            }
        }

        [HttpPost]
        public IActionResult AddDateToNewMeet(CreateMeetViewModel createMeetViewModel)
        {
            var dates = new Dates()
            {
                DateStart = DateTime.Now.AddDays(1),
                DateEnd = DateTime.Now.AddDays(1).AddHours(2)
            };
            createMeetViewModel.DatesList.Add(dates);

            var serializedCreateMeetViewModel = JsonConvert.SerializeObject(createMeetViewModel);

            return RedirectToAction("NewMeet", "Meet", new { serializedCreateMeetViewModel });
        }

        [HttpPost]
        public IActionResult DeleteDateFromNewMeet(CreateMeetViewModel createMeetViewModel)
        {
            if (createMeetViewModel.DateIndexToDelete > 0)
            {
                createMeetViewModel.DatesList.RemoveAt(createMeetViewModel.DateIndexToDelete);
            }

            var serializedCreateMeetViewModel = JsonConvert.SerializeObject(createMeetViewModel);

            return RedirectToAction("NewMeet", "Meet", new { serializedCreateMeetViewModel });
        }

        [HttpPost]
        public IActionResult CreateMeet(CreateMeetViewModel createMeetViewModel)
        {
            var user = _userService.GetUserById(_userGuid);
            var newMeetViewModel = new MeetListViewModel();
            
            if (user == null || createMeetViewModel.DatesList == null)
                return RedirectToAction("Index", "Meet");
            
            if (createMeetViewModel.FixedDate && createMeetViewModel.DatesList.Count > 1)
            {
                newMeetViewModel.AlertMessage = "Select only one date with fixed date parameter!";
                return RedirectToAction("NewMeet", "Meet", newMeetViewModel);
            }

            foreach (var dates in createMeetViewModel.DatesList)
            {
                if (dates.DateStart >= dates.DateEnd || dates.DateStart < DateTime.Now)
                {
                    newMeetViewModel.AlertMessage = "Incorrect date!";
                    return RedirectToAction("NewMeet", "Meet", newMeetViewModel);
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

            return RedirectToAction("SetupMeet", "Meet", new { guid = newMeet.Id });
        }

        [HttpGet]
        public IActionResult SetupMeet(Guid guid)
        {
            var user = _userService.GetUserById(_userGuid);
            Meet meet = _meetService.GetMeetById(guid);
            
            if (meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");
            
            var setupMeetViewModel = new SetupMeetViewModel();
            List<MemberViewModel> memberViewModels = new List<MemberViewModel>();

            foreach (var memberItem in meet.MembersList)
            {
                var memberViewModel = new MemberViewModel
                {
                    Id = memberItem.UserId,
                    Login = _userService.GetUserById(memberItem.UserId).UserName,
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

            setupMeetViewModel.MeetId = meet.Id;
            setupMeetViewModel.Title = meet.Title;
            setupMeetViewModel.UserName = user.UserName;
            setupMeetViewModel.State = meet.State;
            setupMeetViewModel.FixedDate = meet.FixedDate;

            List<DatesViewModel> datesViewModels = new List<DatesViewModel>();
            foreach (var dates in meet.DatesList)
            {
                var datesViewModel = new DatesViewModel
                {
                    Id = dates.Id,
                    MeetId = dates.MeetId,
                    MemberId = dates.UserId,
                    DateStart = dates.DateStart,
                    DateEnd = dates.DateEnd,
                    MemberLogin = _userService.GetUserById(dates.UserId).UserName
                };
                datesViewModels.Add(datesViewModel);
            }
            setupMeetViewModel.DatesList = datesViewModels;
            setupMeetViewModel.MembersList = memberViewModels;

            return View(setupMeetViewModel);
        }

        [HttpPost]
        public IActionResult UpdateMeet(SetupMeetViewModel setupMeetViewModel)
        {
            Meet meet = _meetService.GetMeetById(setupMeetViewModel.MeetId);
            
            if (meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");
            
            if (meet.State == "waiting")
            {
                meet.Title = setupMeetViewModel.Title;
                _meetService.SaveAll();
            }

            return RedirectToAction("SetupMeet", "Meet", new { guid = setupMeetViewModel.MeetId });
        }
        
        [HttpPost]
        public IActionResult CalculateMeet(SetupMeetViewModel setupMeetViewModel)
        {
            var user = _userService.GetUserById(_userGuid);
            Meet meet = _meetService.GetMeetById(setupMeetViewModel.MeetId);
            
            if (user == null || meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");
            
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

            return RedirectToAction("SetupMeet", "Meet", new { guid = setupMeetViewModel.MeetId });
        }
        
        [HttpPost]
        public IActionResult LaunchMeet(SetupMeetViewModel setupMeetViewModel)
        {
            var user = _userService.GetUserById(_userGuid);
            Meet meet = _meetService.GetMeetById(setupMeetViewModel.MeetId);
            
            if (user == null || meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");
            
            if (meet.FixedDate)
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

            return RedirectToAction("SetupMeet", "Meet", new { guid = setupMeetViewModel.MeetId });
        }

        [HttpPost]
        public IActionResult DeleteMeet(SetupMeetViewModel setupMeetViewModel)
        {
            Meet meet = _meetService.GetMeetById(setupMeetViewModel.MeetId);

            if (meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");

            _meetService.DeleteMeet(meet);

            return RedirectToAction("Index", "Meet");
        }

        [HttpPost]
        public IActionResult KickMemberFromMeet(SetupMeetViewModel setupMeetViewModel, Guid memberId)
        {
            Member memberToDelete = _meetService.GetMemberByUserIdAndMeetId(memberId, setupMeetViewModel.MeetId);
            
            if (memberToDelete == null)
                return RedirectToAction("Index", "Meet");
            
            if (memberToDelete.IsOwner == false)
            {
                _meetService.DeleteMember(memberToDelete);
                _meetService.SaveAll();
            }

            return RedirectToAction("SetupMeet", "Meet", new { guid = setupMeetViewModel.MeetId });
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
        public IActionResult OpenMeet(Guid guid)
        {
            var user = _userService.GetUserById(_userGuid);
            Meet meet = _meetService.GetMeetById(guid);

            List<MemberViewModel> memberViewModels = new List<MemberViewModel>();

            if (meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");
            
            var setupMeetViewModel = new SetupMeetViewModel
            {
                MeetId = meet.Id
            };
            
            foreach (var memberItem in meet.MembersList)
            {
                var memberViewModel = new MemberViewModel
                {
                    Login = _userService.GetUserById(memberItem.UserId).UserName,
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

            setupMeetViewModel.MeetId = meet.Id;
            setupMeetViewModel.Title = meet.Title;
            setupMeetViewModel.UserName = user.UserName;
            setupMeetViewModel.State = meet.State;

            List<DatesViewModel> datesViewModels = new List<DatesViewModel>();
            foreach (var dates in meet.DatesList)
            {
                var datesViewModel = new DatesViewModel
                {
                    Id = dates.Id,
                    MeetId = dates.MeetId,
                    MemberId = dates.UserId,
                    DateStart = dates.DateStart,
                    DateEnd = dates.DateEnd,
                    MemberLogin = _userService.GetUserById(dates.UserId).UserName
                };
                datesViewModels.Add(datesViewModel);
            }

            setupMeetViewModel.DatesList = datesViewModels;
            setupMeetViewModel.MembersList = memberViewModels;

            return View(setupMeetViewModel);
        }

        [HttpPost]
        public IActionResult AddDateToOpenedMeet(SetupMeetViewModel setupMeetViewModel)
        {
            var datesViewModel = new DatesViewModel()
            {
                DateStart = DateTime.Now.AddDays(1),
                DateEnd = DateTime.Now.AddDays(1).AddHours(2),
            };
            setupMeetViewModel.DatesList.Add(datesViewModel);

            var serializedSetupMeetViewModel = JsonConvert.SerializeObject(setupMeetViewModel);

            return RedirectToAction("OpenedMeet", "Meet", new { serializedSetupMeetViewModel });
        }

        [HttpPost]
        public IActionResult DeleteDateFromOpenedMeet(SetupMeetViewModel setupMeetViewModel)
        {
            if (setupMeetViewModel.DateIndexToDelete > 0)
            {
                setupMeetViewModel.DatesList.RemoveAt(setupMeetViewModel.DateIndexToDelete);
            }

            var serializedSetupMeetViewModel = JsonConvert.SerializeObject(setupMeetViewModel);

            return RedirectToAction("OpenedMeet", "Meet", new { serializedSetupMeetViewModel });
        }

        [HttpPost]
        public IActionResult ConfirmDates(SetupMeetViewModel setupMeetViewModel)
        {
            var user = _userService.GetUserById(_userGuid);
            Meet meet = _meetService.GetMeetById(setupMeetViewModel.MeetId);
            
            if (user == null || meet == null || meet.DatesList == null || meet.MembersList == null)
                return RedirectToAction("Index", "Meet");
            
            foreach (var dates in setupMeetViewModel.DatesList)
            {
                if (dates.DateStart >= dates.DateEnd || dates.DateStart < DateTime.Now)
                {
                    setupMeetViewModel.AlertMessage = "Incorrect date!";
                    return RedirectToAction("OpenMeet", "Meet", new { guid = setupMeetViewModel.MeetId });
                }
            }
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
                }
            }

            return RedirectToAction("OpenMeet", "Meet", new { guid = setupMeetViewModel.MeetId });
        }
    }
}
