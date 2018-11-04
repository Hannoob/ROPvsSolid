using SOLIDExample.DomainObjects;
using SOLIDExample.Interfaces;

namespace SOLIDExample.Interfaces
{
    public interface IUserRepo
    {
        User GetUserDetails(string username);
    }
}