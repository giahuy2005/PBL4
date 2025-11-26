using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PBL4.Model.BO;
using PBL4.Model.Entities;
namespace PBL4.ViewModel
{
    public partial class ViewModelSignInUC : ObservableObject
    {
        public event Action? LoginSucceeded;

        [ObservableProperty]
        private string? username;
        [ObservableProperty]
        private string? password;
        [RelayCommand]
        async void Login()
        {
            // truy cập database 
            UserBO userBO = new UserBO();
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Username và Password không được để trống.");
                return;
            }
            try
            {
                bool result = await userBO.CheckUserLogin(Username, Password);
                if (result)
                {
                    LoginSucceeded?.Invoke();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public ViewModelSignInUC()
        {

        }

    }
}
