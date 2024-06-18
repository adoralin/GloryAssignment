using System.Windows;
using GloryAssignment.ViewModel;

namespace GloryAssignment
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            DataContext= mainWindowViewModel;
        }
    }
}
