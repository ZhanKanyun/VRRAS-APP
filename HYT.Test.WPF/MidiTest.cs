using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HYT.Test.WPF
{
    public class MidiTest
    {
        [DllImport("winmm.dll")]
        private static extern UInt32 midiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, UInt32 dwCallback, UInt32 dwInstance, UInt32 dwFlags);

        [DllImport("winmm.dll")]
        private static extern UInt32 midiOutClose(IntPtr hMidiOut);

        [DllImport("winmm.dll")]
        private static extern UInt32 midiOutShortMsg(IntPtr hMidiOut, UInt32 dwMsg);

        private bool _isOpened;
        private IntPtr _deviceHandle;

        public bool IsOpened
        {
            get
            {
                return _isOpened;
            }
        }

        /// <summary>
        /// 设备句柄,可以不公开
        /// </summary>
        public IntPtr DeviceHandle
        {
            get
            {
                return _deviceHandle;
            }
        }

        public uint ShortPlay(uint msg)
        {
            if (_isOpened)
                return midiOutShortMsg(_deviceHandle, msg);
            else
                return 621;
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="key">音高(音调)</param>
        /// <param name="volume">音量</param>
        /// <param name="chenel">通道</param>
        /// <returns></returns>
        public uint ShortPlay(uint key, uint volume, uint chenel)
        {
            return ShortPlay(144 + key * 256 + volume * 65536 + chenel);
        }

        public IntPtr Open()
        {
            if (_isOpened)
                return _deviceHandle;

            IntPtr h_Device;
            uint h_r = midiOutOpen(out h_Device, 0, 0, 0, 0);
            _isOpened = (h_r == 0);

            if (_isOpened)
            {
                _deviceHandle = h_Device;
                return h_Device;
            }
            else
            {
                _deviceHandle = IntPtr.Zero;
                return IntPtr.Zero;
            }
        }

        public uint Close()
        {
            if (_isOpened)
            {
                _isOpened = false;
                _deviceHandle = IntPtr.Zero;
                return midiOutClose(_deviceHandle);
            }
            else
                return 5;
        }
    }
}
