using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCL
{
    public class ScreenMananger
    {
        #region ■------------------ 单例

        private ScreenMananger() { }
        public readonly static ScreenMananger Instance = new ScreenMananger();

        #endregion

        public Screen MainScreen
        {
            get {
                return Screen.PrimaryScreen;
            }
        }

        public Screen SecondScreen
        {
            get
            {
                if (Screen.AllScreens.Count()<2)
                {
                    return null;
                }
                return Screen.AllScreens.First(o=>o.Primary==false);
            }
        }
    }
}
