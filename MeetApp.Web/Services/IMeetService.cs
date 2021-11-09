using MeetApp.DAL.Models;
using MeetApp.Web.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Web.Services
{
    public interface IMeetService
    {
        public Member GetMemberByUserIdAndMeetId(Guid userId, Guid meetId);

        public List<Meet> GetAllOwnedMeetsForUser(Guid guid);

        public List<Meet> GetAllMemberMeetsForUser(Guid guid);

        public Meet GetMeetById(Guid id);

        public void CreateNewMeet(Meet meet);

        public StatusResult JoinMeet(User user, Guid meetId);

        public void SaveAll();

        public void DeleteMember(Member member);

        public void DeleteMeet(Meet meet);
    }
}
