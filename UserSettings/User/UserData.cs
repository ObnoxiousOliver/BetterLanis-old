using BetterLanis.UserSettings.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterLanis.UserSettings.User
{
    public class UserData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Class { get; set; }

        public LoginData LoginData { get; set; }
    }
}