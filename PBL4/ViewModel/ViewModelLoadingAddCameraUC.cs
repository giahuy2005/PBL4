using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL4.ViewModel
{
    public partial class ViewModelLoadingAddCameraUC : ObservableObject
    {
        // event báo về cho viewmodelAddCamera 
        public event Action? IsCancel;
        // sự kiện khi người dùng ấn hủy 
        [RelayCommand]
        public void CancelAddCamera()
        {
            IsCancel?.Invoke();
        }
    }
}
