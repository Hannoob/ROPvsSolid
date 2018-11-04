using Microsoft.Extensions.Logging;
using SOLIDExample.DomainObjects;
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
        private readonly IEmailService _emailService;
        private readonly ILogger<UserHandler> _log;

        public UserHandler(ILogger<UserHandler> log, IUserRepo userRepo, IPasswordUtils passwordUtils, IEmailService emailService)
        {
            _log = log;
            _userRepo = userRepo;
            _passwordUtils = passwordUtils;
            _emailService = emailService;
        }

        public User AuthenticateUser(string username, string password)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                throw new Exception("The username and password cannot be null");

            User userDetails = null;
            try
            {
                userDetails = _userRepo.GetUserDetails(username);
            }
            catch (Exception e)
            {
                _log.LogError("Failed to load user from the DB, this could be because the user does not exist", e);
            }

            if (userDetails == null)
                throw new Exception("Authentication Failed");

            if (!_passwordUtils.ComparePasswords(password,userDetails.Password))
                throw new Exception("Authentication Failed");

            try
            {
                _emailService.EmailClient(userDetails.Email, "You have logged into your new awesome fitness app!");
            }
            catch (Exception ex)
            {
                _log.LogDebug("Failed to send login confirmation email to client");
            }

            _log.LogDebug("User successfully logged in!");

            return userDetails;
        }

    }
}
