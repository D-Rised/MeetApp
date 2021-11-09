using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetApp.DAL.Repositories
{
    public interface IMeetRepository
    {
        public IQueryable<User> GetAllUsers();
        public IQueryable<Meet> GetAllMeets();
        public IQueryable<Meet> GetAllMeetsForOwner(User user);
        public IQueryable<Meet> GetAllMeetsForMember(User user);

        public IQueryable<T> Get<T>() where T : class;

        public void Add<T>(T newEntity) where T : class;

        public void SaveAll();

        public void DeleteMeet(Meet meet);

        public void DeleteUser(User user);

        public void DeleteMember(Member member);
    }
}
