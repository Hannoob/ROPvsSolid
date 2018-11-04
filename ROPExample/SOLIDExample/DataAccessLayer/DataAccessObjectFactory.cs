using SOLIDExample.DataAccessLayer.DataAccessObjects;
using SOLIDExample.DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOLIDExample.DataAccessLayer
{
    public class DataAccessObjectFactory
    {
        public static User create(UserDataAccessObject userDataAccessObject)
        {
            return new User()
            {
                UserId = userDataAccessObject.UserId,
                Email = userDataAccessObject.Email,
                Username = userDataAccessObject.Username,
                Password = userDataAccessObject.Password
            };
        }
    }
}
