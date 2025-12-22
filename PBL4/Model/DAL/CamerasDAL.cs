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
    class CamerasDAL
    {
        private readonly Supabase.Client _client;

        public CamerasDAL()
        {
            const string SUPABASE_URL = "https://hpktabtbxcizjcagyids.supabase.co";
            const string SUPABASE_KEY = "sb_publishable_WlIoWQiaN7kffntdEKJ69w_ALs_srhe";
            _client = new Supabase.Client(SUPABASE_URL, SUPABASE_KEY);
        }
        public string ExtractIp(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Authority;
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> IsCameraIpExistsAsync(string userId, string url)
        {
            var ip = ExtractIp(url);
            try
            {
                var response = await _client
                    .From<Cameras>()
                    .Where(c => c.IdUser == userId)
                    .Get();

                return response.Models.Any(c =>
                {

                    var existingIp = ExtractIp(c.URL);
                    return existingIp == ip;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking ip: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Cameras>> GetCamerasByUserIdAsync(string userId)
        {
            try
            {
                var response = await _client
                    .From<Cameras>()
                    // Sử dụng cú pháp hiện đại (Lambda)
                    .Where(c => c.IdUser == userId)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving cameras for user {userId}: {ex.Message}");
                return new List<Cameras>();
            }
        }
        public async Task<Cameras?> GetCamerasByIdAsync(string cameraId)
        {
            try
            {
                var response = await _client
                    .From<Cameras>()
                    .Where(c => c.IdCamera == cameraId)
                    .Get();
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving camera with ID {cameraId}: {ex.Message}");
                return null;
            }
        }
        public async Task<Cameras?> InsertCameraAsync(Cameras newCamera)
        {
            try
            {
                var response = await _client
                    .From<Cameras>()
                    .Insert(newCamera);

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                 $"Lỗi khi thêm camera vào database:\n{ex.Message}",
                 "Supabase Error",
                 MessageBoxButton.OK,
                   MessageBoxImage.Error
                );
                throw; 
            }
        }
        public async Task<Boolean> DesignInfoCamera(Cameras Camera)
        {
            try
            {
                var response = await _client
                .From<Cameras>()
                .Update(Camera);
                return response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                 $"Lỗi khi sửa camera vào database:\n{ex.Message}",
                 "Supabase Error",
                 MessageBoxButton.OK,
                   MessageBoxImage.Error
                );
                return false;

            }
        }
        public async Task<bool> DeleteCameraAsync(string idCamera)
        {
            try
            {
                await _client
                    .From<Cameras>()
                    .Where(c => c.IdCamera == idCamera)
                    .Delete();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting camera: {ex.Message}");
                return false;
            }
        }

    }
}