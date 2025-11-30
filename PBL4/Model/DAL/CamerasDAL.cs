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
                return uri.Host; 
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> IsCameraIpExistsAsync(string userId, string ip)
        {
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



        public async Task DeleteCameraAsync(Cameras cameraToDelete)
        {
            try
            {
                await _client
                    .From<Cameras>()
                    .Delete(cameraToDelete);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting camera: {ex.Message}");
                throw; // Ném lỗi lên lớp Repository xử lý
            }
        }
    }
}