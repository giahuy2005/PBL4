using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PBL4.Model.DAL;
using PBL4.Model.Entities;

namespace PBL4.Model.BO
{
    internal class CamerasBO : ICamerasBO
    {
        // Khai báo DAL
        private readonly CamerasDAL _camerasDal;

        public CamerasBO(CamerasDAL camerasDal)
        {
            _camerasDal = camerasDal;
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
            return await _camerasDal.InsertCameraAsync(newCamera);
        }

        public async Task DeleteCameraAsync(Cameras cameraToDelete)
        {
            // Logic nghiệp vụ: Kiểm tra quyền sở hữu camera
            // (Ví dụ: nếu cameraToDelete.IdUser != currentUserId thì throw exception)

            // Gọi DAL
            await _camerasDal.DeleteCameraAsync(cameraToDelete);
        }
    }
}