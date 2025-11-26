using PBL4.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBL4.Model.BO
{
    public interface IUserBO
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string userId);
        Task<User> RegisterUserAsync(User newUser);
        Task<bool> CheckUserLogin(string username, string password);
    }
}