using KT.TCP.Train;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.Windows.Forms.AxHost;

namespace KCL
{
    public class DeviceDataAnalysisManager_JSAPI
    {
        private DeviceDataAnalysisManager_JSAPI()
        {
        }
        public static readonly DeviceDataAnalysisManager_JSAPI Instance = new DeviceDataAnalysisManager_JSAPI();

        public static DeviceDataAnalysisManager Fun = DeviceDataAnalysisManager.Instance;

    }

}