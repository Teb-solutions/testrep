using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public interface IUserQueries
    {
        User GetUserById(int userid);
        IEnumerable<User> GetUsersById(IEnumerable<int> userid);
    }
}
