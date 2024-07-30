<template>
    <div class="page flexC-center-lr pageTrain">

        <div class="projectGroup">
            <div class="group-title">评估</div>
            <div class="group-content flexR-center-lr">
                <div class="itemCard" @click="btn_openAssess_click" :data-assessid="item.id" v-for="item in pcList"
                    :key="item.name">
                    <el-image class="itemCard-img" :src="item.img" fit="fill" />
                    <span class="itemCard-nameTitle">名称</span>
                    <span class="itemCard-name">{{ item.name }}</span>
                </div>
            </div>
        </div>

        <div class="projectGroup">
            <div class="group-title">训练和游戏</div>
            <div class="group-content flexR-center-lr">
                <div @click="btn_openTrain_click" :data-id="item.id" class="itemCard" v-for="item in xlList">
                    <el-image class="itemCard-img" :src="item.img" fit="fill" />

                    <span class="itemCard-nameTitle">名称</span>
                    <span class="itemCard-name">{{ item.name }}</span>
                </div>
            </div>
        </div>


    </div>
</template>

<script>
module.exports = {
    data: function () {
        return {
            pcList: [],
            xlList: [],
        }
    },
    methods: {
        /**
         * 打开游戏或训练
         * @param {*} e 
         */
        btn_openTrain_click: function (e) {
            if (this.$root.currentPatient == null) {
                this.$message.error("请先登录用户");
                return;
            }
            if (window.DeviceManager.deviceState == null || !window.DeviceManager.deviceState.IsNormal) {
                this.$message.error("设备状态异常，详情查看左下角设备状态信息");
                return;
            }
            var id = e.target.dataset.id;

            if (this.$root.currentPatient.Data == null) {
                this.$message.error("请先进行评估，再开始训练和游戏！");
                return;
            }

            //打开实景训练
            if (id == 100001) {
                this.$root.openTrain_click();
            } else {//打开游戏
                var game_Config = window.aa_localDB.getTrainByid(id);

                CSharp_App.StartGame(JSON.stringify(game_Config)).then(res => {
                    res = JSON.parse(res);
                    if (res.IsSuccess) {
                        // this.$root.$message.success('启动成功');
                    } else {
                        this.$root.$message.error('启动失败，' + res.message);
                    }
                });
            }
        },
        /**
         * 打开评估
         * @param {*} e 
         */
        btn_openAssess_click: function (e) {
            if (this.$root.currentPatient == null) {
                this.$message.error("请先登录用户");
                return;
            }
            if (window.DeviceManager.deviceState == null || !window.DeviceManager.deviceState.IsNormal) {
                this.$message.error("设备状态异常，详情查看左下角设备状态信息");
                return;
            }

            var id = e.target.dataset.assessid;

            if (id == "scale1001") {

                this.$root.current_assess_id = id;
                this.$root.dialog_assess_visible = true;
                CSharp_App.KeyBoard_Hide();
            }

        },
    },
    mounted: function () {
        this.pcList = this.$root.aa_localDB.data_pcList;
        this.xlList = this.$root.aa_localDB.data_xlList;
    },
    deactivated: function () {
        this.trainset_dialog_visible = false;
    }
}


</script>

<style>
.pageTrain {
    position: relative;
    /* background-color: white; */
    height: 100%;
}

.pageTrain .projectGroup {
    margin-top: 100px;
    width: calc(100% - 100px);
    font-size: 24px;
}

.pageTrain .group-title {
    border-bottom: 1px solid #A4A6AA;
}

.pageTrain .group-content {
    margin-top: 0px;
}

.itemCard {
    position: relative;
    height: 250px;
    width: 250px;
    margin-left: 25px;
    margin-top: 25px;
    box-shadow: 4px 4px 4px 0px rgba(0, 0, 0, 0.3);
    border-radius: 10px;
}

.itemCard-img {
    position: absolute;
    width: 100%;
    flex-grow: 1;
    pointer-events: none;
}

.itemCard-nameTitle {
    position: absolute;
    left: 10px;
    bottom: 37px;
    font-size: 12px;
    pointer-events: none;
    color: #909399;
    font-weight: 700;
}

.itemCard-name {
    position: absolute;
    left: 10px;
    bottom: 12px;
    font-size: 20px;
    line-height: 24px;
    pointer-events: none;
    color: white;
    font-weight: 700;
}
</style>