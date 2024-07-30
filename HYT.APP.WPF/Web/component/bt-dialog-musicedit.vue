<template>
    <div class="bt-dialog-wrapper flexC-center-lr musicEdit">
        <div class="bt-dialog flexC-center-lr" style="width: 30vw;min-height:40vh;">
            <div class="header">
                <div>{{ dialogName }}</div>

                <div class="header-close" @click="btn_dialogClose_click">
                    <el-icon>
                        <CloseBold />
                    </el-icon>
                </div>
            </div>

            <el-form v-if="dialog_pageModel != null" id="formedit" label-position="right" label-width="65px"
                style="width:80%" :model="dialog_pageModel" :rules="dialog_add_rules" ref="editUserForm"
                class="flexC-center-lr mt40" size="large">

                <el-form-item label="文件上传" prop="FilePath" label-width="80px" class="flexR-center-lr">
                    <!-- <el-input v-model="dialog_pageModel.FilePath" disabled>
                    </el-input>
                    <el-button class="btnSelectMusic" size="mini" @click="btn_selectMusic_click">上传音乐</el-button> -->

                    <el-upload ref="uploadMusic" size="large" v-model:file-list="fileList" accept=".mid"
                        :on-success="uploadsuccess" :show-file-list="false" :limit="2">
                        <el-button type="primary">{{ dialogName == "上传音乐" && dialog_pageModel.Size <= 0 ? "点击上传" : "重新上传"
                        }}</el-button>
                    </el-upload>

                    <span v-if="dialog_pageModel.Size <= 0" class="form-item-hint ml20" style="color: orangered;">请先上传音乐
                    </span>
                    <!-- <span v-if="dialog_pageModel.Size>0" class="form-item-hint ml20">文件大小：{{ dialog_pageModel.Size}}KB </span> -->

                </el-form-item>

                <el-form-item label="音乐名称" prop="Name" label-width="80px">
                    <el-input v-model="dialog_pageModel.Name" minlength="1" maxlength="30" show-word-limit>
                    </el-input>
                </el-form-item>
                <el-form-item label="音乐大小" prop="Size" label-width="80px">
                    <el-input style="width: 150px;" disabled v-model="dialog_pageModel.Size">
                    </el-input>
                    <span class="form-item-hint ml10"> KB</span>
                </el-form-item>

                <el-form-item label="BPM" prop="BPM" label-width="80px">
                    <!-- <el-input type="text" v-model="dialog_pageModel.BPM" minlength="0" maxlength="11" show-word-limit
                        clearable oninput="value=value.replace(/[^0-9]/g,'')">
                    </el-input> -->
                    <el-input-number v-model="dialog_pageModel.BPM" placeholder="输入BPM" :precision="0" :min="1" :max="200" :step="1" />
                    <span class="form-item-hint ml10"> bpm</span>
                </el-form-item>



            </el-form>

            <el-button class="btnSave" type="primary" @click="submitForm('editUserForm')">保存
            </el-button>
        </div>
    </div>
</template>

<script>
// debugger.
module.exports = {
    created: function () {
        // debugger
    },
    data: function () {
        return {
            dialogName: "修改音乐信息",
            dialog_pageModel: null,
            fileList: [],
            dialog_add_rules: {
                // Name: [
                //     { required: true, message: '请输入音乐名称', trigger: 'change' },
                //     { min: 2, max: 8, message: '音乐名称长度必须在 1 到  20 个字符之间', trigger: 'blur' },
                //     { pattern: "^[\u4e00-\u9fa5a-zA-Z0-9]+$", message: '音乐名称仅能由中文、字母、数字组成', trigger: 'blur' },
                // ],
                // FilePath: [
                //     { required: true, message: '请选择文件', trigger: 'change' },
                // ],
                BPM: [
                    { required: true, message: '请指定BPM', trigger: 'change' },
                ],
            },
            musicStearm: '',
        };
    },
    props: ['musicid'],
    watch: {
    },
    methods: {

        uploadsuccess: function (e, a, b) {
            if (this.fileList.length > 1) {
                this.fileList.splice(0, 1);
            }
            var self = this;
            this.musicStearm = "";
            var a = this.fileList;
            var file = a[0];
            console.log("【音乐上传】选中文件" + file);
            var reader = file.raw.stream().getReader();
            var result = "";
            // read() 返回了一个 promise
            // 当数据被接收时 resolve
            reader.read().then(function processText({ done, value }) {
                // Result 对象包含了两个属性：
                // done  - 当 stream 传完所有数据时则变成 true
                // value - 数据片段。当 done 为 true 时始终为 undefined  字符串用逗号间隔
                if (done) {

                    console.log("【音乐上传】Stream reader complete,Size=" + result.length);

                    self.musicStearm = result;
                    self.dialog_pageModel.Size = (file.size/1000).toFixed(0);
                    // if (self.dialog_pageModel.Name == "" || self.dialog_pageModel.Name == undefined) {
                    self.dialog_pageModel.Name = file.name.replace(".mid", "");
                    // }
                    return;
                }

                const chunk = value;
                
                // console.log(chunk);
                if (result!="") {
                    result += ','+chunk;
                }else
                {
                    result += chunk;
                }
             

                // 再次调用这个函数以读取更多数据
                return reader.read().then(processText);
            });

        },
        btn_dialogClose_click: function () {
            this.$emit('close', false)
        },
        //选择文件
        btn_selectMusic_click: function (e) {
            CSharp_DB.JS_MusicSelectFile().then(res => {
                res = JSON.parse(res);
                if (res.success) {
                    this.dialog_pageModel.FilePath = res.data;
                }
            });
        },
        //提交表单
        submitForm(formName) {
            if (this.musicid.length <= 0) {
                if (this.musicStearm == "") {
                    this.$message.error({
                        message: '请先上传音乐',
                    });
                    return;
                }
            }
            // debugger
            this.$refs[formName].validate((valid) => {
                if (valid) {
                    let self = this;
                    //编辑
                    if (this.musicid.length > 0) {
                        CSharp_DB.Music_EditStream(JSON.stringify(this.dialog_pageModel), this.musicStearm).then(res => {
                            res = JSON.parse(res);
                            if (res.IsSuccess) {
                                self.$message.success({
                                    message: '修改成功',
                                });
                                CSharp_App.KeyBoard_Hide();
                                // debugger
                                this.$emit('close', true);//刷新父组件

                            } else {
                                self.$message.error(res.message);
                            }
                        });
                    }
                    //新建
                    else {
                        CSharp_DB.Music_AddStream(JSON.stringify(this.dialog_pageModel), this.musicStearm).then(res => {
                            
                            res = JSON.parse(res);
                            if (res.IsSuccess) {
                                self.$message.success({
                                    message: '添加成功',
                                });
                                CSharp_App.KeyBoard_Hide();
                                this.$emit('close', true);//刷新父组件

                            } else {
                                self.$message.error(res.message);
                            }
                        });
                    }
                } else {
                    console.log('提交错误');
                    return false;
                }
            });
        },
        //重置表单
        resetForm(formName) {
            this.$refs[formName].resetFields();
        },
    },
    mounted: function () {
        //编辑
        if (this.musicid.length > 0) {
            this.dialogName = "修改音乐信息";

            //解决问题，同一个引用对象，修改编辑框没点确定会直接变
            CSharp_DB.Music_GetByID(this.musicid).then(res => {
                res = JSON.parse(res);
                if (res.success) {
                    this.dialog_pageModel = res.data;
                }
            });
        }
        //新建
        else {
            this.dialogName = "上传音乐";
            this.dialog_pageModel = {
                Name: "",
                BPM: 60,
                FilePath: "",
                Size: 0,
            };

        }
        this.musicStearm = "";

    }
};
</script>

<style>
.btnSave {
    position: absolute;
    bottom: 40px;
    right: 40px;
}

/* 添加用户弹窗 */
.musicEdit .dialogForm {
    display: flex;
    width: 100%;
    grid-template-columns: 430px 430px;
    grid-template-rows: repeat(6, 71px);
    align-items: center;
    column-gap: 40px;
    justify-content: center;

    margin-top: 40px;
}

.musicEdit .el-form-item {
    width: 100%;
    font-size: 50px;
}
</style>
