using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace HYT.Test.WPF
{
    public partial class MainWindow : Window
    {
        [DllImport("CPPDLL.dll", EntryPoint = "add")]
        static extern int add(int a,int b);
        [DllImport("CPPDLL.dll", EntryPoint = "subtract")]
        static extern int subtract(int a, int b);


        public MainWindow()
        {
            InitializeComponent();

            var a  = MainWindow.add(5,5);
            var b= MainWindow.subtract(5,1);
            StringBuilder sb= new StringBuilder();
            sb.AppendLine("88");

        }
    }
}
