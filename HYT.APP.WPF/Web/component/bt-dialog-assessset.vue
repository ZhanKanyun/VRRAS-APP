<template>

  <div class="bt-dialog-wrapper flexC-center-lr">
    <div class="bt-dialog flexC-center-lr" style="width: 950px;">
      <div class="header">
        <div>选择评估</div>

        <div class="header-close" @click="btn_dialogClose_click">
          <el-icon>
            <CloseBold />
          </el-icon>
        </div>
      </div>
      <div class="flexR-center-lr" style="height: 560px">
        <div class="assessList flexC-center-lr ml20">
          <div :class="scale.id == currentSelectScaleID ? 'assess-select' : 'assess'"
            v-for="(scale, index) in scalesList" :key="scale.id" @click="scale_click" :data-id="scale.id">
            <el-image :src="scale.icon" fit="fill" style="pointer-events: none;height: 100%;width: 100%;" />
            <!-- :style="{ left: index % 2 == 0 ? 10 + 'px' : 'auto',right: index % 2 != 0 ? 10 + 'px' : 'auto' }" -->
            <div class="assess-name" >{{ scale.name }}</div>
          </div>
        </div>

        <div class="assessSetPanel ml40">
          <!-- <div class="ml20 mt20" style="font-weight: 700;font-size: 18px;">
            设备状态
          </div>
          <div class="content ml20 mt20">
            <canvas id="canvas3d"> </canvas>
          </div> -->
          <div class="ml20 mt20" style="font-weight: 700;font-size: 18px;">测试方法</div>
          <div class="ml20 mt20" style="height: 270px;width:400px;pointer-events: none;">
            <img :src="imgSrc" style="max-width: 100%;max-height: 100%;">
          </div>
          <div class="ml20 mt20" style="max-width: 400px;font-size: 16px;">
            {{ pcMethod }}
          </div>

          <el-button type="primary" size="large" class="btnStart" @click="btnStart_click">开始评估</el-button>
        </div>
      </div>

    </div>
  </div>
</template>

<script>
module.exports = {
  data() {
    return {
      dialog_visible: false,
      scalesList: [],
      currentSelectScaleID: -1,
      pcMethod: "",
      imgSrc:''
    };
  },
  props: ["typeid"],

  methods: {
    btn_dialogClose_click: function () {
      this.$emit("close", false);
    },

    scale_click: function (e) {
      let scaleid = e.target.dataset.id;
      this.currentSelectScaleID = scaleid;
      let scale = window.aa_localDB.getAssessByid(scaleid);
      this.pcMethod = scale.pcMethod;
      this.imgSrc = scale.img;
    },

    btnStart_click: function () {
      if (
        window.DeviceManager.connectDevstate()
      ) {
        this.btn_dialogClose_click();

        let scale = window.aa_localDB.getAssessByid(this.currentSelectScaleID);

        this.$root.$refs.freewalkDiv.$refs.assesstitle.innerHTML = scale.name;
        window.m_assessData = null;
        this.$root.$refs.freewalkDiv.show(this.typeid);
        window.cameraFlag = this.typeid;

      } else {
        // this.$message.error({
        //   message: window.DeviceManager.sensorIDStr + "设备未连接！",
        // });
        this.$message.error({
          message: "启动失败，设备未连接！",
        });
      }
    },
  },
  mounted: function () {
    let type = window.aa_localDB.getAssessTypeByID(this.typeid);
    let scales = [];
    type.scaleids.forEach((assessID) => {
      let scale = window.aa_localDB.getAssessByid(assessID);
      if (scale) {
        scales.push(scale);
      }
    });

    this.scalesList = scales;
    if (this.scalesList.length > 0) {
      this.pcMethod = this.scalesList[0].pcMethod;
      this.currentSelectScaleID = this.scalesList[0].id;
      this.imgSrc = this.scalesList[0].img;
    } else {
      this.pcMethod = "";
      this.currentSelectScaleID = -1;
    }
  },
};
</script>

<style>

.assessList {
  width: 440px;
  height: 100%;
  /* background-color: red; */
}

.assess-name {
  position: absolute;
  right: 10px;
  bottom: 10px;
  z-index: 8;
  background-color: #628CF9;
  border-radius: 8px;
  height: 20px;
  padding: 0px 10px;
  color: white;
  pointer-events: none;
}

.assess {
  position: relative;
  width: 100%;
  height: 80px;
  /* background-color: aqua; */
  margin-top: 10px;
  border-radius: 8px;
}

.assess-select {
  position: relative;
  width: 100%;
  height: 80px;
  background-color: green;
  margin-top: 10px;
  border: solid blue 3px;
  box-sizing: border-box;
  box-shadow: 0px 4px 4px rgba(0, 0, 0, 0.25);
  border-radius: 8px;
}

.assessSetPanel {
  position: relative;
  width: 440px;
  height: 100%;
  /* background-color: blue; */
}

.btnStart {
  position: absolute;
  bottom: 30px;
  right: 20px;
}

.content {
  position: relative;
  flex-grow: 1;
  width: 80%;
  background: #ebeef5;
}
</style>
