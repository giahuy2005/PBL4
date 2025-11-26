using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PBL4.Model.Entities;

namespace PBL4.Model.DAL
{
    class CamerasDAL
    {
        private readonly Supabase.Client _client;

        public CamerasDAL()
        {
            const string SUPABASE_URL = "db.hpktabtbxcizjcagyids.supabase.co";
            const string SUPABASE_KEY = "sb_publishable_WlIoWQiaN7kffntdEKJ69w_ALs_srhe";
            _client = new Supabase.Client(SUPABASE_URL, SUPABASE_KEY);
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
        public async Task<Cameras> InsertCameraAsync(Cameras newCamera)
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
                Console.WriteLine($"Error inserting camera: {ex.Message}");
                return null;
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