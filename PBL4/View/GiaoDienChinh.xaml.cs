using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PBL4.View
{
    /// <summary>
    /// Interaction logic for GiaoDienChinh.xaml
    /// </summary>
    public partial class GiaoDienChinh : Window
    {
        private bool _isClosingHandled = false;
        public GiaoDienChinh()
        {
            InitializeComponent();
        }
        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (DataContext is PBL4.ViewModel.ViewModelGiaoDienChinh vm)
            {
                await vm.StopServer();
            }
        }

    }


}
