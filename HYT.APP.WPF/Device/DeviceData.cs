using CL.Common;
using CL.Common.Data;
using System;

namespace HYT.APP.WPF.Device
{
    /// <summary>
    /// 设备数据
    /// </summary>
    public class DeviceData : NotifyPropertyClass
    {
        public int Index { get; set; }
        /// <summary>
        /// 数据类型 0=非步行状态改变 1=左脚离地 2=左脚着地 3=右脚离地 4=右脚着地
        /// </summary>
        public StepTypes StepType { get; set; } = 0;

        public int DataLength { get; set; } = 14;

        public int State
        {
            get { return State_IsRun + State_IsTXGZ + State_IsDJGZ + State_IsJJTZ+State_IsTXGZ_CJ; }
        }

        private int _State_IsRun = 1 ;
        /// <summary>
        /// 电机状态 0启动 1停止
        /// </summary>
        public int State_IsRun
        {
            get { return _State_IsRun; }
            set { _State_IsRun = value;NotifyPropertyChanged(nameof(State_IsRun)); }
        }

        private int _State_IsTXGZ;
        /// <summary>
        /// 电机状态 0正常 1通信故障
        /// </summary>
        public int State_IsTXGZ
        {
            get { return _State_IsTXGZ; }
            set { _State_IsTXGZ = value; NotifyPropertyChanged(nameof(State_IsTXGZ)); }
        }

        private int _State_IsDJGZ;
        /// <summary>
        /// 电机状态 0正常 1电机故障
        /// </summary>
        public int State_IsDJGZ
        {
            get { return _State_IsDJGZ; }
            set { _State_IsDJGZ = value; NotifyPropertyChanged(nameof(State_IsDJGZ)); }
        }

        private int _State_IsJJTZ;
        /// <summary>
        /// 电机状态 0正常 1紧急停止
        /// </summary>
        public int State_IsJJTZ
        {
            get { return _State_IsJJTZ; }
            set { _State_IsJJTZ = value; NotifyPropertyChanged(nameof(State_IsJJTZ)); }
        }

        private int _State_IsDanger;
        /// <summary>
        /// 电机状态 0正常 1危险
        /// </summary>
        public int State_IsDanger
        {
            get { return _State_IsDanger; }
            set { _State_IsDanger = value; NotifyPropertyChanged(nameof(State_IsDanger)); }
        }

        private int _State_IsTXGZ_CJ;
        /// <summary>
        /// 采集模块通信故障 0正常 1危险
        /// </summary>
        public int State_IsTXGZ_CJ
        {
            get { return _State_IsTXGZ_CJ; }
            set { _State_IsTXGZ_CJ = value; NotifyPropertyChanged(nameof(State_IsTXGZ_CJ)); }
        }

        private float _Speed=1;
        /// <summary>
        /// 行走速度
        /// </summary>
        public float Speed
        {
            get { return _Speed; }
            set { _Speed = value; NotifyPropertyChanged(nameof(Speed)); }
        }

        /// <summary>
        /// 左脚
        /// </summary>
        public Foot LeftFoot { get; set; } = new Foot();

        /// <summary>
        /// 右脚
        /// </summary>
        public Foot RightFoot { get; set; } = new Foot();

        /// <summary>
        /// 重心坐标
        /// </summary>
        public FootPoint ZX { get; set; }=new FootPoint();

        /// <summary>
        /// 转化为字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ConvertToByteArray()
        {
            byte[] data = new byte[DataLength];
            data[0] = 0x55;
            data[1] = 0x0a;

            //转换状态
            byte state = 0;
            state = ByteHelper.SetBitValueToOne(state, 0, 1, (byte)State_IsRun);
            state = ByteHelper.SetBitValueToOne(state, 1, 1, (byte)State_IsTXGZ);
            state = ByteHelper.SetBitValueToOne(state, 2, 1, (byte)State_IsDJGZ);
            state = ByteHelper.SetBitValueToOne(state, 3, 1, (byte)State_IsJJTZ);
            state = ByteHelper.SetBitValueToOne(state, 4, 1, (byte)State_IsDanger);
            state = ByteHelper.SetBitValueToOne(state, 5, 1, (byte)State_IsTXGZ_CJ);
            data[2] = state;

            //转换速度
            //data[3]= ByteHelper.SetBitValueToOne(state, 7, 1, Speed>0?(byte)1: (byte)0);
            //TODO 临时
            ushort speed1 = (ushort)Math.Round(Speed * 10000);
            byte[] speed = BitConverter.GetBytes(speed1);
            data[3] = speed[1];
            data[4] = speed[0];

            //左右脚状态
            data[5] = ByteHelper.SetBitValueToOne(data[5], 5, 1, LeftFoot.IsGround == 0 ? (byte)1 : (byte)0);
            data[5] = ByteHelper.SetBitValueToOne(data[5], 4, 1, LeftFoot.IsGround == 1 ? (byte)1 : (byte)0);
            data[5] = ByteHelper.SetBitValueToOne(data[5], 1, 1, RightFoot.IsGround == 0 ? (byte)1 : (byte)0);
            data[5] = ByteHelper.SetBitValueToOne(data[5], 0, 1, RightFoot.IsGround == 1 ? (byte)1 : (byte)0);

            //坐标
            data[6] = (byte)LeftFoot.Point.X;
            data[7] = (byte)LeftFoot.Point.Y;

            data[8] = (byte)RightFoot.Point.X;
            data[9] = (byte)RightFoot.Point.Y;

            data[10] = (byte)ZX.X;
            data[11] = (byte)ZX.Y;

            #region CRC16校验

            byte[] crcData = new byte[DataLength - 2];
            Array.Copy(data, crcData, DataLength - 2);
            var crcResult = CRC.CRC16(crcData);
            data[DataLength-2] = crcResult[0];
            data[DataLength-1] = crcResult[1];

            #endregion

            return data;
        }

        /// <summary>
        /// 从字节数组解析
        /// </summary>
        /// <returns></returns>
        public void FromByteArray(byte[] data)
        {
            Time=DateTime.Now;
            State_IsRun = ByteHelper.GetBitValueFromOne(data[2], 0, 1) ;
            State_IsTXGZ = ByteHelper.GetBitValueFromOne(data[2], 1, 1) ;
            State_IsDJGZ = ByteHelper.GetBitValueFromOne(data[2], 2, 1);
            State_IsJJTZ = ByteHelper.GetBitValueFromOne(data[2], 3, 1) ;
            State_IsDanger = ByteHelper.GetBitValueFromOne(data[2], 4, 1);
            State_IsTXGZ_CJ = ByteHelper.GetBitValueFromOne(data[2], 5, 1);
            //老协议
            //int sign = ByteHelper.GetBitValueFromOne(data[3], 7, 1);
            //int exponent = ByteHelper.GetBitValueFromOne(data[3], 6, 5);
            //int fraction = ByteHelper.GetBitValueFromTwo(data[3], data[4], 9, 10);
            //Speed = (float)(Math.Pow(-1, sign) * Math.Pow(2, exponent - 15) * (1 + fraction / 1024));

            //解析速度 注意高低位
            byte[] speedArray  = new byte[2];
            speedArray[0] = data[4];
            speedArray[1] = data[3];
            Speed = BitConverter.ToUInt16(speedArray)/10000f;

            LeftFoot.IsGround = ByteHelper.GetBitValueFromOne(data[5], 4, 1) ;
  
            RightFoot.IsGround = ByteHelper.GetBitValueFromOne(data[5], 0, 1);

            LeftFoot.Point.X = data[6];
            LeftFoot.Point.Y = data[7];
            RightFoot.Point.X = data[8];
            RightFoot.Point.Y = data[9];
            ZX.X = data[10];
            ZX.X = data[11];
        }

        public override string ToString()
        {
            return $"------------------------\r\n〓〓1.电机状态：是否启动={State_IsRun} 通信故障={State_IsTXGZ} 电机故障={State_IsDJGZ} 紧急停止={State_IsJJTZ} 危险={State_IsDanger} \r\n"+
            $"〓〓2.速度：{Speed} \r\n〓〓3.左脚：{LeftFoot.ToString()}  右脚：{RightFoot.ToString()}  \r\n〓〓4.重心={ZX}\r\n";
        }

        /// <summary>
        /// 收到的时刻
        /// </summary>
        public DateTime Time { get; set; }
    }

    /// <summary>
    /// 脚
    /// </summary>
    public class Foot : NotifyPropertyClass
    {
        private int _IsGround;
        /// <summary>
        /// 是否落地
        /// </summary>
        public int IsGround
        {
            get { return _IsGround; }
            set { _IsGround = value;NotifyPropertyChanged("IsGround"); }
        }

        /// <summary>
        /// 坐标
        /// </summary>
        public FootPoint Point { get; set; }=new FootPoint();

        public override string ToString()
        {
            return $"【落地={IsGround} 坐标={Point}】";
        }

        public void Set(Foot foot)
        {
            IsGround = foot.IsGround;
            Point.X = foot.Point.X;
            Point.Y = foot.Point.Y;    
        }
        public void SetPoint(Foot foot)
        {
            Point.X = foot.Point.X;
            Point.Y = foot.Point.Y;
        }
    }

    /// <summary>
    /// 脚的坐标
    /// </summary>
    public class FootPoint : NotifyPropertyClass
    {
        private int _X;
        public int X 
        {
            get { return _X; }
            set {  _X=value; NotifyPropertyChanged("X"); }
        }

        private int _Y;
        public int Y
        {
            get { return _Y; }
            set { _Y = value; NotifyPropertyChanged("Y"); }
        }

        public override string ToString()
        {
            return $"({X.ToString("D2")},{Y.ToString("D3")})" ;
        }
    }

    public enum StepTypes
    {
        None=0,
        /// <summary>
        /// 左脚抬起
        /// </summary>
        LeftUp=1,
        /// <summary>
        /// 左脚放下
        /// </summary>
        LeftGround,
        /// <summary>
        /// 右脚抬起
        /// </summary>
        RightUp,
        /// <summary>
        /// 右脚放下
        /// </summary>
        RightGround
    }
}
