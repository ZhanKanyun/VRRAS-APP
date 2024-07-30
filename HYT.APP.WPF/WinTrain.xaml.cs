using CL.Common;
using KCL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace HYT.APP.WPF
{
    /// <summary>
    /// WinTrain.xaml 的交互逻辑
    /// </summary>
    public partial class WinTrain : Window
    {
        #region ■------------------ 构造加载


        public WinTrain()
        {
            InitializeComponent();

            try
            {

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

    }


}
