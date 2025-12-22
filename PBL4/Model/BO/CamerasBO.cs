using PBL4.Model.DAL;
using PBL4.Model.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace PBL4.Model.BO
{
    internal class CamerasBO : ICamerasBO
    {
        // Khai báo DAL
        private readonly CamerasDAL _camerasDal;

        public CamerasBO()
        {
            _camerasDal = new CamerasDAL();
        }


        public Task<List<Cameras>> LoadCamerasByUserIdAsync(string userId)
        {
            return _camerasDal.GetCamerasByUserIdAsync(userId);
        }

        public async Task<Cameras> AddNewCameraAsync(Cameras newCamera)
        {
            if (string.IsNullOrEmpty(newCamera.URL))
            {
                throw new ArgumentException("Camera URL is required.");
            }

            // Logic nghiệp vụ: Có thể giới hạn số lượng camera mỗi user
            // (Ví dụ: gọi UserBO để kiểm tra)

            // Gọi DAL và trả về kết quả đã được chèn (có ID)
            try
            {
                return  await _camerasDal.InsertCameraAsync(newCamera);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                MessageBox.Show(
                           $"Lỗi khi thêm camera vào database:\n{ex.Message}",
                           "Supabase Error",
                           MessageBoxButton.OK,
                           MessageBoxImage.Error
                       );
                return null;
            }
        }

        public async Task<bool> IsCameraIpExistsAsync(string userId, string url)
        {
            return await _camerasDal.IsCameraIpExistsAsync(userId, url);
        }
        public async Task<Cameras?> GetCamerasByIdAsync(string cameraId)
        {
            return await _camerasDal.GetCamerasByIdAsync(cameraId);
        }
        public async Task<bool> DesignInfoCamera(Cameras Camera)
        {
            return await _camerasDal.DesignInfoCamera(Camera);
        }
        public async Task<bool> DeleteCamera(string id_cameraToDelete)
        {
            return await _camerasDal.DeleteCameraAsync(id_cameraToDelete);
        }

    }
}
