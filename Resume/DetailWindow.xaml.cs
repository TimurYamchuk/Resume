using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Resume
{
    /// <summary>
    /// Interaction logic for DetailWindow.xaml
    /// </summary>
    public partial class DetailWindow : Window
    {
        public DetailWindowViewModel DetailVM { get; }

        public DetailWindow(ResumeModel data)
        {
            InitializeComponent();
            DetailVM = new DetailWindowViewModel(data);
            DataContext = DetailVM;
        }
    }
}
