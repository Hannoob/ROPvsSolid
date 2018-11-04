using SOLIDExample.DataAccessLayer.DataAccessObjects;
using SOLIDExample.DomainObjects;
using SOLIDExample.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOLIDExample.DataAccessLayer
{
    public class UserRepo : IUserRepo
    {
        private UserDataAccessObject GenerateUser()
        {
            return new UserDataAccessObject()
            {
                UserId = "",
                Email = "email@server.co.za",
                Username = "Username",
                Password = "Password"
            };
        }

        public User GetUserDetails(string username)
        {
            return DataAccessObjectFactory.create(GenerateUser());
        }
    }
}
