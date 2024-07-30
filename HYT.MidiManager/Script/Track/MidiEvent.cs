
using System;
using System.Text;

namespace HYT.MidiManager
{
    public class MidiEvent
    {
       
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public string BytearrayToHexStr(byte[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte value in array)
            {
                char high = (char)((value >> 4) & 0x0f);
                char low = (char)(value & 0x0f);
                high = Convert.ToChar(high < 10 ? (high + '0') : (high - (char)10 + 'A'));
                low = Convert.ToChar(low < 10 ? (low + '0') : (low - (char)10 + 'A'));
                builder.Append(high);
                builder.Append(low);
                builder.Append(" ");
            }
            return builder.ToString();
        }

        public override string ToString()
        {
            return $"  Tick={AbsoluteTicks.ToString("D6")}   类型={message.MessageType.ToString().PadLeft(7,' ')}   数据={BytearrayToHexStr(message.GetBytes())}";
        }
        private object owner = null;

        private int absoluteTicks;

        private IMidiMessage message;

        private MidiEvent next = null;

        private MidiEvent previous = null;

        internal MidiEvent(object owner, int absoluteTicks, IMidiMessage message)
        {
            #region Require

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            else if (absoluteTicks < 0)
            {
                throw new ArgumentOutOfRangeException("absoluteTicks", absoluteTicks,
                    "Absolute ticks out of range.");
            }
            else if (message == null)
            {
                throw new ArgumentNullException("e");
            }

            #endregion

            this.owner = owner;
            this.absoluteTicks = absoluteTicks;
            this.message = message;
        }

        internal void SetAbsoluteTicks(int absoluteTicks)
        {
            this.absoluteTicks = absoluteTicks;
        }

        internal object Owner
        {
            get
            {
                return owner;
            }
        }

        public int AbsoluteTicks
        {
            get
            {
                return (int)(absoluteTicks / SpeedManager.Instance.Speed);
                //return (int)Math.Round((absoluteTicks * SpeedManager.Instance.Speed));
            }
        }

        public int DeltaTicks
        {
            get
            {
                int deltaTicks;

                if (Previous != null)
                {
                    deltaTicks = AbsoluteTicks - previous.AbsoluteTicks;
                }
                else
                {
                    deltaTicks = AbsoluteTicks;
                }

                return deltaTicks;
            }
        }

        public IMidiMessage MidiMessage
        {
            get
            {
                return message;
            }
        }

        internal MidiEvent Next
        {
            get
            {
                return next;
            }
            set
            {
                next = value;
            }
        }

        internal MidiEvent Previous
        {
            get
            {
                return previous;
            }
            set
            {
                previous = value;
            }
        }


        public bool IsDone = false;
    }
}
