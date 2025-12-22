using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.Model;
using PBL4.Model.BO; 
using PBL4.Model.Entities;
using PBL4.Services;
namespace PBL4.ViewModel
{
    public partial class ViewModelCameraDescriptionUC : ObservableObject
    {
        private string camera_id; 
        [ObservableProperty]
        private string namecamera;
        [ObservableProperty]
        private string url;
        [ObservableProperty]
        private string nameuser;
        [ObservableProperty]
        private string password;
        [ObservableProperty]
        private string userid;
        // hàm event báo về để tắt view 
        public event Action? IsClose;
        public Cameras? camera;
        private CameraClient _client;
        public event Action? IsDesgin;


        public ViewModelCameraDescriptionUC(CameraModel Cam,CameraClient client)
        {
            // lấy camera từ database 
            camera_id = Cam.CamId ?? "";
            _client = client;
            LoadCameraInfoAsync();
        }
        private async void LoadCameraInfoAsync()
        {
            try
            {

                 camera = await new CamerasBO().GetCamerasByIdAsync(camera_id);

                if (camera != null)
                {
                    Namecamera = camera.NameCamera ?? "";
                    Url = camera.URL ?? "";
                    Nameuser = camera.NameUser ?? "";
                    Password = camera.Password ?? "";
                    Userid = camera.IdUser ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }
        [RelayCommand]
        public void CloseDescriptionCamera()
        {
            IsClose?.Invoke();
        }
        [RelayCommand]
        public async void SaveInfoCamera()
        {
            if (_client == null || !_client.check_ws())
            {
                MessageBox.Show("Mất kết nối tới Server kiểm tra camera. Vui lòng thử lại sau.", "Lỗi kết nối");
                return;
            }

            CamerasBO camerabo = new CamerasBO();

            bool isChanged = false;
            bool isConnectionChanged = false;

            if (Namecamera != camera?.NameCamera)
            {
                isChanged = true;
            }

            if (Url != camera?.URL)
            {

                bool checkip = await camerabo.IsCameraIpExistsAsync(Userid, Url);
                if (checkip)
                {
                    MessageBox.Show("Địa chỉ IP và Port này đã tồn tại trong hệ thống!", "Trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Dừng ngay, không lưu gì cả
                }

                isChanged = true;
                isConnectionChanged = true; // Bật cờ này lên
            }

            if (Nameuser != camera?.NameUser)
            {
                isChanged = true;
                isConnectionChanged = true; // Bật cờ này lên
            }

            // Check Pass
            if (Password != camera?.Password)
            {
                isChanged = true;
                isConnectionChanged = true; // Bật cờ này lên
            }
            if (!isChanged)
            {
                MessageBox.Show("Không có thay đổi nào để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


            if (isConnectionChanged)
            {


  
                bool isOk = await _client.CheckCamera("check_temp_id", Url, Nameuser, Password);

                if (!isOk)
                {
                    MessageBox.Show("Không thể kết nối tới camera! Vui lòng kiểm tra lại URL, Tài khoản hoặc Mật khẩu.", "Lỗi kết nối camera", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Dừng ngay, không lưu
                }
            }

            camera!.NameCamera = Namecamera;
            camera!.URL = Url;
            camera!.NameUser = Nameuser;
            camera!.Password = Password;

            try
            {
                await camerabo.DesignInfoCamera(camera);
                MessageBox.Show("Đã lưu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu vào Database: " + ex.Message);
            }
        }

     }
}
