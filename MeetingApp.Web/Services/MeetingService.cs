using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingApp.DAL.Models;
using MeetingApp.DAL.Repositories;
using Microsoft.AspNetCore.Identity;

namespace MeetingApp.Web.Services
{
    public class MeetingService
    {
        private readonly DbRepository _DbRepository;

        public MeetingService(DbRepository DbRepository)
        {
            _DbRepository = DbRepository;
        }
        public List<User> GetAllUsers()
        {
            return _DbRepository.GetAllUsers().ToList();
        }
        public User GetUserByLogin(string login)
        {
            return _DbRepository.Get<User>().FirstOrDefault(x => x.login == login);
        }

        public List<Meeting> GetAllOwnedMeetingsForUser(User user)
        {
            List<Meeting> meetings = _DbRepository.GetAllMeetings().ToList();
            List<Meeting> ownedMeetings = new List<Meeting>();
            foreach (var meeting in meetings)
            {
                for (int i = 0; i < meeting.membersList.Count; i++)
                {
                    if (meeting.membersList[i].userId == user.Id && meeting.membersList[i].role == "owner")
                    {
                        ownedMeetings.Add(meeting);
                    }
                    
                }
            }
            return ownedMeetings;
        }
        public List<Meeting> GetAllMemberMeetingsForUser(User user)
        {
            List<Meeting> meetings = _DbRepository.GetAllMeetings().ToList();
            List<Meeting> memberMeetings = new List<Meeting>();
            foreach (var meeting in meetings)
            {
                for (int i = 0; i < meeting.membersList.Count; i++)
                {
                    if (meeting.membersList[i].userId == user.Id && meeting.membersList[i].role == "member")
                    {
                        memberMeetings.Add(meeting);
                    }

                }
            }
            return memberMeetings;
        }
       
        public void CreateNewUser(User user)
        {
            _DbRepository.Add(user);
        }
        public void CreateNewMeeting(Meeting meeting)
        {
            _DbRepository.Add(meeting);
        }
        
        public string JoinMeeting(User user, Guid meetingId)
        {
            List<Meeting> meetings = _DbRepository.GetAllMeetings().ToList();
            foreach (var meeting in meetings)
            {
                if (meeting.Id == meetingId)
                {
                    for (int i = 0; i < meeting.membersList.Count; i++)
                    {
                        if (meeting.membersList[i].userId == user.Id)
                        {
                            return "user already exist";
                        }
                    }
                    Member member = new Member();
                    member.userId = user.Id;
                    member.meetingId = meeting.Id;
                    member.role = "member";
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
        public void DeleteMeeting(Meeting meeting)
        {
            _DbRepository.DeleteMeeting(meeting);
        }
    }
}
