

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HYT.MidiManager
{
    /// <summary>
    /// Represents a device capable of sending MIDI messages.
    /// </summary>
    public sealed class OutputDevice : OutputDeviceBase
    {
        #region Win32 Midi Output Functions and Constants


        //private static extern int midiOutOpen(ref UInt32 handle, UInt32 deviceID, MidiOutProc proc, UInt32 instance, UInt32 flags);
        //private static extern int midiOutOpen(out IntPtr handle, UInt32 uDeviceID, MidiOutProc dwCallback, UInt32 dwInstance, UInt32 dwFlags);
        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, UInt32 dwCallback, UInt32 dwInstance, UInt32 dwFlags);

        [DllImport("winmm.dll")]
        private static extern int midiOutClose(IntPtr hMidiOut);
        //private static extern int midiOutClose(UInt32 handle);

        #endregion

        private MidiOutProc midiOutProc;

        private bool runningStatusEnabled = false;

        private int runningStatus = 0;

        #region Construction

        /// <summary>
        /// Initializes a new instance of the OutputDevice class.
        /// </summary>
        public OutputDevice(int deviceID) : base(deviceID)
        {
            midiOutProc = HandleMessage;

            //var result = midiOutOpen(ref hndle, (uint)deviceID, midiOutProc, 0, CALLBACK_FUNCTION);
            IntPtr h_Device;

            //int result = midiOutOpen(out h_Device, (uint)deviceID, midiOutProc, 0, CALLBACK_FUNCTION);
            int result = midiOutOpen(out h_Device, 0, 0, 0, 0);

            if (result != MidiDeviceException.MMSYSERR_NOERROR)
            {
                _deviceHandle = IntPtr.Zero;

                throw new OutputDeviceException(result);
            }
            else
            {
                _deviceHandle = h_Device;
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (lockObject)
                {
                    Reset();

                    // Close the OutputDevice.
                    //int result = midiOutClose((uint)Handle);
                    
                    int result = midiOutClose(_deviceHandle);
                    _deviceHandle = IntPtr.Zero;
                    if (result != MidiDeviceException.MMSYSERR_NOERROR)
                    {
                        // Throw an exception.
                        throw new OutputDeviceException(result);
                    }
                }
            }
            else
            {
                midiOutReset(_deviceHandle);
                //midiOutClose((uint)Handle);

                _deviceHandle = IntPtr.Zero;
                midiOutClose(_deviceHandle);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Closes the OutputDevice.
        /// </summary>
        /// <exception cref="OutputDeviceException">
        /// If an error occurred while closing the OutputDevice.
        /// </exception>
        public override void Close()
        {
            #region Guard

            if (IsDisposed)
            {
                return;
            }

            #endregion

            Dispose(true);
        }

        /// <summary>
        /// Resets the OutputDevice.
        /// </summary>
        public override void Reset()
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            runningStatus = 0;

            base.Reset();
        }

        public override void Send(ChannelMessage message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            lock (lockObject)
            {
                // If running status is enabled.
                if (runningStatusEnabled)
                {
                    // If the message's status value matches the running status.
                    if (message.Status == runningStatus)
                    {
                        // Send only the two data bytes without the status byte.
                        Send(message.Message >> 8);
                    }
                    // Else the message's status value does not match the running
                    // status.
                    else
                    {
                        // Send complete message with status byte.
                        Send(message.Message);

                        // Update running status.
                        runningStatus = message.Status;
                    }
                }
                // Else running status has not been enabled.
                else
                {
                    Send(message.Message);
                }
            }
        }

        public override void Send(SysExMessage message)
        {
            // System exclusive cancels running status.
            runningStatus = 0;

            base.Send(message);
        }

        public override void Send(SysCommonMessage message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            // System common cancels running status.
            runningStatus = 0;

            base.Send(message);
        }

        public void PlaySend(int message)
        {
            #region Require

            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            #endregion

            lock (lockObject)
            {
                Send(message);
            }
        }

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the OutputDevice uses
        /// a running status.
        /// </summary>
        public bool RunningStatusEnabled
        {
            get
            {
                return runningStatusEnabled;
            }
            set
            {
                runningStatusEnabled = value;

                // Reset running status.
                runningStatus = 0;
            }
        }

        #endregion
    }

    /// <summary>
    /// The exception that is thrown when a error occurs with the OutputDevice
    /// class.
    /// </summary>
    public class OutputDeviceException : MidiDeviceException
    {
        #region OutputDeviceException Members

        #region Win32 Midi Output Error Function

        [DllImport("winmm.dll")]
        private static extern int midiOutGetErrorText(UInt32 errCode, StringBuilder message, UInt32 sizeOfMessage);

        #endregion

        #region Fields

        // The error message.
        private StringBuilder message = new StringBuilder(128);

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the OutputDeviceException class with
        /// the specified error code.
        /// </summary>
        /// <param name="errCode">
        /// The error code.
        /// </param>
        public OutputDeviceException(int errCode) : base(errCode)
        {
            // Get error message.
            midiOutGetErrorText((uint)errCode, message, (uint)message.Capacity);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return message.ToString();
            }
        }

        #endregion

        #endregion
    }
}
