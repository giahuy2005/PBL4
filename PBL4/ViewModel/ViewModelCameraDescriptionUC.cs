using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.Model;

namespace PBL4.ViewModel
{
    public partial class ViewModelCameraDescriptionUC : ObservableObject
    {
        [ObservableProperty]
        private string namecamera;
        [ObservableProperty]
        private string url;
        [ObservableProperty]
        private string nameuser;
        [ObservableProperty]
        private string password;
        // hàm event báo về để tắt view 
        public event Action? IsClose;
        public ViewModelCameraDescriptionUC(CameraModel Cam)
        {
            Namecamera = Cam.NameCamera ?? "";
            Url = Cam.Url ?? "";
            Nameuser = Cam.NameUser ?? "";
            Password = Cam.Password ?? "";
        }
        [RelayCommand]
        public void CloseDescriptionCamera()
        {
            IsClose?.Invoke();
        }
        [RelayCommand]
        public void SaveInfoCamera()
        {
            MessageBox.Show("đã save");
        }
    } 
}
