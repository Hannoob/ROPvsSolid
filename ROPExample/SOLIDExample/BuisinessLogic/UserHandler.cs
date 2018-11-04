using SOLIDExample.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOLIDExample.BuisinessLogic
{
    public class UserHandler
    {
        private readonly IUserRepo _userRepo;
        private readonly IPasswordUtils _passwordUtils;

        public UserHandler(IUserRepo userRepo, IPasswordUtils passwordUtils)
        {
            _userRepo = userRepo;
            _passwordUtils = passwordUtils;
        }

        public void AuthenticateUser(string username, string password)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                throw new Exception("The username and password cannot be null");

            var userDetails = _userRepo.GetUserDetails(username);
            if (userDetails == null)
                throw new Exception("Authentication Failed");

            if (!_passwordUtils.ComparePasswords(password,userDetails.Password))
                throw new Exception("Authentication Failed");

            if ()
            //>> ROP.tee emailClient
            //>> ROP.tee saveToHistoryLog
        }

    }
}
