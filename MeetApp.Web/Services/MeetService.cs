using MeetApp.DAL.Models;
using MeetApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetApp.Web.Services
{
    public class MeetService
    {
        private readonly DbRepository _DbRepository;

        public MeetService(DbRepository DbRepository)
        {
            _DbRepository = DbRepository;
        }
        public List<User> GetAllUsers()
        {
            return _DbRepository.GetAllUsers().ToList();
        }
        public User GetUserByLogin(string login)
        {
            return _DbRepository.Get<User>().FirstOrDefault(x => x.UserName == login);
        }
        public User GetUserById(Guid Id)
        {
            return _DbRepository.Get<User>().FirstOrDefault(x => x.Id == Id);
        }
        public Member GetMemberByUserIdAndMeetId(Guid userId, Guid meetId)
        {
            return _DbRepository.Get<Member>().FirstOrDefault(x => x.userId == userId && x.meetId == meetId);
        }

        public List<Meet> GetAllOwnedMeetsForUser(User user)
        {
            List<Meet> meets = _DbRepository.GetAllMeets().ToList();
            List<Meet> ownedMeets = new List<Meet>();
            foreach (var meet in meets)
            {
                for (int i = 0; i < meet.membersList.Count; i++)
                {
                    if (meet.membersList[i].userId == user.Id && meet.membersList[i].role == "owner")
                    {
                        ownedMeets.Add(meet);
                    }
                    
                }
            }
            return ownedMeets;
        }
        public List<Meet> GetAllMemberMeetsForUser(User user)
        {
            List<Meet> meets = _DbRepository.GetAllMeets().ToList();
            List<Meet> memberMeets = new List<Meet>();
            foreach (var meet in meets)
            {
                for (int i = 0; i < meet.membersList.Count; i++)
                {
                    if (meet.membersList[i].userId == user.Id && meet.membersList[i].role == "member")
                    {
                        memberMeets.Add(meet);
                    }

                }
            }
            return memberMeets;
        }
       
        public void CreateNewMeet(Meet meet)
        {
            _DbRepository.Add(meet);
        }
        
        public string JoinMeet(User user, Guid meetId)
        {
            List<Meet> meets = _DbRepository.GetAllMeets().ToList();
            foreach (var meet in meets)
            {
                if (meet.Id == meetId)
                {
                    for (int i = 0; i < meet.membersList.Count; i++)
                    {
                        if (meet.membersList[i].userId == user.Id)
                        {
                            return "user already exist";
                        }
                    }
                    Member member = new Member();
                    member.userId = user.Id;
                    member.meetId = meet.Id;
                    member.role = "member";
                    member.state = "Not ready";
                    _DbRepository.Add(member);
                    return "joined";
                }
            }
            return "no found";
        }

        public void SaveAll()
        {
            _DbRepository.SaveAll();
        }
        public void DeleteMember(Member member)
        {
            _DbRepository.DeleteMember(member);
        }
        public void DeleteMeet(Meet meet)
        {
            _DbRepository.DeleteMeet(meet);
        }
    }
}
