<template>
  <div class="nav flexR-center-lr">

    <!-- 当前时间 -->
    <div class="systemTime flexC-center-lr">
      <div class="f12 mt15">{{ time.Format('hh:mm:ss') }}</div>
      <div class="f12" style="margin-top: 8px;">{{ time.FormatNoTime() }}</div>
    </div>


    <!-- 设备状态 -->
    <div class="deviceStateClass flexR-center-lr mr40">
      <el-popover placement="top-start" title="设备状态信息" :width="400" trigger="hover">
        <template #reference>

          <div class="flexR-center-lr">
            <svg v-if="deviceState.IsNormal" class="myicon" style="font-size: 36px;" aria-hidden="true">
              <use xlink:href="#icon-shebei-zhengchang"></use>
            </svg>
            <svg v-if="deviceState.IsNormal == false" class="myicon" style="font-size: 36px;" aria-hidden="true">
              <use xlink:href="#icon-shebei"></use>
            </svg>
            <div v-if="deviceState.IsNormal" class="deviceState-normal ">设备正常</div>
            <div v-if="deviceState.IsNormal == false" class="deviceState-error ">设备异常</div>
          </div>

        </template>

        <!-- 已连接 -->
        <el-descriptions title="" :column="2" :border="true" :min-width="200" v-if="deviceState.COM != ''">
          <el-descriptions-item :width="100" label="通讯端口">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.COM != '' ? 'black' : 'red' }">{{ deviceState.COM == "" ? "未连接" :
              deviceState.COM }}</span>
          </el-descriptions-item>

          <el-descriptions-item label="PLC通讯">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.IsTXGZ_PLC == 0 ? 'black' : 'red' }">{{ deviceState.IsTXGZ_PLC == 0 ? "正常"
              : "故障"
            }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="电机">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.IsDJGZ == 0 ? 'black' : 'red' }">{{ deviceState.IsDJGZ == 0 ? "正常" :
              "故障"
            }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="采集通讯">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.IsTXGZ_CJ == 0 ? 'black' : 'red' }">{{ deviceState.IsTXGZ_CJ == 0 ? "正常" :
              "故障"
            }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="紧急停止">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.IsJJTZ == 0 ? 'black' : 'red' }">{{ deviceState.IsJJTZ == 0 ? "否" : "是"
            }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="发生危险">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.IsDanger == 0 ? 'black' : 'red' }">{{ deviceState.IsDanger == 0 ? "否" :
              "是"
            }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="启动">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.IsRun == 0 ? 'black' : 'black' }">{{ deviceState.IsRun == 0 ? "已启动" : "未启动"
            }}</span>
          </el-descriptions-item>
        </el-descriptions>

        <!-- 未连接 -->
        <el-descriptions title="" :column="2" :border="true" v-if="deviceState.COM == ''">
          <el-descriptions-item label="通讯端口">
            <span class="deviceStateClass-itemContent" :style="{ color: deviceState.COM != '' ? 'black' : 'red' }">{{ deviceState.COM == "" ? "未连接" :
              deviceState.COM }}</span>
          </el-descriptions-item>

          <el-descriptions-item label="PLC通讯" >
            <span class="deviceStateClass-itemContent"></span>
          </el-descriptions-item>
          <el-descriptions-item label="电机" >
            <span class="deviceStateClass-itemContent"></span>
          </el-descriptions-item>
          <el-descriptions-item label="采集通讯" >
            <span class="deviceStateClass-itemContent"></span>
          </el-descriptions-item>
          <el-descriptions-item label="紧急停止">
            <span class="deviceStateClass-itemContent"></span>
          </el-descriptions-item>
          <el-descriptions-item label="发生危险">
            <span class="deviceStateClass-itemContent"></span>
          </el-descriptions-item>
          <el-descriptions-item label="启动">
            <span class="deviceStateClass-itemContent"></span>
          </el-descriptions-item>
        </el-descriptions>

      </el-popover>
    </div>

    <!-- 当前用户 -->
    <div id="currentPatientPanel" class="flexR-center-lr currentPatient mr40">
      <el-popover placement="top-start" title="当前用户信息" :width="pinfo == null?0:480" trigger="hover">
        <template #reference>
          <div class="flexR-center-lr">
            <el-avatar v-if="pinfo != null && pinfo.Sex == 1" src="./resource/img/women.png" />
            <el-avatar v-else src="./resource/img/men.png" />
            <div class="ml10">{{ pinfo == null ? ' 请先登录 ' : pinfo.Name }}</div>
          </div>
        </template>

        <div v-if="pinfo == null">请先登录</div>
        <el-descriptions v-if="pinfo != null" title="" :column="1" :border="true">
          <el-descriptions-item :min-width="30" label="编号">
            <div style="width: 300px;">{{ pinfo.SN }}</div>
          </el-descriptions-item>
          <el-descriptions-item :min-width="30" label="病症">
            <div>{{ pinfo.DiseaseType }}</div>
          </el-descriptions-item>
          <el-descriptions-item :min-width="30" label="金币数">
            <div>{{ pinfo.Gold }}</div>
          </el-descriptions-item>
          <el-descriptions-item :min-width="30" label="最新步态记录">
            <div style="white-space: pre;">{{ getDataResult(pinfo.Data) }}</div>
          </el-descriptions-item>
        </el-descriptions>
      </el-popover>

    </div>

    <!-- 导航 需要登录才显示-->
    <router-link class="nav-item flexC-center-lr" :ref="item.ref" :to="item.page" @click="item_click"
      v-for="item in items" :key="item.id" :data-id="item.id" replace v-show="pinfo != null">

      <svg v-if="item.id != 0" class="nav-item-icon" aria-hidden="true">
        <use :xlink:href="item.id == currentID ? item.icon_on : item.icon_off"></use>
      </svg>

      <div v-if="item.id != 0" class="nav-item-name">{{ item.name }}
      </div>
    </router-link>

    <!-- 导航2 不登录也显示-->
    <router-link class="nav-item flexC-center-lr" :ref="item.ref" :to="item.page" @click="item_click"
      v-for="item in items2" :key="item.id" :data-id="item.id" replace>

      <svg v-if="item.id != 0" class="nav-item-icon" aria-hidden="true">
        <use :xlink:href="item.id == currentID ? item.icon_on : item.icon_off"></use>
      </svg>

      <div v-if="item.id != 0" class="nav-item-name">{{ item.name }}
      </div>

    </router-link>

    <!-- 退出 -->
    <div class="nav-item flexC-center-lr" @click="exit_click">

      <svg class="nav-item-icon" aria-hidden="true">
        <use :xlink:href="currentID == 7 ? '#icon-tuichu' : '#icon-tuichu'"></use>
      </svg>
      <div class="nav-item-name">退出</div>
    </div>
  </div>
</template>

<script>
// const { KTimer } = require('../object/KTimer');

// debugger.
module.exports = {
  data: function () {
    return {
      items: [
        {
          id: 2,
          name: "主页",
          icon_on: '#icon-shouye-dianji',
          icon_off: '#icon-shouye',
          icon_error: 'icon-ts',
          image: "",
          selectImage: "",
          page: '/home',
          ref: "page2"
        },
        {
          id: 3,
          name: "记录",
          icon_on: '#icon-jilu--dianji',
          icon_off: '#icon-jilu',
          image: "",
          selectImage: "",
          page: '/history',
          ref: "page3"
        },

      ],
      items2: [
        {
          id: 1,
          name: "用户",
          icon_on: '#icon-yonghu-dianji',
          icon_off: '#icon-yonghu',
          icon_error: 'icon-ts',
          image: "",
          selectImage: "",
          page: '/',
          ref: "page1"
        },
        {
          id: 4,
          name: "设置",
          icon_on: '#icon-shezhi-dianji',
          icon_off: '#icon-shezhi',
          image: "",
          selectImage: "",
          page: '/setting',
          ref: "page4"
        },

      ],
      currentID: 1,
      time: new Date(),
      deviceState:
      {
        IsNormal: false,
        COM: "",
        IsRun: 1,
        IsTXGZ_PLC: 1,
        IsDJGZ: 1,
        IsJJTZ: 1,
        IsDanger: 1,
        IsTXGZ_CJ: 1
      }
    };
  },
  props: ['pinfo', 'devstate', 'admin'],
  watch: {
    pinfo: function (nnn, old) {
      console.log("pinfo  " + nnn);
    }
  },
  methods: {
    //解析用户评估数据
    getDataResult: function (data) {
      var result = "";
      try {
        if (data.length > 0) {
          data = JSON.parse(data);
          if (data.speed!=undefined) {
            result += "步速:" + data.speed.toFixed(2) + "米/秒"
          }
          if (data.rhythm!=undefined) {
            result += "   步频:" + data.rhythm + "次/分"
          }
          if (data.steplenght!=undefined) {
            result += "   步长:" + data.steplenght + "厘米"
          }
          if (data.symm!=undefined) {
            result += "   对称性:" + data.symm + "%"
          }
          return result;
        }
      } catch (error) {

      }
      return result;
    },
    item_click: function (e) {
      // debugger;
      if (e.target.dataset.id != 0) {
        console.log('切换主导航：' + e.target.dataset.id);
        this.currentID = e.target.dataset.id;
        CSharp_App.KeyBoard_Hide();
        //停止MIDI试听
        CSharp_DB.Music_StopTest().then(res => {
          res = JSON.parse(res);
          if (res.success) {

          } else {

          }
        });

      }

    },
    exit_click: function (e) {
      this.$confirm(
        '确认退出系统吗?',
        '操作提示',
        {
          confirmButtonText: '确认',
          cancelButtonText: '取消',
          type: 'warning',
        }
      )
        .then(() => {
          this.$loading({
            body:true,
            text:"正在退出系统..."
          })
          CSharp_App.Exit();
        })
        .catch(() => {

        })
    },
  },
  mounted: function () {
    var self = this;
    this.timer = new window.KTimer();
    this.timer.tick = function () {
      self.time = new Date();
      if (window.DeviceManager.deviceState) {
        self.deviceState = window.DeviceManager.deviceState;
      }
    }
    this.timer.start(500);

  },
};
</script>

<style>
.deviceStateClass-itemContent{
  display: block;
  width: 80px;
}
.systemTime {
  position: absolute;
  width: 100px;
  height: 100%;
  left: 0;
  bottom: 0;
  background-color: #ffffff38;
  color: white;
}

.deviceStateClass {
  position: absolute;
  width: 150px;
  height: 100%;
  left: 200px;
  bottom: 0;
}

.deviceState-normal {
  color: white;
  margin-left: 5px;
}

.deviceState-error {
  color: #FF8F0B;
  margin-left: 5px;
}

.currentPatient {
  position: absolute;
  left: 1320px;
  color: aliceblue;
  height: 100%;
  color: #eee;
  justify-content: center;
  /* margin-right: 100px; */
  font-size: 16px;
}

.nav {
  /* width: 100%; */
  height: 70px;

  /* justify-content: flex-end; */
}

.nav-item {
  position: relative;
  height: 60px;
  /* min-width: 60px; */
  width: 60px;
  margin-right: 10px;
  margin-left: 10px;
  text-align: center;
}

.nav-item-current {
  position: relative;
  height: 60px;
  min-width: 60px;
  margin-right: 10px;
  margin-left: 10px;
  text-align: center;
}

.nav-item-icon {
  margin-top: 6px;
  width: 30px;
  width: 30px;
  pointer-events: none;
}

.nav-item-name {
  line-height: 26px;
  pointer-events: none;
  color: #a2a3a5;
}



a[class='router-link-active router-link-exact-active nav-item flexC-center-lr'] {
  background-color: #1252f6;
  border-radius: 30px;
}

a[class='router-link-active router-link-exact-active nav-item flexC-center-lr'] div {
  color: white;
}
</style>
