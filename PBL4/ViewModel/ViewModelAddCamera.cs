using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.Model;
using PBL4.Model.BO;
using PBL4.Model.Entities;
using PBL4.Services;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Media3D;
using Users = PBL4.Model.Entities.User;
// frame communitytoolkit mvvm
namespace PBL4.ViewModel
{
    public partial class ViewModelAddCamera : ObservableObject
    {
        private Users user;
        [ObservableProperty]
        private ObservableObject? currentView;
        private ViewModelLoadingAddCameraUC loadingAddCameraUC;
        private ViewModelAddCameraUC addCameraUC;
        private bool is_loading = false;
        private CameraClient _client;
        public ViewModelAddCamera(CameraClient _client,Users user)
        {
            // khởi tạo các viewmodel con
            this.user = user;
            loadingAddCameraUC = new ViewModelLoadingAddCameraUC();
            loadingAddCameraUC.IsCancel += OnIsCancel;
            addCameraUC = new ViewModelAddCameraUC();
            addCameraUC.IsAdd += OnIsAdd;
            addCameraUC.IsClose += OnIsClose;
            CurrentView = addCameraUC;
            this._client = _client;

        }
    


        private async Task OnIsAdd()
        {
            CurrentView = loadingAddCameraUC;
            string camName = addCameraUC.Namecamera!;
            string url = addCameraUC.Url!;
            string user = addCameraUC.Nameuser!;
            string pass = addCameraUC.Password!;
            bool isOk = await _client.CheckCamera("onlycamcheck", url,user,pass);

            if (!isOk)
            {
                CurrentView = addCameraUC;
                return;
            }
            else
            {
                CamerasBO camerabo = new CamerasBO();
                bool is_duplicate = await camerabo.IsCameraIpExistsAsync(this.user.IdUser,url); 
                if(is_duplicate)
                {
                    MessageBox.Show("Camera với địa chỉ IP này đã tồn tại.");
                    CurrentView = addCameraUC;
                    return;
                }
                var newCam = new Cameras
                {
                   // IdCamera = Guid.NewGuid().ToString(),
                    IdUser = this.user.IdUser,
                    NameCamera = camName,
                    URL = url,
                    NameUser = user,
                    Password = pass
                };
                MessageBox.Show(newCam.ToString());
                try
                {
                    var inserted = await camerabo.AddNewCameraAsync(newCam);

                    if (inserted != null)
                    {
                        MessageBox.Show("Thêm camera thành công");
                        Application.Current.Windows
                            .OfType<Window>()
                            .SingleOrDefault(w => w.DataContext == this)
                            ?.Close();
                        return;
                    }

                    // Nếu inserted = null → lỗi đã được log từ BO/DAL
                    MessageBox.Show("Thêm camera thất bại, vui lòng thử lại!",
                                    "Lỗi",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                    CurrentView = addCameraUC;
                }
                catch (Exception ex)
                {
                    // Hiện ERROR CHÍNH XÁC
                    MessageBox.Show($"Không thể thêm camera:\n{ex.Message}",
                                    "Lỗi hệ thống",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                    CurrentView = addCameraUC;
                }
            }
        }
        private void OnIsClose()
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
        private async void OnIsCancel()
        {
            bool isCancel= await _client.CancelCheckCamera("onlycamcheck");

            // chưa có lệnh gửi về server kêu nó dừng lại 
            CurrentView = addCameraUC;
        }

    }
}

