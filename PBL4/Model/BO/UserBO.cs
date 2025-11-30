using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PBL4.Model.DAL;
using PBL4.Model.Entities;

namespace PBL4.Model.BO
{
    internal class UserBO : IUserBO
    {
        private readonly UserDAL _userDal;

        public UserBO()
        {
            _userDal = new UserDAL();
            _ = InitializeAsync();   // fire-and-forget (không block UI)
        }

        private async Task InitializeAsync()
        {
            await _userDal.InitializeAsync();
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return _userDal.GetAllUsersAsync();
        }

        public Task<User> GetUserByIdAsync(string userId)
        {
            return _userDal.GetUserByIdAsync(userId);
        }

        public async Task<User> RegisterUserAsync(User newUser)
        {
            if (string.IsNullOrEmpty(newUser.UserName) ||
                string.IsNullOrEmpty(newUser.Password))
            {
                throw new ArgumentException("Username và Password trống.");
            }

            var existingUser = await _userDal.GetUserByUsernameAsync(newUser.UserName);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username đã tồn tại");
            }

            return await _userDal.InsertUserAsync(newUser);
        }

        public async Task<bool> CheckUserLogin(string username, string password)
        {
            var user = await _userDal.GetUserByUsernameAsync(username);

            if (user == null)
                throw new ArgumentException("Username không tồn tại: " + username);

            return user.Password == password;
        }
        public Task<User> GetUserByUserNameAsync(string username)
        {
            return _userDal.GetUserByUsernameAsync(username);
        }
    }
}
