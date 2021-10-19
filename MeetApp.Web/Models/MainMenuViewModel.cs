using MeetApp.DAL.Models;
using System;
using System.Collections.Generic;

namespace MeetApp.Models
{
    public class MainMenuViewModel
    {
        public string userLogin { get; set; }
        public IList<Meet> ownedMeets { get; set; }
        public IList<Meet> memberMeets { get; set; }
        public string alertMessage { get; set; }
    }
}
