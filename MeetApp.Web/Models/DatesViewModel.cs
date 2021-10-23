using System;

namespace MeetApp.Web.Models
{
    public class DatesViewModel
    {
        public int Id { get; set; }
        public Guid MeetId { get; set; }
        public Guid MemberId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string MemberLogin { get; set; }
    }
}
