

using Sanford.Threading;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace HYT.MidiManager
{
    public abstract class OutputDeviceBase : MidiDevice
    {
        [DllImport("winmm.dll")]
        protected static extern int midiOutReset(IntPtr handle);

        [DllImport("winmm.dll")]
        private static extern int midiOutShortMsg(IntPtr hMidiOut, UInt32 dwMsg);
        //protected static extern int midiOutShortMsg(UInt32 handle, UInt32 message);


        [DllImport("winmm.dll")]
        protected static extern int midiOutPrepareHeader(IntPtr handle, IntPtr headerPtr, UInt32 sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        protected static extern int midiOutUnprepareHeader(IntPtr handle, IntPtr headerPtr, UInt32 sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        protected static extern int midiOutLongMsg(IntPtr handle, IntPtr headerPtr, UInt32 sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        protected static extern int midiOutGetDevCaps(UInt32 deviceID, ref MidiOutCaps caps, UInt32 sizeOfMidiOutCaps);

        [DllImport("winmm.dll")]
        protected static extern int midiOutGetNumDevs();

        protected const int MOM_OPEN = 0x3C7;
        protected const int MOM_CLOSE = 0x3C8;
        protected const int MOM_DONE = 0x3C9;

        protected delegate void GenericDelegate<T>(T args);

        // Represents the method that handles messages from Windows.
        protected delegate void MidiOutProc(int handle, int msg, int instance, int param1, int param2);

        // For releasing buffers.
        protected DelegateQueue delegateQueue = new DelegateQueue();

        protected readonly object lockObject = new object();

        // The number of buffers still in the queue.
        protected int bufferCount = 0;

        // Builds MidiHeader structures for sending system exclusive messages.
        private MidiHeaderBuilder headerBuilder = new MidiHeaderBuilder();

        // The device handle.
        protected UInt32 hndle = 0;


        public IntPtr _deviceHandle;
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

        public OutputDeviceBase(int deviceID) : base(deviceID)
        {
        }

        ~OutputDeviceBase()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                delegateQueue.Dispose();
            }

            base.Dispose(disposing);
        }

        public virtual void Send(ChannelMessage message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            Send(message.Message);
        }

        public virtual void Send(SysExMessage message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            lock (lockObject)
            {
                headerBuilder.InitializeBuffer(message);
                headerBuilder.Build();

                // Prepare system exclusive buffer.
                int result = midiOutPrepareHeader(_deviceHandle, headerBuilder.Result, (uint)SizeOfMidiHeader);

                // If the system exclusive buffer was prepared successfully.
                if (result == MidiDeviceException.MMSYSERR_NOERROR)
                {
                    bufferCount++;

                    // Send system exclusive message.
                    result = midiOutLongMsg(_deviceHandle, headerBuilder.Result, (uint)SizeOfMidiHeader);

                    // If the system exclusive message could not be sent.
                    if (result != MidiDeviceException.MMSYSERR_NOERROR)
                    {
                        midiOutUnprepareHeader(_deviceHandle, headerBuilder.Result, (uint)SizeOfMidiHeader);
                        bufferCount--;
                        headerBuilder.Destroy();

                        // Throw an exception.
                        throw new OutputDeviceException(result);
                    }
                }
                // Else the system exclusive buffer could not be prepared.
                else
                {
                    // Destroy system exclusive buffer.
                    headerBuilder.Destroy();

                    // Throw an exception.
                    throw new OutputDeviceException(result);
                }
            }
        }

        public virtual void Send(SysCommonMessage message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            Send(message.Message);
        }

        public virtual void Send(SysRealtimeMessage message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            Send(message.Message);
        }

        public override void Reset()
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            lock (lockObject)
            {
                // Reset the OutputDevice.
                int result = midiOutReset(_deviceHandle);

                if (result == MidiDeviceException.MMSYSERR_NOERROR)
                {
                    while (bufferCount > 0)
                    {
                        Monitor.Wait(lockObject);
                    }
                }
                else
                {
                    // Throw an exception.
                    throw new OutputDeviceException(result);
                }
            }
        }

        protected void Send(int message)
        {
            lock (lockObject)
            {
                //int result = (uint)midiOutShortMsg((uint)Handle, (uint)message);

                int result = midiOutShortMsg(_deviceHandle, (uint)message);

                if (result != MidiDeviceException.MMSYSERR_NOERROR)
                {
                    throw new OutputDeviceException(result);
                }
            }
        }

        public static MidiOutCaps GetDeviceCapabilities(int deviceID)
        {
            MidiOutCaps caps = new MidiOutCaps();

            // Get the device's capabilities.
            int result = midiOutGetDevCaps((uint)deviceID, ref caps, (uint)Marshal.SizeOf(caps));

            // If the capabilities could not be retrieved.
            if (result != MidiDeviceException.MMSYSERR_NOERROR)
            {
                // Throw an exception.
                throw new OutputDeviceException(result);
            }

            return caps;
        }

        // Handles Windows messages.
        protected virtual void HandleMessage(int handle, int msg, int instance, int param1, int param2)
        {
            if (msg == MOM_OPEN)
            {
            }
            else if (msg == MOM_CLOSE)
            {
            }
            else if (msg == MOM_DONE)
            {
                delegateQueue.Post(ReleaseBuffer, new IntPtr(param1));
            }
        }

        // Releases buffers.
        private void ReleaseBuffer(object state)
        {
            lock (lockObject)
            {
                IntPtr headerPtr = (IntPtr)state;

                // Unprepare the buffer.
                int result = midiOutUnprepareHeader(_deviceHandle, headerPtr, (uint)SizeOfMidiHeader);

                if (result != MidiDeviceException.MMSYSERR_NOERROR)
                {
                    Exception ex = new OutputDeviceException(result);

                    OnError(new Sanford.Multimedia.ErrorEventArgs(ex));
                }

                // Release the buffer resources.
                headerBuilder.Destroy(headerPtr);

                bufferCount--;

                Monitor.Pulse(lockObject);

                Debug.Assert(bufferCount >= 0);
            }
        }

        public override void Dispose()
        {
            #region Guard

            if (IsDisposed)
            {
                return;
            }

            #endregion

            lock (lockObject)
            {
                Close();
            }
        }

        public override int Handle
        {
            get
            {
                return (int)hndle;
            }
        }


        public static int DeviceCount
        {
            get
            {
                return midiOutGetNumDevs();
            }
        }

    }
}
