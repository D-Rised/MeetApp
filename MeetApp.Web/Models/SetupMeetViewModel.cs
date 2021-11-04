using System;
using System.Collections.Generic;

namespace MeetApp.Web.Models
{
    public class SetupMeetViewModel
    {
        public Guid MeetId { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string State { get; set; }
        public string MemberState { get; set; }
        public bool FixedDate { get; set; }
        public IList<DatesViewModel> DatesList { get; set; }
        public IList<MemberViewModel> MembersList { get; set; }
        public string AlertMessage { get; set; }
        public int DateIndexToDelete { get; set; }
    }
}
