<template>
    <div class="bt-dialog-wrapper flexC-center-lr patientedit">
        <div class="bt-dialog flexC-center-lr" style="width: 30vw;min-height:50vh;">
            <div class="header">
                <div>{{ dialogName }}</div>

                <div class="header-close" @click="btn_dialogClose_click">
                    <el-icon>
                        <CloseBold />
                    </el-icon>
                </div>
            </div>

            <el-form v-if="dialog_pageModel != null" id="formedit" label-position="right" label-width="65px"
                :model="dialog_pageModel" :rules="dialog_add_rules" ref="editUserForm" class="flexC-center-lr mt40"
                size="large">
                <el-form-item label="姓名" prop="Name" label-width="80px">
                    <el-input v-model="dialog_pageModel.Name" minlength="2" maxlength="8" show-word-limit 
                        oninput="value=value.replace(/[^\u4e00-\u9fa5a-zA-Z]/g,'')">
                    </el-input>
                    <span class="form-item-hint">提示：必填，由 2 到 8 个中文、字母组成</span>
                </el-form-item>

                <el-form-item label="性别" prop="Sex" label-width="80px">
                    <el-radio-group v-model="dialog_pageModel.Sex">
                        <el-radio-button :label="0">男</el-radio-button>
                        <el-radio-button :label="1">女</el-radio-button>
                    </el-radio-group>
                </el-form-item>
                <el-form-item label="出生日期" label-width="80px">
                    <el-date-picker type="date" placeholder="选择日期" v-model="dialog_pageModel.Birthday"
                        :disabled-date="disabledDate" style="width: 100%;">
                    </el-date-picker>
                </el-form-item>
                <!-- <el-form-item label="编号" prop="SN">
            <el-input v-model="dialog_pageModel.SN" minlength="5" maxlength="18" show-word-limit clearable>
            </el-input>
            <span class="form-item-hint">提示：由 5 到 18 个字母或数字组成（不填将自动生成编号）</span>
          </el-form-item> -->




                <el-form-item label="手机号" prop="Phone" label-width="80px">
                    <el-input type="text" v-model="dialog_pageModel.Phone" minlength="0" maxlength="11" show-word-limit
                        clearable oninput="value=value.replace(/[^0-9]/g,'')">
                    </el-input>
                </el-form-item>


                <el-form-item label="用户病症" prop="DiseaseType" label-width="80px">
                    <!-- <el-input v-model="dialog_pageModel.DiseaseType" minlength="0" maxlength="20" show-word-limit
                        clearable spellcheck="false">
                    </el-input> -->
                    <el-select v-model="dialog_pageModel.DiseaseType" placeholder="请选择病症">
                        <el-option v-for="item in diseaseTypes" :key="item.Name" :label="item.Name" :value="item.Name" />
                    </el-select>
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
            dialogName: "修改用户信息",
            dialog_pageModel: null,
    
            diseaseTypes:[
                {ID:'1',Name:'帕金森'},{ID:'2',Name:'脑卒中'}
            ],
            dialog_add_rules: {
                Name: [
                    { required: true, message: '请输入姓名', trigger: 'change' },
                    { min: 2, max: 8, message: '姓名长度必须在 2 到  8 个字符之间', trigger: 'blur' },
                    { pattern: "^[\u4e00-\u9fa5a-zA-Z]+$", message: '姓名仅能由中文、字母组成', trigger: 'blur' },
                ],
                DiseaseType: [
                    { required: true, message: '请选择病症', trigger: 'change' },
                ],
            }
        };
    },
    props: ['userid'],
    watch: {
    },
    methods: {
        btn_dialogClose_click: function () {
            this.$emit('close', false)
        },
        disabledDate: function (time) {
            return time.getTime() > Date.now() || time.getTime() < new Date(1800, 1, 1)
        },
        //提交表单
        submitForm(formName) {
            // debugger
            this.$refs[formName].validate((valid) => {
                if (valid) {
                    let self = this;
                    //编辑用户
                    if (this.userid.length>0) {
                        CSharp_DB.Patient_Edit(JSON.stringify(this.dialog_pageModel)).then(res => {
                            res = JSON.parse(res);
                            if (res.IsSuccess) {
                                self.$message.success({
                                    message: '修改成功',
                                });
                                // debugger
                                this.$emit('close', true);//刷新父组件
                                this.$root.getDataCacheFromCSharp();//刷新app和导航栏
                                this.$parent.$parent.getDataCacheFromCSharp();//刷新当前用户页面和导航栏
                                CSharp_App.KeyBoard_Hide();
                            } else {
                                self.$message.error(res.message);
                            }
                        });
                    }
                    //新建用户 
                    else {
                        CSharp_DB.Patient_Add(JSON.stringify(this.dialog_pageModel)).then(res => {
                            res = JSON.parse(res);
                            if (res.IsSuccess) {
                                self.$message.success({
                                    message: '添加成功',
                                });
                                this.$emit('close', true);//刷新父组件
                                CSharp_App.KeyBoard_Hide();

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
        //编辑用户
        if (this.userid.length>0) {
            this.dialogName = "修改用户信息";
            //解决问题，同一个引用对象，修改编辑框没点确定会直接变
            CSharp_DB.Patient_GetByID(this.userid).then(res => {
                res = JSON.parse(res);
                if (res.success) {
                    this.dialog_pageModel = res.data;
                }
            });
        }
        //新建用户
        else {
            this.dialogName = "添加用户";
            this.dialog_pageModel = {
                SN: "",
                Name: "",
                Sex: 0,
                Birthday: "",
                Phone: ""
            };
        }

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
.patientedit .dialogForm {
    display: flex;
    width: 100%;
    grid-template-columns: 430px 430px;
    grid-template-rows: repeat(6, 71px);
    align-items: center;
    column-gap: 40px;
    justify-content: center;

    margin-top: 40px;
}

.patientedit .el-form-item {
    width: 80%;
    font-size: 50px;
}
</style>
