<template>
  <div class="page flexC-center-lr patientManager">
    <div class="page-head flexR-center-lr">
      <i class="icon-dw ml40"></i>
      <div class="ml10">用户管理</div>
    </div>

    <div class="page-content flexC-center-lr">
      <!-- 搜索、添加、恢复 -->
      <div class="content-operate flexR-center-lr">

        <div class="flexR-center-lr">
          <el-button size="large" type="primary" @click="btn_addPatient_click">
            <el-icon class="el-icon--right mr10">
              <Plus />
            </el-icon> 添加用户
          </el-button>
        </div>

        <div style="width: 400px" class="ml40">
          <el-input size="large" maxlength="20" v-model="search_key" placeholder="输入姓名、编号搜索" spellcheck="false">
            <template #prefix>
              <el-icon class="el-input__icon">
                <search />
              </el-icon>
            </template>
            <template #append>
              <el-button slot="append" @click="btn_clearSearchKey_click">
                <el-icon>
                  <Close />
                </el-icon>
              </el-button>
            </template>

          </el-input>
        </div>

      </div>


      <!-- 表格 -->
      <div class="content-table-border flexC-center-lr">
        <el-table ref="table" :data="patientList" class="content-table" :highlight-current-row="false" :stripe="true"
          :border="false" @row-click="row_click" @row-dblclick="row_dblclick">
          <el-table-column type="index" width="70" label='序号' align="center" />
          <el-table-column prop="Name" label="姓名" width="180" align="center" />
          <el-table-column prop="SN" label="编号" width="215" align="center" />

          <el-table-column prop="Sex" label="性别" width="70" align="center">
            <template #default="scope">
              <span>{{ scope.row.Sex == 0 ? '男' : '女' }}</span>
            </template>
          </el-table-column>
          <!-- <el-table-column prop="IDCardNo" label="身份证号" width="220" align="center" /> -->
          <el-table-column prop="Birthday" label="年龄" width="70" align="center">
            <template #default="scope">
              <span>{{
                scope.row.Birthday ? new Date().getFullYear() - new
                  Date(scope.row.Birthday).getFullYear() : ''
              }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="DiseaseType" label="病症" width="120" align="center" header-align="center" />
          <el-table-column prop="Phone" label="手机号" width="140" align="center" header-align="center" />
          <el-table-column prop="Data" label="最新步态记录" width="540" align="center" class="hidden-lg-and-down">
            <template #default="scope">
              <span style="white-space: pre;">{{ getDataResult(scope.row.Data) }}</span>
            </template>
          </el-table-column>
          <!-- <el-table-column prop="CreateTime" label="添加时间" width="215" align="center" class="hidden-lg-and-down">
            <template #default="scope">
              <span>{{ scope.row.CreateTime ? new Date(scope.row.CreateTime).Format() : '' }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="LastLoginTime" label="最后登录时间" width="215" align="center" class="hidden-lg-and-down">
            <template #default="scope">
              <span>{{ scope.row.LastLoginTime ? new Date(scope.row.LastLoginTime).Format() : ''
              }}</span>
            </template>
          </el-table-column> -->

          <el-table-column prop="ShoeSize" label="操作" align="center" min-width="200">
            <template #default="scope">
              <!-- <el-button size="mini" @click="btn_userSJFX_click(scope.$index, scope.row)">数据分析
            </el-button> -->
              <el-button size="large" style="margin-left:10px;"
                @click.stop="btn_patientEdit_click(scope.$index, scope.row)">修改
              </el-button>
              <span class="ml10">|</span>
              <div v-if="$root.currentPatient != null && $root.currentPatient.ID == scope.row.ID"
                style="color:#DBDBDB;width: 70px;display: inline-block;" class="ml10">使用中</div>

              <el-button v-else style="margin-left:10px;" size="large"
                @click.stop="btn_patientChange_click(scope.$index, scope.row)">登录
              </el-button>

              <span class="ml10">|</span>
              <div v-if="$root.currentPatient != null && $root.currentPatient.ID == scope.row.ID"
                style="color:#DBDBDB;width: 70px;display: inline-block;" class="ml10">使用中</div>
              <el-button v-else type="danger" style="margin-left:10px;" size="large"
                @click.stop="btn_userDelete_click(scope.$index, scope.row)">删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- 分页 -->
        <el-pagination class="pagination mt10" @current-change="page_change" :current-page="page_index" background
          layout="prev, pager, next,total" :total="page_count" :page-size="page_size">
        </el-pagination>
      </div>



    </div>


    <!-- 编辑用户 -->
    <bt-dialog-patientedit style="position:absolute;" v-if="pateintEdit_dialog_visible" :userid="dialog_edit_userid"
      @close="pateint_edit_Close"></bt-dialog-patientedit>
  </div>
</template>

<script>
// debugger
module.exports = {

  setup() {

  },
  data: function () {
    return {
      patientList: [],
      page_index: 1,
      page_count: 0,
      page_size: 10,
      search_key: "",
      pateintEdit_dialog_visible: false,
      dialog_edit_userid: 0,

      selectionArray: []

    };
  },
  methods: {
    //解析用户评估数据
    getDataResult: function (data) {
      var result = "";
      try {
        if (data.length > 0) {
          data = JSON.parse(data);
          if (data.speed!=undefined) {
            result += "步速:" + data.speed.toFixed(2)+"米/秒"
          }
          if (data.rhythm!=undefined) {
            result += "    步频:" + data.rhythm+"次/分"
          }
          if (data.steplenght!=undefined) {
            result += "    步长:" + data.steplenght+"厘米"
          }
          if (data.symm!=undefined) {
            result += "    对称性:" + data.symm +"%"
          }
          return result;
        }
      } catch (error) {

      }
      return result;
    },
    // 行单击选中
    row_click: function (row, column, event) {
      this.$refs.table.toggleRowSelection(row);
    },
    row_dblclick: function (row, column, event) {

    },
    //关闭编辑用户弹窗
    pateint_edit_Close: function (e) {
      this.pateintEdit_dialog_visible = false;
      this.updatePatientList();
    },
    disabledDate: function (time) {
      return time.getTime() > Date.now() || time.getTime() < new Date(1800, 1, 1)
    },
    //清空搜索关键字
    btn_clearSearchKey_click: function () {
      this.search_key = "";
    },

    //添加用户
    btn_addPatient_click: function (e) {
      this.dialog_edit_userid = '';
      this.pateintEdit_dialog_visible = true;
    },
    //编辑用户
    btn_patientEdit_click: function (index, row) {
      this.dialog_edit_userid = row.ID;
      this.pateintEdit_dialog_visible = true;
    },

    //删除用户
    btn_userDelete_click: function (index, row) {
      if (this.$root.currentPatient != null && this.$root.currentPatient.ID == row.ID) {
        this.$message({
          message: "当前登录用户无法删除",
          type: 'error'
        });
        return
      }

      this.$confirm('确认删除 [' + row.Name + '] 吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        CSharp_DB.Patient_Delete(row.ID).then(res => {
          res = JSON.parse(res);
          if (res.IsSuccess) {
            this.updatePatientList();
            // app.JS_AppManager.getUserList(app.data_user_currentPage, 10, app.data_user_filterKey);
            this.$message({
              message: '删除成功',
              type: 'success'
            });
          } else {
            this.$message({
              message: res.message,
              type: 'error'
            });
          }
        });
      }).catch(() => {

      });
    },
    //切换用户
    btn_patientChange_click: function (index, row) {
      var self = this;
      CSharp_App.JS_ChangeCurrentPatient(row.ID).then(res => {
        res = JSON.parse(res);
        if (res.IsSuccess) {
          self.$message.success({
            message: '登录成功',
          });
          self.$root.currentPatient = res.data;

          localStorage.setItem("user", res.data);

        } else {
          self.$message.error({
            message: '登录失败，' + res.message,
          });
          localStorage.setItem("user", null);
        }
      });
    },
    //切换页面
    page_change: function (e) {
      this.page_index = e;
      this.updatePatientList();
    },
    //更新数据
    updatePatientList: function () {
      CSharp_DB.Patient_GetWherePage(
        this.page_index,
        this.page_size,
        this.search_key
      ).then((res) => {

        res = JSON.parse(res);
        if (res.IsSuccess) {
          this.patientList = res.data.Data;
          this.page_count = res.data.Count;
          this.page_index = res.data.PageIndex;
        }
      });
    },
    //
    getDataCacheFromCSharp: function () {
      this.updatePatientList();
    }
  },
  watch: {
    //搜索-关键字刷新，刷新数据
    search_key: function (filterKey) {
      console.log("用户搜索：" + filterKey);
      this.page_index = 1;
      this.updatePatientList();
    },
  },

  mounted: function () {
    // this.updatePatientList();
  },
  activated: function () {
    console.log("用户列表 activated");
    this.updatePatientList();
  }
};
</script>

<style>
.patientManager .content-operate {
  height: 100px;
  width: calc(100% - 80px);
  flex-shrink: 0;
  justify-content: flex-end;
}

.content-table-border {
  width: calc(100% - 80px);
  height: calc(100% - 140px);
  background-color: #FFFFFF;
  justify-content: space-between;
  border-radius: 8px;
  min-height: 750px;
  flex-shrink: 0;
}

.content-table {
  width: calc(100% - 60px);
  /* height: calc(100% - 40px); */
  flex-shrink: 0;
}

.pagination {
  position: relative;
  height: 60px;
  width: calc(100% - 60px);
  flex-shrink: 0;
  justify-content: flex-end;
}

/* 添加用户弹窗 */
.dialogForm {
  display: grid;
  grid-template-columns: 45% 45%;
  grid-template-rows: repeat(6, 71px);
  align-items: center;
  column-gap: 5%;
  justify-content: center;
}
</style>