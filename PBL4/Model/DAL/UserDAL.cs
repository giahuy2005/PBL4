using PBL4.Model.Entities;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PBL4.Model.DAL
{
    public class UserDAL
    {
        private readonly Supabase.Client _client;

        public UserDAL()
        {
            const string SUPABASE_URL = "https://hpktabtbxcizjcagyids.supabase.co";
            const string SUPABASE_KEY = "sb_publishable_WlIoWQiaN7kffntdEKJ69w_ALs_srhe";

            _client = new Supabase.Client(SUPABASE_URL, SUPABASE_KEY);
        }

        // GỌI HÀM NÀY SAU KHI TẠO ĐỐI TƯỢNG
        public async Task InitializeAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                // Gửi request với timeout
                var requestTask = _client.From<User>().Limit(1).Get(cts.Token);

                await requestTask; // nếu quá 3 giây → OperationCanceledException

                // Báo thành công
                MessageBox.Show(
                    "Kết nối Supabase thành công!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return true;
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    "Không thể kết nối tới Supabase!\n\n" +
                    "Lỗi: Request timeout sau 3 giây.",
                    "Lỗi kết nối",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể kết nối tới Supabase!\n\n" +
                    "Lỗi: " + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }





        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var response = await _client.From<User>().Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving users: " + ex.Message);
                return new List<User>();
            }
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _client
                    .From<User>()
                    .Where(u => u.IdUser == userId)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                // Timeout 3s cho request tìm user
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                var response = await _client
                    .From<User>()
                    .Where(u => u.UserName == username)
                    .Single(cts.Token);

                return response;
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    "Lỗi: Request tìm user timeout sau 3 giây.",
                    "Timeout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi lấy user: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return null;
            }
        }


        public async Task<User> InsertUserAsync(User newUser)
        {
            try
            {
                var response = await _client.From<User>().Insert(newUser);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting user: {ex.Message}");
                return null;
            }
        }
    }
}
