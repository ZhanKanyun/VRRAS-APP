

<template>
    <div class="bt-dialog-wrapper flexC-center-lr trainset">
        <div class="bt-dialog flexC-center-lr" style="width: 30vw;height: 30vh;">
            <div class="header">
                <div>训练设置</div>

                <div class="header-close" @click="btn_dialogClose_click">
                    <el-icon>
                        <CloseBold />
                    </el-icon>
                </div>
            </div>

            <div class="flexC-center-lr" style="height:100%;width: 100%;font-size: 16px;">

                <div class="gameSetPanel ml20">

                    <div class="ml20 mt20"
                        v-if="(game_Config != null && game_Config.maxCount != 0 && game_Config.maxCount > 0) || (game_Config != null && game_Config.maxDifficulty > 0) || (game_Config != null && game_Config.maxTime > 0)"
                        style="font-weight: 700;font-size: 18px;">设置</div>


                    <div class="gameSet-item" v-if="game_Config != null && game_Config.maxBuSu > 0">
                        <div class="ml40" style="font-weight: 700;width: 80px;">步速：</div>
                        <el-input-number size="large" class="ml10" :precision="2" style="width: 150px;" v-model="set_BuSu"
                            :step="0.05" :min="0.1" :max="game_Config.maxBuSu" label="">
                        </el-input-number>
                        <div class="ml20">米/秒</div>
                    </div>
                    <div class="gameSet-item" v-if="game_Config != null && game_Config.maxBuPin > 0">
                        <div class="ml40" style="font-weight: 700;width: 80px;">步频：</div>
                        <el-input-number size="large" class="ml10" :precision="0" style="width: 150px;" v-model="set_BuPin"
                            :min="10" :max="game_Config.maxBuPin" label="" :disabled="diseaseType != '帕金森'">
                        </el-input-number>
                        <div class="ml20">次/分</div>
                    </div>
                    <div class="gameSet-item" v-if="game_Config != null">
                        <div class="ml40" style="font-weight: 700;width: 80px;">对称性：</div>
                        <el-input-number size="large" class="ml10" :precision="0" style="width: 150px;"
                            v-model="set_DuiChenXing" :min="50" :max="150" label="" :disabled="diseaseType != '脑卒中'">
                        </el-input-number>
                        <div class="ml20">%</div>
                    </div>
                    <div class="gameSet-item" v-if="game_Config != null && game_Config.maxTime > 0">
                        <div class="ml40" style="font-weight: 700;width: 80px;">训练时长：</div>
                        <el-input-number size="large" class="ml10" :precision="0" style="width: 150px;" v-model="set_Time"
                            :min="1" :max="game_Config.maxTime" label="">
                        </el-input-number>
                        <div class="ml20">分钟</div>
                    </div>

                    <el-button type="primary" size="large" class="btnStart" @click="btnStart_click">开始训练</el-button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
module.exports = {
    data() {
        return {
            config: null,
            gameList: [],
            set_Time: 5,
            set_BuSu: 1,
            set_BuPin: 60,
            set_DuiChenXing: 100,
            game_Config: null,
            diseaseType: "脑卒中"
        }
    },
    props: ['gameid', 'typename'],
    watch: {

    },

    methods: {
        btn_dialogClose_click: function () {
            this.$emit('close', false)
        },

        //开始训练
        btnStart_click: function (e) {
            if (this.$root.currentPatient == null) {
                this.$message.error("请先登录用户");
                return;
            }
            if (window.DeviceManager.deviceState == null || !window.DeviceManager.deviceState.IsNormal) {
                this.$message.error("设备状态异常，详情查看左下角设备状态信息。");
                return;
            }
                        
            if (this.set_Time == undefined || this.set_Time == "") {
                this.$message.error("请输入时长");
                return;
            }
            if (this.set_BuSu == undefined || this.set_BuSu == "") {
                this.$message.error("请输入步速");
                return;
            }
            if (this.set_DuiChenXing == undefined || this.set_DuiChenXing == "") {
                this.$message.error("请输入正确对称性");
                return;
            }
            if (this.set_BuPin == undefined || this.set_BuPin == "") {
                this.$message.error("请输入正确步频");
                return;
            }
            if (this.game_Config.runMode == 'Web') {


            } else {

                this.game_Config.set_Time = this.set_Time;
                this.game_Config.set_BuSu = this.set_BuSu;
                this.game_Config.set_BuPin = this.set_BuPin;
                this.game_Config.set_DuiChenXing = this.set_DuiChenXing;


                CSharp_App.StartGame(JSON.stringify(this.game_Config)).then(res => {
                    res = JSON.parse(res);
                    if (res.IsSuccess) {
                        CSharp_App.KeyBoard_Hide();
                        this.$emit('close', false)
                        // this.$root.$message.success('启动成功');
                    } else {
                        this.$root.$message.error('启动失败，' + res.message);
                    }
                });
            }

        },
    },
    created: function () {

    },
    mounted: function () {

        // symm speed steplenght rhythm
        this.diseaseType = this.$root.currentPatient.DiseaseType;

        console.oldLog(this.$root.currentPatient.Data);
        if (this.$root.currentPatient.Data != null) {
            var newSet = JSON.parse(this.$root.currentPatient.Data);
            this.set_BuSu = newSet.speed;
            this.set_BuPin = newSet.rhythm;

            if (this.diseaseType == "脑卒中") {
                this.set_DuiChenXing = newSet.symm;
            }
        }

        let gameid = this.gameid;
        this.currentSelectTrainID = gameid;
        this.game_Config = window.aa_localDB.getTrainByid(gameid);
        if (this.game_Config.maxDifficulty > 0) {
            this.set_Difficulty = 1;
        }


    },
};
</script>

<style>
.trainset .el-radio__input {
    width: 0px;
    height: 0px;
}

.trainset .el-radio__inner {
    border: none;
    width: 0px;
    height: 0px;
}

.trainset .el-radio {

    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    margin-bottom: 5px;
    margin-top: 5px;
    margin-right: 10px;
    width: 40px;
}

.trainset .el-radio__label {
    font-size: var(--el-radio-font-size);
    padding-left: 0px;
}

.trainset .el-radio.is-bordered {
    padding: 0 15px 0 15px;
}

.trainset .el-radio.is-bordered.is-checked {
    border-color: var(--el-color-primary);
    background-color: var(--el-color-primary);
}

.trainset .el-radio__input.is-checked+.el-radio__label {
    color: white;
}

.trainset .el-radio__inner::after {
    width: 0px;
}

.trainset .gameList-wrap {
    height: calc(100% - 40px);
    overflow-y: hidden;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;

}

.trainset .gameList-tit {
    width: calc(100% - 40px);
    height: 40px;
    font-size: 18px;
}

.trainset .gameList {
    width: 500px;
    overflow-y: scroll;
    min-height: calc(100% - 40px);
}

.trainset .game {
    position: relative;
    width: 100%;
    height: 186px;
    margin-bottom: 10px;
    flex-shrink: 0;
    border-radius: 8px;
}

.trainset .game-select {
    position: relative;
    width: 100%;
    height: 186px;
    margin-bottom: 10px;
    flex-shrink: 0;
    /* box-shadow: 0px 0px 5px #1252f6; */
    /* transform: scale(1.05); */
    border: solid blue 3px;
    box-sizing: border-box;
    border-radius: 8px;
}

.trainset .gameAction {
    position: absolute;
    left: 10px;
    bottom: 8px;
    font-size: 14px;
    color: white;
    pointer-events: none;
}

.trainset .gameSetPanel {
    position: relative;
    width: 500px;
    height: 100%;
    /* background-color: blue; */
}

.trainset .gameSet-tit {
    margin-top: 20px;
    margin-left: 20px;
    font-size: 24px;
    font-weight: bold;
}

.trainset .gameSet-item {
    width: 96%;
    display: flex;
    flex-direction: row;
    align-items: center;
    margin-top: 30px;
}

.trainset .btnStart {
    position: absolute;
    bottom: 30px;
    right: 200px;
}
</style>
