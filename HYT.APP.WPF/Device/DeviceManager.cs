using CL.Common;
using CL.Common.Data;
using HYT.APP.WPF.Device;
using KCL;
using KT.TCP;
using KT.TCP.Train;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Xml.Linq;

namespace KCL
{
    /// <summary>
    /// 设备管理器
    /// 接收处理串口的数据
    /// </summary>
    public class DeviceManager
    {
        #region ■------------------ 单例

        private DeviceManager()
        {

        }
        public static readonly DeviceManager Instance = new DeviceManager();

        #endregion

        #region ■------------------ 字段相关

        #endregion

        /// <summary>
        /// 启动设备管理器
        /// </summary>
        public void Start()
        {

            //开始串口通信
            SPDevice_ZBJ.Instance.OnDataReceiveEvent -= Instance_OnDataReceiveEvent;
            SPDevice_ZBJ.Instance.OnDataReceiveEvent += Instance_OnDataReceiveEvent; ;
            SPDevice_ZBJ.Instance.OnConnect += (sender, com) =>
            {
                App_JSAPI.Instance.Notification("设备连接成功", "success", 1000);
                SendDeviceStateToWeb();

                TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_DeviceState)
                {
                    D2 = SPDevice_ZBJ.Instance.IsCommunication ? 1 : 0,
                });
            };
            SPDevice_ZBJ.Instance.OnDisconnect += (sender, com) =>
            {
                App_JSAPI.Instance.Notification("设备断开连接", "error", 1000);
                SendDeviceStateToWeb();

                TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_DeviceState)
                {
                    D2 = SPDevice_ZBJ.Instance.IsCommunication ? 1 : 0,
                });
            };

            SPDevice_ZBJ.Instance.Start(ConfigManager.Instance.SSetting.BaudRate);
        }

        #region ■------------------ 收到数据 发送控制信号给训练

        /// <summary>
        /// 上一次接收到的数据
        /// </summary>
        public DeviceData LastDeviceData;

        /// <summary>
        /// 是否紧急停止
        /// </summary>
        public bool IsJJTZ
        {
            get
            {
                if (IsCommunication && LastDeviceData != null && LastDeviceData.State_IsJJTZ == 1)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 上一次训练数据
        /// </summary>
        GaitRecordUnity LastGaitRecord;

        /// <summary>
        /// 数据解析 发送控制信号给训练或评测 记录并分析数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_OnDataReceiveEvent(object? sender, DeviceDataEventArgs e)
        {
            try
            {
                DeviceData newData = new DeviceData();
                newData.FromByteArray(e.Data);

                DataAnalysisDone?.Invoke(null, newData);

                SendDeviceStateToWeb();

                if (LastDeviceData != null)
                {
                    newData.Index = LastDeviceData.Index + 1;
                }

                //发送最新数据给训练
                if (LastDeviceData == null || LastDeviceData.LeftFoot.IsGround != newData.LeftFoot.IsGround || LastDeviceData.RightFoot.IsGround != newData.RightFoot.IsGround)
                {
                    GaitRecordUnity gaitRecordUnity = new GaitRecordUnity(DeviceDataAnalysisManager.Instance.CurrentGaitRecord);

                    //if (LastGaitRecord == null || LastGaitRecord.StepCount != gaitRecordUnity.StepCount)
                    //{
                    LastGaitRecord = gaitRecordUnity;
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_DeviceData)
                    {
                        Data = JsonConvert.SerializeObject(gaitRecordUnity)
                    });
                    //}
                }

                //发送控制信号给训练
                //左脚状态变更
                if (LastDeviceData == null || LastDeviceData.LeftFoot.IsGround != newData.LeftFoot.IsGround)
                {
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_Controll)
                    {
                        Data = "左脚状态变更",
                        D1 = newData.LeftFoot.IsGround,
                        //D3 = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Rhythm_Average,
                        //D4 = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Symm_Average,

                    });
                    SendDataToWeb(newData);
                    if (LastDeviceData != null)
                    {
                        newData.StepType = (StepTypes)(newData.LeftFoot.IsGround == 1 ? 2 : 1);
                        DeviceDataAnalysisManager.Instance.AddRecord(newData);
                    }
                }
                //右脚状态变更
                if (LastDeviceData == null || LastDeviceData.RightFoot.IsGround != newData.RightFoot.IsGround)
                {
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_Controll)
                    {
                        Data = "右脚状态变更",
                        D2 = newData.RightFoot.IsGround,
                        //D3 = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Rhythm_Average,
                        //D4 = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Symm_Average,
                    });
                    SendDataToWeb(newData);
                    if (LastDeviceData != null)
                    {
                        newData.StepType = (StepTypes)(newData.RightFoot.IsGround == 1 ? 4 : 3);
                        DeviceDataAnalysisManager.Instance.AddRecord(newData);
                    }
                }
                //速度变更
                if (LastDeviceData == null || LastDeviceData.Speed != newData.Speed)
                {
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_Controll)
                    {
                        Data = "速度变更",
                        D3 = newData.Speed
                    });
                    SendDataToWeb(newData);
                }
                //危险状态变更
                if (LastDeviceData == null || LastDeviceData.State_IsDanger != newData.State_IsDanger)
                {
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_Controll)
                    {
                        Data = "危险状态变更",
                        D4 = newData.State_IsDanger,
                    });
                    SendDataToWeb(newData);
                }
                //设备故障变更
                if (LastDeviceData == null || LastDeviceData.State != newData.State)
                {
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_Controll)
                    {
                        Data = "设备状态变更" + newData.State_IsRun.ToString() + newData.State_IsTXGZ.ToString() + newData.State_IsDJGZ.ToString() + newData.State_IsJJTZ.ToString() + newData.State_IsTXGZ_CJ.ToString()
                    });
                    SendDataToWeb(newData);

                    //紧急停止功能
                    if (newData.State_IsJJTZ == 1)
                    {

                    }

                }

                LastDeviceData = newData;

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        ///数据分析完毕
        /// </summary>
        public event EventHandler<DeviceData> DataAnalysisDone;

        #endregion

        #region ■------------------ 发送数据到Web


        /// <summary>
        /// 发送设备状态给Web
        /// </summary>
        /// <returns></returns>
        public void SendDeviceStateToWeb()
        {
            try
            {
                DeviceState deviceState = new DeviceState();
                deviceState.COM = SPDevice_ZBJ.Instance.IsCommunication ? SPDevice_ZBJ.Instance.COM : "";
                if (SPDevice_ZBJ.Instance.IsCommunication)
                {
                    if (DeviceManager.Instance.LastDeviceData != null)
                    {
                        deviceState.IsRun = Instance.LastDeviceData.State_IsRun;
                        deviceState.IsTXGZ_PLC = DeviceManager.Instance.LastDeviceData.State_IsTXGZ;
                        deviceState.IsDJGZ = DeviceManager.Instance.LastDeviceData.State_IsDJGZ;
                        deviceState.IsJJTZ = DeviceManager.Instance.LastDeviceData.State_IsJJTZ;
                        deviceState.IsDanger = DeviceManager.Instance.LastDeviceData.State_IsDanger;
                        deviceState.IsTXGZ_CJ = DeviceManager.Instance.LastDeviceData.State_IsTXGZ_CJ;
                    }

                }
                else
                {
                    deviceState.IsRun = 1;
                    deviceState.IsTXGZ_PLC = 1;
                    deviceState.IsDJGZ = 1;
                    deviceState.IsJJTZ = 1;
                    deviceState.IsDanger = 1;
                    deviceState.IsTXGZ_CJ = 1;
                }

                object msg = new { state = deviceState };
                //发送到浏览器
                string device_PData = JsonConvert.SerializeObject(msg);
                string script = "window.DeviceManager.receiveData('" + device_PData + "')";
                script = "if(window.DeviceManager&&window.DeviceManager.receiveData){" + script + "}";
                BrowserManager.Instance.ExecuteJSAsync(script);

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);

            }
        }

        /// <summary>
        /// COM口是否通信
        /// </summary>
        public bool IsCommunication
        {
            get { return SPDevice_ZBJ.Instance.IsCommunication; }
        }

        /// <summary>
        /// 发送设备数据给Web
        /// </summary>
        public void SendDataToWeb(DeviceData deviceData)
        {
            try
            {
                if (deviceData != null)
                {
                    DeviceState deviceState = new DeviceState();
                    deviceState.COM = SPDevice_ZBJ.Instance.IsCommunication ? SPDevice_ZBJ.Instance.COM : "";
                    if (SPDevice_ZBJ.Instance.IsCommunication)
                    {
                        deviceState.IsRun = deviceData.State_IsRun;
                        deviceState.IsTXGZ_PLC = deviceData.State_IsTXGZ;
                        deviceState.IsDJGZ = deviceData.State_IsDJGZ;
                        deviceState.IsJJTZ = deviceData.State_IsJJTZ;
                        deviceState.IsDanger = deviceData.State_IsDanger;
                        deviceState.IsTXGZ_CJ = deviceData.State_IsTXGZ_CJ;
                    }
                    else
                    {
                        deviceState.IsRun = 1;
                        deviceState.IsTXGZ_PLC = 1;
                        deviceState.IsDJGZ = 1;
                        deviceState.IsJJTZ = 1;
                        deviceState.IsDanger = 1;
                        deviceState.IsTXGZ_CJ = 1;
                    }

                    object msg = new { state = deviceState, data = LastDeviceData };
                    //发送到浏览器
                    string device_PData = JsonConvert.SerializeObject(msg);
                    string script = "window.DeviceManager.receiveData('" + device_PData + "')";
                    script = "if(window.DeviceManager&&window.DeviceManager.receiveData){" + script + "}";
                    BrowserManager.Instance.ExecuteJSAsync(script);
                }
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }

        }

        #endregion

        #region ■------------------ 发送控制信号给设备

        /// <summary>
        /// 上位机发送控制指令：启动电机并设置速度  0到6.5
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(float speed)
        {
            if (speed > 6.5) speed = 6.5f;
            if (speed < 0) speed = 0f;

            byte[] data = new byte[7];
            data[0] = 0x55;
            data[1] = 0x0a;

            //电机启动/停止命令状态字
            data[2] = 0;//0启动 1停止

            //行走速度，注意高低位 0.5*10000=5000=0x1388  speedData={0x88 0x13}
            ushort speed1 = (ushort)Math.Round(speed * 10000);
            byte[] speedData = BitConverter.GetBytes(speed1);
            data[3] = speedData[1];
            data[4] = speedData[0];

            #region CRC16校验

            byte[] crcData = new byte[data.Length - 2];
            Array.Copy(data, crcData, data.Length - 2);
            var crcResult = CRC.CRC16(crcData);
            data[data.Length - 2] = crcResult[0];
            data[data.Length - 1] = crcResult[1];

            #endregion

            SPDevice_ZBJ.Instance.SendData(data);

        }

        /// <summary>
        /// 上位机发送控制指令：停止电机
        /// </summary>
        public void Stop()
        {
            byte[] data = new byte[7];
            data[0] = 0x55;
            data[1] = 0x0a;

            //电机启动/停止命令状态字
            data[2] = 1;//0启动 1停止

            //            #region ■------------------ 测试代码

            //            if (ConfigManager.Instance.SSetting.IsDebug)
            //            {
            //#if DEBUG
            //                data[2] = 0;//0启动 1停止  暂时 TODO
            //#endif
            //            }

            //            #endregion


            //行走速度
            data[3] = 0;
            data[4] = 0;

            #region CRC16校验

            byte[] crcData = new byte[data.Length - 2];
            Array.Copy(data, crcData, data.Length - 2);
            var crcResult = CRC.CRC16(crcData);
            data[data.Length - 2] = crcResult[0];
            data[data.Length - 1] = crcResult[1];

            #endregion

            SPDevice_ZBJ.Instance.SendData(data);

        }

        #endregion

    }

}
