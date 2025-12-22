using PBL4.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBL4.Model.BO
{
    public interface ICamerasBO
    {
        Task<List<Cameras>> LoadCamerasByUserIdAsync(string userId);
        Task<Cameras> AddNewCameraAsync(Cameras newCamera);
        Task<bool> IsCameraIpExistsAsync(string userId, string ip);
        Task<Cameras> GetCamerasByIdAsync(string cameraId);
        Task<bool> DesignInfoCamera(Cameras cameraToUpdate);
        Task<bool> DeleteCamera(string id_cameraToDelete);
        
    }
}