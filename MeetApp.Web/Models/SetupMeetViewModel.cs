using System;
using System.Collections.Generic;

namespace MeetApp.Models
{
    public class SetupMeetViewModel
    {
        public Guid meetId { get; set; }
        public string title { get; set; }
        public string userLogin { get; set; }
        public string state { get; set; }
        public string memberState { get; set; }
        public bool fixedDate { get; set; }
        public int index { get; set; }
        public IList<DatesViewModel> DatesList { get; set; }
        public IList<MemberViewModel> MembersList { get; set; }
        public string alertMessage { get; set; }
    }
}
