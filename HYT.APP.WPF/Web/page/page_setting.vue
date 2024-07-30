<template>
    <div class="page flexC-center-lr pageSetting">

        <div class="page-head flexR-center-lr">
            <i class="icon-dw ml40"></i>
            <div class="ml10">系统设置</div>
        </div>
        <div class="page-content flexC-center-lr">

            <div class="wrapper flexC-center-lr mt40" v-if="setting != null">
     

                <div class="item mt40">
                    <div class="item-tit">曲库管理</div>
                    <div class="item-content flexR-center-lr">

                        <el-button size="large" type="primary" @click="btn_musicManager_click">曲库管理

                        </el-button>

                    </div>
                </div>


                <div class="item">
                    <div class="item-tit">关于</div>
      
                    <div class="item-content flexR-center-lr">
                        <el-descriptions class="margin-top infos mt10" title="" :column="1" border v-if="config != null"
                            size="large">
                            <el-descriptions-item v-for="(info, index) in config.infos" :key="index" :min-width="200">
                                <template #label>
                                    {{ info.name }}
                                </template>
                                {{ info.value }}
                            </el-descriptions-item>
                        </el-descriptions>
                    </div>



                </div>

            </div>
            <el-image class="back" src="../Web/resource/img/page-set-back.png" fit="fill" />


        </div>

        <!-- 音乐管理 -->
        <bt-dialog-musicmananger v-if="music_dialog_visible" style="position:absolute;" @close="music_Close">
        </bt-dialog-musicmananger>

        <!-- 训练音乐配置 -->
        <bt-dialog-musictrain v-if="musicTrain_dialog_visible" style="position:absolute;" @close="musicTrain_Close">
        </bt-dialog-musictrain>

    </div>
</template>

<script>
// debugger
module.exports = {
    inject: ['admin'],
    data: function () {
        return {
            setting: null,
            config: null,
            dbbackup_dialog_visible: false,
            dialog_changepwd_visible: false,
            about_dialog_visible: false,
            music_dialog_visible: false,
            musicTrain_dialog_visible: false,
            // admin:null
        }
    },
    methods: {
        organizationName_keyDown: function (e) {
            if (e.key == "Enter") {
                this.$refs.organizationName.blur();
            }
        },
        organizationName_blur: function () {
            this.btn_changeDWMC_click();
        },
        music_Close: function () {
            this.music_dialog_visible = false;
        },
        btn_musicTrain_click:function(e){
            this.musicTrain_dialog_visible = true;
        },
        musicTrain_Close: function () {
            this.musicTrain_dialog_visible = false;
        },

        updateSetting: function () {
            CSharp_DB.Setting_Get().then(res => {
                res = JSON.parse(res);
                // debugger
                if (res.success) {
                    this.setting = res.data.setting;
                }
            });
        },

        // 修改单位名称
        btn_musicManager_click: function (e) {
            this.music_dialog_visible = true;
        }
    },
    created: function () {
    },
    mounted: function () {

        console.log("setting mounted");
        this.updateSetting();
        this.$root.getDataCacheFromCSharp();
        this.config = this.$root.aa_config;


    },
}
</script>

<style>
.pageSetting .admin .el-button+.el-button {
    margin-left: 0px;
    margin-right: 12px;
}

.pageSetting .back {
    position: absolute;
    width: 600px;
    height: 500px;
    right: 20px;
    bottom: 20px;
}

.pageSetting .version {
    font-size: 16px;
}

.pageSetting .wrapper {
    position: relative;
    width: calc(100% - 80px);
    height: calc(100% - 80px);
    background-color: #FFFFFF;
    border-radius: 8px;
}

.pageSetting .item {
    width: calc(100% - 80px);
    margin-bottom: 40px;
}

.pageSetting .item-tit {
    font-weight: 700;
    font-size: 18px;
    color: #444444;
    margin-bottom: 10px;
}

.pageSetting .item-content {
    width: 600px;
}

.el-descriptions__cell,
.el-descriptions__label,
.is-bordered-label {
    width: 100px !important;
}
</style>