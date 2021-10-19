using System;
using System.Collections.Generic;

namespace MeetApp.Models
{
    public class SetupMeetViewModel
    {
        public Guid MeetId { get; set; }
        public string Title { get; set; }
        public string UserLogin { get; set; }
        public string State { get; set; }
        public string MemberState { get; set; }
        public bool FixedDate { get; set; }
        public int Index { get; set; }
        public IList<DatesViewModel> DatesList { get; set; }
        public IList<MemberViewModel> MembersList { get; set; }
        public string AlertMessage { get; set; }
    }
}
