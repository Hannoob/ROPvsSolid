using SOLIDExample.DomainObjects;

namespace SOLIDExample.Interfaces
{
    public interface IUserRepo
    {
        User GetUserDetails(string username);
    }
}