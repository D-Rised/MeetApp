using MeetApp.DAL.Models;
using MeetApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MeetApp.Web.Models;

namespace MeetApp.Web.Services
{
    public class MeetService
    {
        private readonly MeetRepository _meetRepository;

        public MeetService(MeetRepository meetRepository)
        {
            _meetRepository = meetRepository;
        }

        public Member GetMemberByUserIdAndMeetId(Guid userId, Guid meetId)
        {
            return _meetRepository.Get<Member>().FirstOrDefault(x => x.UserId == userId && x.MeetId == meetId);
        }

        public List<Meet> GetAllOwnedMeetsForUser(Guid guid)
        {
            User user = _meetRepository.Get<User>().FirstOrDefault(x => x.Id == guid);

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

        public List<Meet> GetAllMemberMeetsForUser(Guid guid)
        {
            User user = _meetRepository.Get<User>().FirstOrDefault(x => x.Id == guid);

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
        
        public StatusResult JoinMeet(User user, Guid meetId)
        {
            Meet meet = GetMeetById(meetId);

            if (user == null)
                return new StatusResult("Cannot be null " + nameof(user), ResultCode.Null);

            if (meet == null)
                return new StatusResult("Cannot be null " + nameof(meet), ResultCode.Null);

            if (meet.State == "launched")
                return new StatusResult("Meet was already launched", ResultCode.Launched);

            for (int i = 0; i < meet.MembersList.Count; i++)
            {
                if (meet.MembersList[i].UserId == user.Id)
                    return new StatusResult("You already joined to this meet", ResultCode.AlreadyExist);
            }
            
            var member = new Member
            {
                UserId = user.Id,
                MeetId = meet.Id,
                IsOwner = false,
                Ready = false
            };
            _meetRepository.Add(member);

            return new StatusResult("Successfully joined to the meet", ResultCode.OK);
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
