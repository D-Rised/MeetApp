using System;

namespace MeetApp.Web.Models
{
    public class MemberViewModel
    {
        public string Login { get; set; }
        public bool IsOwner { get; set; }
        public string State { get; set; }
    }
}
