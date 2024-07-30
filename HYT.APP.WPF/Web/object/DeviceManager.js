/**
* 设备管理
*/
export class DeviceManager {
  constructor() {
    this.deviceState = {
      IsNormal: false,
      COM: "",
      IsRun: 1,
      IsTXGZ_PLC: 1,
      IsDJGZ: 1,
      IsJJTZ: 1,
      IsDanger: 1,
      IsTXGZ_CJ: 1
    };//
    this.deviceData = null;//设备数据
  }
  receiveData(data) {
    // console.log(data)
    var msg = JSON.parse(data);
    if (msg.state) {
      this.deviceState = msg.state;
    }
    if (msg.data) {
      this.deviceData = msg.data;//通信状态
    }
  }
}