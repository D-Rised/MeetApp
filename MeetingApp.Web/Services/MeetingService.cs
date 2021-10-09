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

        public User GetUserByLogin(string login)
        {
            return _DbRepository.Get<User>().FirstOrDefault(x => x.login == login);
        }
        public List<Meeting> GetAllMeetingsForUser(User user)
        {
            List<Meeting> meetings = _DbRepository.GetAllMeetingsForUser(user).ToList();
            return meetings;
        }
        public List<Dates> GetAllDatesForMeeting(Meeting meeting)
        {
            List<Dates> dates = _DbRepository.GetAllDatesForMeeting(meeting).ToList();
            return dates;
        }
        public void CreateNewUser(User user)
        {
            _DbRepository.Add(user);
        }
        public void CreateNewMeeting(Meeting meeting)
        {
            _DbRepository.Add(meeting);
        }
    }
}
