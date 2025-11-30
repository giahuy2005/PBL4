using PBL4.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBL4.Model.BO
{
    public interface ICamerasBO
    {
        Task<List<Cameras>> LoadCamerasByUserIdAsync(string userId);
        Task<Cameras> AddNewCameraAsync(Cameras newCamera);
        Task DeleteCameraAsync(Cameras cameraToDelete);
        Task<bool> IsCameraIpExistsAsync(string userId, string ip);
    }
}