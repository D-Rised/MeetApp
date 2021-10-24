using MeetApp.DAL.Models;
using MeetApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.Web.Services
{
    public class MeetService
    {
        private readonly MeetRepository _meetRepository;

        public MeetService(MeetRepository meetRepository)
        {
            _meetRepository = meetRepository;
        }
        public List<User> GetAllUsers()
        {
            return _meetRepository.GetAllUsers().ToList();
        }
        public User GetUserByLogin(string login)
        {
            return _meetRepository.Get<User>().FirstOrDefault(x => x.UserName == login);
        }
        public User GetUserById(Guid id)
        {
            return _meetRepository.Get<User>().FirstOrDefault(x => x.Id == id);
        }
        public Member GetMemberByUserIdAndMeetId(Guid userId, Guid meetId)
        {
            return _meetRepository.Get<Member>().FirstOrDefault(x => x.UserId == userId && x.MeetId == meetId);
        }

        public List<Meet> GetAllOwnedMeetsForUser(User user)
        {
            if (user != null)
            {
                List<Meet> meets = _meetRepository.GetAllMeets().ToList();
                
                List<Meet> meetsTrue = _meetRepository.GetAllMeets().ToList();
                
                List<Meet> ownedMeets = new List<Meet>();
                
                foreach (var meet in meets)
                {
                    for (int i = 0; i < meet.MembersList.Count; i++)
                    {
                        if (meet.MembersList[i].UserId == user.Id && meet.MembersList[i].IsOwner)
                        {
                            ownedMeets.Add(meet);
                        }

                    }
                }
                return ownedMeets;
            }
            else
            {
                return null;
            }
        }
        public List<Meet> GetAllMemberMeetsForUser(User user)
        {
            if (user != null)
            {
                List<Meet> meets = _meetRepository.GetAllMeets().ToList();
                List<Meet> memberMeets = new List<Meet>();
                foreach (var meet in meets)
                {
                    for (int i = 0; i < meet.MembersList.Count; i++)
                    {
                        if (meet.MembersList[i].UserId == user.Id && meet.MembersList[i].IsOwner == false)
                        {
                            memberMeets.Add(meet);
                        }

                    }
                }
                return memberMeets;
            }
            else
            {
                return null;
            }
        }

        public Meet GetMeetById(Guid id)
        {
            return _meetRepository.Get<Meet>().Include(x => x.MembersList).Include(x => x.DatesList).FirstOrDefault(x => x.Id == id);
        }
        
        public void CreateNewMeet(Meet meet)
        {
            if (meet != null)
            {
                _meetRepository.Add(meet);
            }
        }
        
        public string JoinMeet(User user, Guid meetId)
        {
            Meet meet = GetMeetById(meetId);
            
            if (user == null || meet == null)
            {
                return "No meet found for this invite key!";
            }

            if (meet.State == "launched")
            {
                return "Meet was already launched!";
            }

            for (int i = 0; i < meet.MembersList.Count; i++)
            {
                if (meet.MembersList[i].UserId == user.Id)
                {
                    return "You already joined to this meet!";
                }
            }
            
            var member = new Member
            {
                UserId = user.Id,
                MeetId = meet.Id,
                IsOwner = false,
                State = "Not ready"
            };
            _meetRepository.Add(member);
            
            return "";
        }

        public void SaveAll()
        {
            _meetRepository.SaveAll();
        }
        public void DeleteMember(Member member)
        {
            if (member != null)
            {
                _meetRepository.DeleteMember(member);
            }
        }
        public void DeleteMeet(Meet meet)
        {
            if (meet != null)
            {
                _meetRepository.DeleteMeet(meet);
            }
        }
    }
}
