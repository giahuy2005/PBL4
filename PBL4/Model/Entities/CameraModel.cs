using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace PBL4.Model
{
    public partial class CameraModel : ObservableObject
    {
        [ObservableProperty]
        private string camId = "";

        [ObservableProperty]
        private BitmapImage? image;
    }
}

