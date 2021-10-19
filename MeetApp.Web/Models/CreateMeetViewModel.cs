using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Models
{
    public class CreateMeetViewModel
    {
        public string title { get; set; }
        public string userLogin { get; set; }
        public bool fixedDate { get; set; }
        public IList<Dates> DatesList { get; set; }
        public string alertMessage { get; set; }
    }
}
