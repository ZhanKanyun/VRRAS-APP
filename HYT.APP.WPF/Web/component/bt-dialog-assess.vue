<template>
  <div id="assesspage" class="page flexC-center-lr assesspage">

    <div class="page-head flexR-center-lr" :class="assessState == 'Finish' ? '' : 'nobg'">
      <i class="icon-dw ml40"></i>
      <div class="ml10">评估页面</div>

      <el-button v-if='assessState == "Pause" || assessState == "Init"' :disabled="isFirstAssess" class="exitBtn"
        @click="btn_Close_click">
        <svg class="thisicon mr10" aria-hidden="true">
          <use xlink:href="#icon-tuichu1"></use>
        </svg>
        退出
      </el-button>
    </div>

    <div class="RunningBox" v-show='assessState != "Finish"'>

      <div class="topBox">
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-pingjunsudu-hui' : '#icon-pingjunsudu'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else>{{ Math.round(currentGaitRecord.Speed_Average * 100) / 100 }}米/秒</div>
          <div class="datatil">平均速度</div>
        </div>
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-pingjunbupin-hui' : '#icon-pingjunbupin'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else>{{ Math.round(currentGaitRecord.Rhythm_Average * 100) / 100 }}次/分钟</div>
          <div class="datatil">平均步频</div>
        </div>
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-duichengxing-hui' : '#icon-duichengxing'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else> {{ Math.round(currentGaitRecord.Symm_Average * 100) / 100 }}%</div>
          <div class="datatil">平均对称性</div>
        </div>
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-pingjunbushi-hui' : '#icon-pingjunbushi'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else> {{ Math.round(currentGaitRecord.StepTime_Average * 100) / 100 }}毫秒</div>
          <div class="datatil">平均步时</div>
        </div>
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-pingjunbukuan-hui' : '#icon-pingjunbukuan'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else>{{ Math.round(currentGaitRecord.StepWidth_Average * 100) / 100 }}厘米</div>
          <div class="datatil">平均步宽</div>
        </div>
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-pingjunkuabuchang-hui' : '#icon-pingjunkuabuchang'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else>{{ Math.round(currentGaitRecord.BF_Average * 100) / 100 }}厘米</div>
          <div class="datatil">平均跨步长</div>
        </div>
        <div class="dataBox">
          <svg class="thisicon" aria-hidden="true">
            <use :xlink:href="assessState == 'Init' ? '#icon-pingjunbuchang-hui' : '#icon-pingjunbuchang'"></use>
          </svg>
          <div class="datadata" v-if="currentGaitRecord == null">---</div>
          <div class="datadata" v-else> {{ Math.round(currentGaitRecord.StepLength_Average * 100) / 100 }}厘米</div>
          <div class="datatil">平均步长</div>
        </div>

      </div>

      <div class="middleBox">
        <div class="middleContent">
          <!-- 速度控制 -->
          <div class="speedBox">
            <el-button :disabled="set_BuSu <= minBuSu" :class="set_BuSu <= minBuSu ? 'noclick' : 'okclick'"
              class="minusSpeed" @click="btn_minusSpeed"></el-button>

            <div class="SpeedValue">
              <div class="val">{{ set_BuSu.toFixed(2) }}</div>
              <div class="danwei">米/秒</div>
            </div>

            <el-button :disabled="set_BuSu >= maxBuSu" :class="set_BuSu >= maxBuSu ? 'noclick' : 'okclick'"
              class="addSpeed" @click="btn_addSpeed"></el-button>
          </div>

          <!-- 提示文字 -->
          <div v-show='assessState == "Running"' class="middleTxt" style="color: #67C23A;">剩余评估时间：<span
              ref="useTimeTxt">0</span>秒</div>

          <div v-show='assessState == "Init" && set_BuSu <= minBuSu' class="middleTxt">请先调整速度，启动测步台</div>
          <div v-show='assessState == "Init" && set_BuSu > minBuSu' class="middleTxt">将速度调整至最舒适状态后，点击开始评估</div>
          <div v-show='assessState == "Init" && isFirstAssess' class="middleTxt" style="top: 325px;">
            初次使用没有评估数据的用户，不可以跳过本次评估</div>
          <div v-show='assessState == "Pause" && isFirstAssess' class="middleTxt">初次使用没有评估数据的用户，不可以跳过本次评估</div>
        </div>
      </div>

      <div class="bottomBox">
        <div class="dataBoxBox">
          <div class="dataBox">
            <div class="datatil">总步数</div>
            <div class="datadata" v-if="currentGaitRecord == null">---</div>
            <div class="datadata" v-else> {{ Math.round(currentGaitRecord.StepCount * 100) / 100 }}</div>
          </div>

          <div class="dataBox">
            <div class="datatil">平均步态周期</div>
            <div class="datadata" v-if="currentGaitRecord == null">---</div>
            <div class="datadata" v-else> {{ Math.round(currentGaitRecord.T_Average * 100) / 100 }}秒</div>
          </div>
        </div>
        <!-- 按钮控制 -->
        <el-button v-show='assessState == "Init"' :disabled="set_BuSu <= minBuSu" class="bottombtn"
          @click="btn_start_click">
          <img v-if="set_BuSu <= minBuSu" src="resource/img/png/开始评估-禁用.svg" style="width: 100%;height: 100%;">
          <img v-else src="resource/img/png/开始评估-激活.svg" style="width: 100%;height: 100%;">
        </el-button>
        <el-button v-show='assessState == "Running"' class="bottombtn" @click="btn_pause_click">
          <img src="resource/img/png/暂停评估.svg" style="width: 100%;height: 100%;">
        </el-button>
        <el-button v-show='assessState == "Pause"' class="bottombtn" @click="btn_continue_click">
          <img src="resource/img/png/继续评估.svg" style="width: 100%;height: 100%;">
        </el-button>

        <div class="dataBoxBox">
          <div class="dataBox">
            <div class="datatil">左脚平均步长</div>
            <div class="datadata" v-if="currentGaitRecord == null">---</div>
            <div class="datadata" v-else> {{ Math.round(currentGaitRecord.StepLengthL_Average * 100) / 100 }}厘米</div>
          </div>

          <div class="dataBox">
            <div class="datatil">右脚平均步长</div>
            <div class="datadata" v-if="currentGaitRecord == null">---</div>
            <div class="datadata" v-else> {{ Math.round(currentGaitRecord.StepLengthR_Average * 100) / 100 }}厘米</div>
          </div>
        </div>
      </div>

    </div>

    <!-- 结束  评估报告显示-->
    <div class="FinishBox" v-show='assessState == "Finish"'>

      <div class="header mt20">评估数据</div>
      <div style="width: 1040px;" v-if="dataResult != null">
        <el-descriptions :column="4" border>
          <el-descriptions-item :width="130" label="总步数">
            {{ Math.round(JSON.parse(dataResult.Result).StepCount * 100) / 100 }}
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均步态周期">
            {{ Math.round(JSON.parse(dataResult.Result).T_Average * 100) / 100 }}秒
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均速度">
            {{ Math.round(JSON.parse(dataResult.Result).Speed_Average * 100) / 100 }}米/秒
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均步频">
            {{ Math.round(JSON.parse(dataResult.Result).Rhythm_Average * 100) / 100 }}次/分钟
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均对称性">
            {{ Math.round(JSON.parse(dataResult.Result).Symm_Average * 100) / 100 }}%
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均步时">
            {{ Math.round(JSON.parse(dataResult.Result).StepTime_Average * 100) / 100 }}毫秒
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均步宽">
            {{ Math.round(JSON.parse(dataResult.Result).StepWidth_Average * 100) / 100 }}厘米
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均跨步长">
            {{ Math.round(JSON.parse(dataResult.Result).BF_Average * 100) / 100 }}厘米
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="平均步长">
            {{ Math.round(JSON.parse(dataResult.Result).StepLength_Average * 100) / 100 }}厘米
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="左脚平均步长">
            {{ Math.round(JSON.parse(dataResult.Result).StepLengthL_Average * 100) / 100 }}厘米
          </el-descriptions-item>
          <el-descriptions-item :width="130" label="右脚平均步长">
            {{ Math.round(JSON.parse(dataResult.Result).StepLengthR_Average * 100) / 100 }}厘米
          </el-descriptions-item>
        </el-descriptions>
      </div>

      <div v-show="dataResult != null" style="width: 1550px;">
        <v-chart style="width: 1550px;height:700px;" ref="zheXianResult" />
      </div>
      <div class="btnBox">
        <el-button class="btn" @click="btn_goTrain_click">以该结果开始训练</el-button>
        <el-button class="btn" @click="btn_save_click">保存</el-button>
        <el-button class="btn" @click="btn_Close_click">返回</el-button>
      </div>
    </div>

  </div>
</template>

<script>
module.exports = {
  data() {
    return {
      stepspeed: 0.05,//速度更改值
      minBuSu: 0,
      maxBuSu: 6.5,
      assessTime: 60,//需要评估时间
      assessConfig: null,

      set_BuSu: 0,
      isFirstAssess: true,//是否是用户第一次评估
      assessState: "None",// None, Init 调试速度中, Running 测试中,暂停中 Pause, 结束 Finish 

      currentTime: 0,//已经评估时间
      startTime: 0,
      lastDate: 0,

      m_useTimer: null, //计时器--评估用时
      m_dataTimer: null,//计时器--数据采集

      currentGaitRecord: null,//实时数据

      dataResult: null,//评估数据
      patientNewData: null,//用户数据
    };
  },
  props: ["assessid"],
  watch: {

  },
  methods: {
    // 初始化
    InitFun: function () {
      if (this.$root.currentPatient.Data == null) {
        this.isFirstAssess = true;
      } else {
        this.isFirstAssess = false;
      }

      this.assessConfig = window.aa_localDB.getAssessByid(this.assessid);
      this.maxBuSu = this.assessConfig.maxBuSu;
      this.assessTime = this.assessConfig.assessTime;
      this.minBuSu = 0;

      this.set_BuSu = 0;
      this.currentGaitRecord = null;

      this.assessState = "Init";
      this.currentTime = 0;
      this.$refs.useTimeTxt.innerHTML = this.assessTime;

      this.patientNewData = null;
      this.dataResult = null;

      this.startUseTimer();
    },

    btn_minusSpeed: function () {
      var thisval = this.set_BuSu * 100 - this.stepspeed * 100;
      thisval = Math.round(thisval) / 100;
      if (thisval <= this.minBuSu) {
        thisval = this.minBuSu;
      }
      if (this.assessState != "Init" && thisval <= this.stepspeed) {
        thisval = this.stepspeed;
      }

      this.set_BuSuChange(thisval, this.set_BuSu);
      this.set_BuSu = thisval;
    },
    btn_addSpeed: function () {
      var thisval = this.set_BuSu * 100 + this.stepspeed * 100;
      thisval = Math.round(thisval) / 100;

      if (thisval > this.maxBuSu) {
        thisval = this.maxBuSu;
      }
      this.set_BuSuChange(thisval, this.set_BuSu);
      this.set_BuSu = thisval;
    },
    set_BuSuChange: function (val, old) {
      // 步速变化，时间重新开始
      if (val != old) {
        this.currentTime = 0;
        this.$refs.useTimeTxt.innerHTML = this.assessTime;
        this.currentGaitRecord = null;

        this.lastDate = new Date();

        CSharp_App.DeviceSetSpeed(val);

        if (this.assessState == "Running") {
          var self = this;
          CSharp_Assess.StartRecord().then(res => {
            res = JSON.parse(res);
            if (res.success) {
              this.lastDate = new Date();
              this.currentTime = 0;
            } else {
              self.$message.error("启动失败");
            }
          });
        }
      }
    },
    // 开始评估
    btn_start_click: function () {
      if (this.assessState != "Init") return;
      var self = this;

      CSharp_Assess.StartRecord().then(res => {
        res = JSON.parse(res);
        if (res.success) {
          this.assessState = "Running";
          this.minBuSu = this.stepspeed;
          this.currentTime = 0;
          this.startTime = new Date();
          this.lastDate = new Date();
        } else {
          self.$message.error("启动失败");
        }
      });
    },
    // 暂停
    btn_pause_click: function () {
      if (this.assessState != "Running") return;
      this.assessState = "Pause";

      CSharp_Assess.PauseRecord();
    },
    // 继续
    btn_continue_click: function () {
      if (this.assessState != "Pause") return;
      this.assessState = "Running";
      this.lastDate = new Date();
      CSharp_Assess.ContinueRecord();

    },

    // 退出评估
    btn_Close_click: function () {
      this.assessState = "None";
      this.clearAllTimer();
      this.$emit("close", false);
      CSharp_Assess.StopRecord();
      CSharp_App.DeviceStop();
    },
    // 评估结束
    finishFun: function () {
      this.assessState = "Finish";
      this.clearAllTimer();
      let self = this;

      CSharp_Assess.StopRecord().then(res => {
        res = JSON.parse(res);
        if (res.success) {

          // 评估数据

          this.dataResult = {
            PatientID: this.$root.currentPatient.ID,
            AssessID: this.assessid,
            PingCeName: this.assessConfig.name,
            StartTime: this.startTime,
            EndTime: new Date(),
            Result: JSON.stringify(res.data),
          };

          var obj = res.data;
          //用户数据更新
          this.patientNewData = {
            symm: obj.Symm_Average,
            speed: obj.Speed_Average,
            steplenght: obj.StepLength_Average,
            rhythm: obj.Rhythm_Average
          }

          this.finishEcharts();
        } else {
          self.$message.error(res.message);
        }
      });

      CSharp_App.DeviceStop();

    },
    // 评估结束图表
    finishEcharts: function () {
      var result = JSON.parse(this.dataResult.Result);

      var xData = [];
      var bc = [];
      var bs = [];
      var bp = [];
      var dcx = [];

      for (let index = 0; index < result.Steps.length; index++) {
        const element = result.Steps[index];
        xData.push(index + 1);
        bc.push(element.BC);
        bs.push(element.BS);
        bp.push(element.BP);
        dcx.push(element.DCX);
      }

      const zhexian = this.$refs.zheXianResult;
      if (zhexian) {
        var option = {
          tooltip: {
            trigger: "axis",
          },
          title: [
            { left: "23%", top: "2%", text: "步长变化" },
            { left: "73%", top: "2%", text: "步时变化" },
            { left: "22%", top: "50%", text: "步频变化" },
            { left: "72%", top: "50%", text: "对称性变化" }
          ],
          grid: [
            { left: "7%", top: "10%", width: "40%", height: "37%" },
            { left: "55%", top: "10%", width: "40%", height: "37%" },
            { left: "7%", top: "58%", width: "40%", height: "37%" },
            { left: "55%", top: "58%", width: "40%", height: "37%" },
          ],
          xAxis:
            [{
              name: '步',
              gridIndex: 0,
              data: xData,
              boundaryGap: false,
              axisLabel: {
                // interval: 4
              },
              splitLine: {
                show: false
              },
              axisLine: {
                onZero: false
              }
            },
            {
              name: '步',
              gridIndex: 1,
              data: xData,
              boundaryGap: false,
              axisLabel: {
                // interval: 4
              },
              splitLine: {
                show: false
              },
              axisLine: {
                onZero: false
              }
            },
            {
              name: '步',
              gridIndex: 2,
              data: xData,
              boundaryGap: false,
              axisLabel: {
                // interval: 4
              },
              splitLine: {
                show: false
              },
              axisLine: {
                onZero: false
              }
            },
            {
              name: '步',
              gridIndex: 3,
              data: xData,
              boundaryGap: false,
              axisLabel: {
                // interval: 4
              },
              splitLine: {
                show: false
              },
              axisLine: {
                onZero: false
              }
            }],
          yAxis:
            [{
              name: '厘米',
              gridIndex: 0,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '毫秒',
              gridIndex: 1,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '次/分钟',
              gridIndex: 2,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '百分比',
              gridIndex: 3,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            }],
          series: [
            {
              name: "步长",
              data: bc,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 0,
              yAxisIndex: 0

            },
            {
              name: "步时",
              data: bs,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 1,
              yAxisIndex: 1
            },
            {
              name: "步频",
              data: bp,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 2,
              yAxisIndex: 2
            },
            {
              name: "对称性",
              data: dcx,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 3,
              yAxisIndex: 3
            },
          ],
        };

        zhexian.setOption(option);
      }
    },
    // 保存评估记录-保存到用户
    btn_save_click: function () {
      this.assessState = "None";
      let self = this;

      //保存评估记录
      CSharp_DB.AssessRecord_Add(JSON.stringify(this.dataResult)).then(res => {
        res = JSON.parse(res);
        if (res.IsSuccess) {
          self.$message.success({
            message: '保存评估记录成功',
          });

          //如果当前在评估页面，刷新评估记录
          if (this.$root.$refs.keepView && this.$root.$refs.keepView.updatePatientList) {
            this.$root.$refs.keepView.updatePatientList();
          }

        } else {
          self.$message.error(res.message);
        }
      });

      //更新用户数据
      var patientData = this.$root.currentPatient;
      patientData.Data = JSON.stringify(this.patientNewData);

      CSharp_DB.Patient_Edit(JSON.stringify(patientData)).then(res => {
        res = JSON.parse(res);
        if (res.IsSuccess) {
          self.$message.success({
            message: '用户数据修改成功',
          });
        } else {
          self.$message.error(res.message);
        }
      });

      this.$emit("close", false);

    },
    // 去训练
    btn_goTrain_click: function () {
      this.btn_save_click();
      this.$emit("close", false);

      this.$root.openTrain_click();
    },

    //计时器
    startUseTimer: function () {
      this.m_useTimer = setInterval(() => {

        if (window.DeviceManager.deviceState == null || !window.DeviceManager.deviceState.IsNormal) {
          this.clearAllTimer();

          var self = this;

          this.$alert("设备状态异常，即将退出评估", "提示",
            {
              confirmButtonText: "确认",
              callback: () => {
                self.btn_Close_click();
              }
            });

          return;
        }

        if (this.assessState == "Running") {
          var newTime = new Date();
          var tVal = Math.floor((newTime - this.lastDate) / 100) / 10;

          this.lastDate = new Date();
          this.currentTime += tVal;
          this.$refs.useTimeTxt.innerHTML = Math.ceil(this.assessTime - this.currentTime);

          if (this.currentTime >= this.assessTime) {

            this.finishFun();
          }
        }
      }, 500);
    },

    //数据更新-由主程序通知
    dataUpdata: function () {

      if (this.assessState != "Running") return;

      // console.log("评估数据更新-子组件");

      CSharp_Assess.GetCurrentRecord().then(res => {
        res = JSON.parse(res);
        if (res.success) {
          this.currentGaitRecord = res.data;
          var newstep = this.currentGaitRecord.Steps[this.currentGaitRecord.Steps.length - 1];

        }
      });
    },

    //清除定时器
    clearAllTimer: function () {
      clearInterval(this.m_dataTimer);
      clearInterval(this.m_useTimer);
      this.m_dataTimer = null;
      this.m_useTimer = null;
    },
  },
  mounted: function () {

    this.InitFun();
  }
};
</script>

<style>
.assesspage {
  width: 100%;
  height: 100%;
  z-index: 999;
  background: radial-gradient(53.43% 50% at 50% 50%, #313249 0%, #2C3043 100%);
  background: url('resource/img/评估背景.jpg');
}

.assesspage .page-head.nobg {
  background: none;
  box-shadow: none;
}

.assesspage .exitBtn {
  position: absolute;
  width: 102px;
  height: 40px;
  right: 30px;
  top: 20px;
  background: #1252F6;
  border: none;
  color: #fff;
}

.el-button.is-disabled,
.el-button.is-disabled:focus,
.el-button.is-disabled:hover {
  background: #acacac;
  color: #fff;
}

.assesspage .exitBtn .thisicon {
  width: 14px;
  height: 14px;
}

.assesspage .RunningBox {
  width: 100%;
  height: calc(100% - 60px);
}

.RunningBox .topBox {
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  padding: 0px;
  gap: 40px;
  width: 1596px;
  height: 106px;
  margin: 0 auto;
}

.RunningBox .middleBox {
  overflow: hidden;
  margin: 0 auto;
  margin-top: 24px;
  box-sizing: border-box;
  width: 1716px;
  height: 650px;
  border-radius: 30px 30px 0px 0px;
}

.RunningBox .middleBox .middleContent {
  overflow: hidden;
  margin: 56px;
  box-sizing: border-box;
  width: 1604px;
  height: 430px;
  border-radius: 30px;
  position: relative;
}

.RunningBox .bottomBox {
  width: 1920px;
  height: 240px;
}

.topBox .dataBox {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 0px;
  gap: 6px;
  width: 200px;
  height: 106px;
  flex: none;
  order: 0;
  flex-grow: 0;
  color: #fff;
}

.dataBox .thisicon {
  width: 40px;
  height: 40px;
}

.dataBox .datadata {
  font-weight: 700;
  font-size: 24px;
  color: #FFFFFF;
}

.dataBox .datatil {
  font-weight: 300;
  font-size: 16px;
  color: #ACACAC;
}

.bottomBox {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  padding: 0px;
  gap: 40px;

  width: 540px;
  height: 106px;
}

.bottomBox .dataBoxBox {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  padding: 0px;
  gap: 40px;

  width: 540px;
  height: 106px;
  color: #fff;
}

.bottomBox .dataBox {
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  align-items: center;
  padding: 0px;
  gap: 20px;
  width: 230px;
  height: 90px;
  flex: none;
  order: 0;
  flex-grow: 0;
}

.bottomBox .dataBox .datadata {
  font-weight: 700;
  font-size: 24px;
  color: #FFFFFF;
}

.bottomBox .dataBox .datatil {
  font-weight: 300;
  font-size: 24px;
  color: #ACACAC;
}

.assesspage .middleTxt {
  position: absolute;
  width: 100%;
  height: 40px;
  left: 0;
  top: 365px;

  font-family: 'Inter';
  font-style: normal;
  font-weight: 300;
  font-size: 34px;
  line-height: 41px;
  text-align: center;
  color: #79BBFF;
}

.assesspage .bottomBox .bottombtn {
  width: 170px;
  height: 170px;
  background: none;
  border: 0;
  padding: 0;
  border-radius: 50%;
}

.assesspage .FinishBox {
  display: flex;
  width: 100%;
  height: 100%;
  background-color: #fff;
  margin: 0;
  flex-direction: column;
  /* justify-content: center; */
  align-items: center;
}

.FinishBox .header {
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 20px;
  margin-top: 20px;
}

.assesspage .btnBox .btn {
  background-color: #f0f3f9;
  font-size: 18px;
  min-width: 150px;
  height: 80px;
  margin: 0 10px;
}

.assesspage .el-input-number .el-input-number__decrease,
.assesspage .el-input-number .el-input-number__increase {
  width: 100px;
}

.assesspage .el-input-number {
  line-height: 60px;
}

.assesspage .el-input-number .el-input__inner {
  height: 60px;
  font-size: 20px;
}

.middleContent .speedBox {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  padding: 40px 0px;
  width: 812px;
  height: 230px;
  margin: 100px auto 0 auto;
  box-sizing: border-box;
  gap: 56px;
}

.middleContent .speedBox .SpeedValue {
  display: flex;
  width: 400px;
  height: 150px;
  color: #FFFFFF;
  /* align-items: center; */
  justify-content: center;
}

.SpeedValue .val {
  min-width: 135px;
  height: 77px;
  font-weight: 700;
  font-size: 64px;
  line-height: 77px;
  text-align: center;
  margin-top: 37px;
}

.SpeedValue .danwei {
  min-width: 47px;
  height: 24px;
  font-weight: 300;
  font-size: 20px;
  line-height: 24px;
  text-align: center;
  margin-top: 90px;
}

.middleContent .speedBox .minusSpeed,
.middleContent .speedBox .addSpeed {
  width: 150px;
  height: 150px;
  border-radius: 50%;
  border: 0;
  padding: 0;
  background: none;
  background-repeat: no-repeat;
  background-size: 100% 100%;
  background-position: center center;
}

.middleContent .speedBox .minusSpeed {
  background-image: url('resource/img/png/步速减-激活.svg');
}

.middleContent .speedBox .minusSpeed.noclick {
  background-image: url('resource/img/png/步速减-禁用.svg');
}

.middleContent .speedBox .minusSpeed.okclick:active {
  background-image: url('resource/img/png/步速减-点击.svg');
}


.middleContent .speedBox .addSpeed {
  background-image: url('resource/img/png/步速加-激活.svg');
}

.middleContent .speedBox .addSpeed.noclick {
  background-image: url('resource/img/png/步速加-禁用.svg');
}

.middleContent .speedBox .addSpeed.okclick:active {
  background-image: url('resource/img/png/步速加-点击.svg');
}
</style>