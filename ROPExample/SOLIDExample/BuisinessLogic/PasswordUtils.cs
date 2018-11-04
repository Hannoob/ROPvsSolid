using SOLIDExample.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOLIDExample.BuisinessLogic
{
    public class PasswordUtils : IPasswordUtils
    {
        public bool ComparePasswords(string password1, string password2)
        {
            return (password1 == password2);
        }
    }
}
