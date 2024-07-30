<template>
  <div id="assessreport" class="bt-dialog-wrapper flexC-center-lr assessreport">
    <!-- 760px -->
    <div id="reportDialog" class="bt-dialog flexC-center-lr" style="width: 762px; min-height: 1077px">
      <div class="header">
        <div ref="reportName">评估详情</div>

        <el-button id="btnPrint__Report" class="btnPrint__Report" @click="btn_print_click">打印</el-button>
        <el-button id="btnExit_Report" class="btnExit_Report" @click="btn_dialogClose_click">退出</el-button>
      </div>

      <div class="bt-dialog-scorll" id="dialogScorll">
        <div class="bt-dialog-content flexC-center-lr">
          <!-- 用户信息 -->

          <div v-if="thisPatient != null" style="width: calc(100% - 44px); margin-top: 10px">
            <el-descriptions title="用户信息" :column="3" border>
              <el-descriptions-item :width="130" label="姓名">
                {{ thisPatient.Name }}
              </el-descriptions-item>
              <el-descriptions-item :width="130" label="性别">
                {{ thisPatient.Sex == 0 ? "男" : "女" }}
              </el-descriptions-item>
              <el-descriptions-item :width="130" label="年龄">
                {{ thisPatient.Birthday ? new Date().getFullYear() - new Date(thisPatient.Birthday).getFullYear() : "未填写"
                }}
              </el-descriptions-item>
              <el-descriptions-item :width="130" label="病症">
                {{ thisPatient.DiseaseType }}
              </el-descriptions-item>
              <el-descriptions-item :width="130" label="项目">评估测试</el-descriptions-item>
            </el-descriptions>
          </div>

          <div v-if="dataResult != null" style="width: calc(100% - 44px); margin-top: 10px">
            <el-descriptions title="评估数据" :column="3" border>
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
          <div v-show="dataResult != null" style="width: calc(100% - 44px);padding-bottom: 5px;">
            <v-chart style="width: 710px;height:950px;" ref="zheXianResult" />
          </div>

        </div>
      </div>

    </div>
  </div>
</template>

<script>
module.exports = {
  data() {
    return {
      thisPatient: null,
      dataResult: null,
      printTime: ""
    };
  },
  props: ["typeid"],

  methods: {
    btn_dialogClose_click: function () {
      this.$emit("close", false);
    },
    //打印
    btn_print_click: function () {
      document.getElementById("btnPrint__Report").style.opacity = 0;
      document.getElementById("btnExit_Report").style.opacity = 0;
      try {
        if (window.aa_config.isDebug) {
          document.getElementById("alpha_version").style.opacity = 0;
        }
      } catch (error) {

      }
      document.getElementById("dialogScorll").style.overflowY = "visible";

      document.getElementById("currentPatientPanel").style.display = "none";
      var app = document.getElementById("app");
      app.style.width = "100vw";

      var reportDialog = document.getElementById("reportDialog")
      var old = window.getComputedStyle(reportDialog, null).boxShadow;
      reportDialog.style.boxShadow = "rgb(169, 169, 169) 0px 0px 0px 0px";

      setTimeout(() => {
        window.print();
        document.getElementById("btnPrint__Report").style.opacity = 1;
        document.getElementById("btnExit_Report").style.opacity = 1;

        try {
          if (window.aa_config.isDebug) {
            document.getElementById("alpha_version").style.opacity = 1;
          }
        } catch (error) {

        }

        document.getElementById("dialogScorll").style.overflowY = "scroll";
        reportDialog.style.boxShadow = old;
        document.getElementById("currentPatientPanel").style.display = "flex";
        app.style.width = "100vw";

      }, 50);
    },

    finishEcharts: function () {
      var result = JSON.parse(this.dataResult.Result);


      var xData = [];
      var xData_l = [];
      var xData_r = [];
      for (let l = 0; l < result.StepCountL; l++) {
        xData_l.push(l + 1);
      }
      for (let r = 0; r < result.StepCountR; r++) {
        xData_r.push(r + 1);
      }

      var bc_l = [];
      var bc_r = [];
      var bs_l = [];
      var bs_r = [];

      var bp = [];
      var dcx = [];

      for (let index = 0; index < result.Steps.length; index++) {
        const element = result.Steps[index];
        xData.push(index + 1);
        bp.push(element.BP);
        dcx.push(element.DCX);

        if (element.F == 1) {
          bc_l.push(element.BC);
          bs_l.push(element.BS);

        } else {
          bc_r.push(element.BC);
          bs_r.push(element.BS);
        }
      }

      const zhexian = this.$refs.zheXianResult;
      if (zhexian) {
        var option = {
          tooltip: {
            trigger: "axis",
          },
          title: [
            { left: "20%", top: "2%", text: "左脚步长变化" },
            { left: "65%", top: "2%", text: "右脚步长变化" },

            { left: "20%", top: "26%", text: "左脚步时变化" },
            { left: "65%", top: "26%", text: "右脚步时变化" },

            { left: "45%", top: "50%", text: "步频变化" },
            { left: "45%", top: "74%", text: "对称性变化" }
          ],
          grid: [
            { left: "12%", top: "8%", width: "35%", height: "15%" },
            { left: "57%", top: "8%", width: "35%", height: "15%" },

            { left: "12%", top: "32%", width: "35%", height: "15%" },
            { left: "57%", top: "32%", width: "35%", height: "15%" },

            { left: "12%", top: "56%", width: "80%", height: "15%" },
            { left: "12%", top: "80%", width: "80%", height: "15%" },
          ],
          xAxis:
            [{
              name: '步',
              gridIndex: 0,
              data: xData_l,
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
              data: xData_r,
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
              data: xData_l,
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
              data: xData_r,
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
              gridIndex: 4,
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
              gridIndex: 5,
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
              name: '厘米',
              gridIndex: 1,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '毫秒',
              gridIndex: 2,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '毫秒',
              gridIndex: 3,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '次/分钟',
              gridIndex: 4,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },
            {
              name: '百分比',
              gridIndex: 5,
              axisLine: { show: true },
              minInterval: 1,
              axisLabel: {
                formatter: "{value}",
              },
              type: "value",
            },],
          series: [
            {
              name: "左脚步长",
              data: bc_l,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 0,
              yAxisIndex: 0

            },
            {
              name: "右脚步长",
              data: bc_r,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 1,
              yAxisIndex: 1

            },
            {
              name: "左脚步时",
              data: bs_l,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 2,
              yAxisIndex: 2
            },
            {
              name: "右脚步时",
              data: bs_r,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 3,
              yAxisIndex: 3
            },
            {
              name: "步频",
              data: bp,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 4,
              yAxisIndex: 4
            },
            {
              name: "对称性",
              data: dcx,
              type: "line",
              // smooth: true,
              showSymbol: false,
              color: ["#91cc75"],
              xAxisIndex: 5,
              yAxisIndex: 5
            },
          ],
        };

        zhexian.setOption(option);
      }
    },
  },
  mounted: function () {

    this.printTime = "打印时间：" + new Date().Format("yyyy-MM-dd hh:mm:ss");

    this.thisPatient = this.$root.currentPatient;

    this.dataResult = null;

    var self = this;
    CSharp_DB.AssessRecord_GetByID(this.typeid).then((res) => {
      res = JSON.parse(res);
      if (res.IsSuccess) {
        this.dataResult = res.data;

        this.finishEcharts();
      } else {
        self.$message.error("获取失败");
      }
    });

  }
};
</script>

<style>
.btnPrint__Report {
  position: absolute;
  right: 90px;
  top: 15px;
}


.btnExit_Report {
  position: absolute;
  right: 20px;
  top: 15px;
}

.eachar {
  /* 如不够全部显示  在元素后插入分页符 */
  page-break-after: auto;
  /* 避免在 内部插入分页符 */
  page-break-inside: avoid;
}

.assessreport .printTime {
  font-size: 14px;
  width: 100%;
  text-align: right;
}
</style>