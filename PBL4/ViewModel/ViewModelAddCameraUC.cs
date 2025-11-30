using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PBL4.ViewModel
{
    public partial class ViewModelAddCameraUC : ObservableObject
    {
        // báo cho  thằng view AddModel biết để add vào 
        public event Func<Task>? IsAdd;
        // báo cho thằng view AddModel biết để đóng
        public event Action? IsClose;

        [ObservableProperty]
        private string? namecamera;
        [ObservableProperty]
        private string? url;
        [ObservableProperty]
        private string? nameuser;
        [ObservableProperty]
        private string? password;
        public ViewModelAddCameraUC()
        {

        }
        [RelayCommand]
        public void AddCamera()
        {
            MessageBox.Show("" + Namecamera + "," + Url + "," + Nameuser + "," + Password);


            if (string.IsNullOrWhiteSpace(Namecamera) || string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(Nameuser) || string.IsNullOrWhiteSpace(Password))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập đầy đủ trường thông tin");
                return;
            }
            MessageBox.Show("" + Namecamera + "," + Url + "," + Nameuser + "," + Password);
            // báo về cho view ViewModelAddCamera 
            IsAdd?.Invoke();
        }
       
        [RelayCommand]
        public void CloseAddCamera()
        {
            IsClose?.Invoke();
        }
    }
}
