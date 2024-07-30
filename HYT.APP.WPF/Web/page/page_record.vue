<template>
  <div class="page flexC-center-lr">
    <div class="page-head flexR-center-lr">
      <i class="icon-dw ml40"></i>
      <div class="ml10">历史记录</div>
    </div>

    <div class="page-content flexC-center-lr">
      <div class="tab-wrapper flexR-center-lr">
        <div :class="activeTabName == '评估记录' ? 'tab-item active' : 'tab-item'" @click="tab_item_click('评估记录')">
          评估记录
        </div>
        <div :class="activeTabName == '训练记录' ? 'tab-item active' : 'tab-item'" @click="tab_item_click('训练记录')"
          style="margin-left: 20px">
          训练记录
        </div>
        <div :class="activeTabName == '游戏记录' ? 'tab-item active' : 'tab-item'" @click="tab_item_click('游戏记录')"
          style="margin-left: 20px">
          游戏记录
        </div>
      </div>
      <div class="tab-content-wrapper">
        <!-- 评估 -->
        <div class="tab-content1 flexC-center-lr" v-if="activeTabName == '评估记录'">
          <!-- 搜索-->
          <div class="content-operate flexR-center-lr">
            <div style="width: 500px" class="ml40 flexR-center-lr">
              <span class="selectDateTitle">选择日期：</span>
              <el-date-picker v-model="assess_search_date" type="daterange" unlink-panels range-separator="至"
                start-placeholder="开始日期" end-placeholder="结束日期" size="large" :editable="false" @change="dateChange(1)" />
            </div>
          </div>

          <!-- 表格 -->
          <div class="table-border flexC-center-lr">
            <el-table ref="tableAssess" :data="assess_list" class="content-table" :stripe="true" :border="false">

              <el-table-column type="index" width="60" label="序号" align="center" />
              <el-table-column prop="PingCeName" label="评估类型" width="250" align="center">
              </el-table-column>
              <el-table-column prop="StartTime" label="开始时间" width="250" align="center">
                <template #default="scope">
                  <span style="margin-left: 10px">{{
                    scope.row.StartTime ? new Date(scope.row.StartTime).Format() : ""
                  }}</span>
                </template>
              </el-table-column>
              <el-table-column prop="EndTime" label="结束时间" width="250" align="center">
                <template #default="scope">
                  <span style="margin-left: 10px">{{
                    scope.row.EndTime ? new Date(scope.row.EndTime).Format() : ""
                  }}</span>
                </template>
              </el-table-column>
              <el-table-column prop="Result" label="结果" width="550" align="center">
                <template #default="scope">
                  <span style="margin: 0;">步速:{{ Math.round(JSON.parse(scope.row.Result).Speed_Average * 100) / 100
                  }}米/秒</span>
                     <span style="margin: 0 0 0 20px;">步频:{{ Math.round(JSON.parse(scope.row.Result).Rhythm_Average * 100) / 100
                  }}次/分</span>
                  <span style="margin: 0 0 0 20px;">对称性:{{ Math.round(JSON.parse(scope.row.Result).Symm_Average * 100) / 100
                  }}%</span>
               
                 
                </template>
              </el-table-column>
              <el-table-column prop="AssessID" label="操作" align="center">
                <template #default="scope">
                  <el-button size="large" @click="btn_assessDetail_click(scope.$index, scope.row)">详情
                  </el-button>
                  <span class="ml10">|</span>

                  <el-button size="large" type="danger" style="margin-left: 10px"
                    @click="btn_assessDelete_click(scope.$index, scope.row)">删除
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <!-- 分页 -->
            <el-pagination class="pagination mt10" @current-change="assess_page_change" :current-page="assess_page_index"
              background layout="prev, pager, next,total" :total="assess_page_count" :page-size="assess_page_size">
            </el-pagination>
          </div>
        </div>

        <!-- 训练 -->
        <div class="tab-content2 flexC-center-lr" v-if="activeTabName == '训练记录'">
          <!-- 搜索-->
          <div class="content-operate flexR-center-lr">
            <div style="width: 500px" class="ml40 flexR-center-lr">
              <span class="selectDateTitle">选择日期：</span>
              <el-date-picker v-model="train_search_date" type="daterange" unlink-panels range-separator="至"
                start-placeholder="开始日期" end-placeholder="结束日期" size="large" :editable="false" @change="dateChange(2)" />
            </div>
          </div>

          <!-- 表格 -->
          <div class="table-border flexC-center-lr">
            <el-table ref="tableTrain" :data="train_list" class="content-table" :stripe="true" :border="false">

              <el-table-column type="index" prop="SN" label="序号" width="60" align="center" />
              <el-table-column prop="TrainName" label="训练名称" width="150" align="center">

              </el-table-column>
              <el-table-column prop="StartTime" label="开始时间" width="220" align="center">
                <template #default="scope">
                  <span style="margin-left: 10px">{{
                    scope.row.StartTime ? new Date(scope.row.StartTime).Format() : ""
                  }}</span>
                </template>
              </el-table-column>
              <el-table-column prop="EndTime" label="结束时间" width="220" align="center">
                <template #default="scope">
                  <span style="margin-left: 10px">{{
                    scope.row.EndTime ? new Date(scope.row.EndTime).Format() : ""
                  }}</span>
                </template>
              </el-table-column>
              <!-- <el-table-column prop="SetTime" label="设置时长" width="150" align="center">
                <template #default="scope">
                  {{ scope.row.SetTime }}分钟
                </template>
              </el-table-column> -->
              <el-table-column prop="Result" label="结果" width="550" align="center">
                <template #default="scope">
                  <div>
                    <span style="margin: 0 5px;" v-for="result in JSON.parse(scope.row.Data).Dic_KV" :Key="result.N">
                      {{ result.N }}：{{ result.V }};
                    </span>
                  </div>
                  <!-- <div>
                    <span style="margin: 0 5px;">对称性:{{ Math.round(JSON.parse(scope.row.Record).Symm_Average * 100) / 100 }}%;</span>
                    <span style="margin: 0 5px;">步频:{{ Math.round(JSON.parse(scope.row.Record).Rhythm_Average * 100) / 100 }}次/分钟;</span>
                    <span style="margin: 0 5px;">步速:{{ Math.round(JSON.parse(scope.row.Record).Speed_Average * 100) / 100 }}米/秒</span>
                  </div> -->
                </template>
              </el-table-column>
              <el-table-column prop="TrainName" label="操作" align="center">
                <template #default="scope">
                  <el-button size="large" @click="btn_trainDetail_click(scope.$index, scope.row)">详情
                  </el-button>
                  <span class="ml10">|</span>
                  <el-button size="large" type="danger" style="margin-left: 10px"
                    @click="btn_trainDelete_click(scope.$index, scope.row)">删除
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <!-- 分页 -->
            <el-pagination class="pagination mt10" @current-change="train_page_change" :current-page="train_page_index"
              background layout="prev, pager, next,total" :total="train_page_count" :page-size="train_page_size">
            </el-pagination>
          </div>
        </div>

        <!-- 游戏 -->
        <div class="tab-content3 flexC-center-lr" v-if="activeTabName == '游戏记录'">
          <!-- 搜索-->
          <div class="content-operate flexR-center-lr">
            <div style="width: 500px" class="ml40 flexR-center-lr">
              <span class="selectDateTitle">选择日期：</span>
              <el-date-picker v-model="game_search_date" type="daterange" :unlink-panels="true" range-separator="至"
                :clearable="true" start-placeholder="开始日期" end-placeholder="结束日期" size="large" :editable="false"
                @change="dateChange(3)" />
            </div>
          </div>

          <!-- 表格 -->
          <div class="table-border flexC-center-lr">
            <el-table ref="tableGame" :data="gamelist" class="content-table" :stripe="true" :border="false">

              <el-table-column type="index" prop="ID" label="序号" width="60" align="center" />
              <el-table-column prop="TrainName" label="游戏名称" width="150" align="center">

              </el-table-column>
              <el-table-column prop="StartTime" label="开始时间" width="220" align="center">
                <template #default="scope">
                  <span style="margin-left: 10px">{{
                    scope.row.StartTime ? new Date(scope.row.StartTime).Format() : ""
                  }}</span>
                </template>
              </el-table-column>
              <el-table-column prop="EndTime" label="结束时间" width="220" align="center">
                <template #default="scope">
                  <span style="margin-left: 10px">{{
                    scope.row.EndTime ? new Date(scope.row.EndTime).Format() : ""
                  }}</span>
                </template>
              </el-table-column>


              <el-table-column prop="Score" label="分数" width="150" align="center">

              </el-table-column>
              <el-table-column prop="AccuracyRate" label="准确率（%）" width="150" align="center">

              </el-table-column>
              <el-table-column prop="TrainName" label="操作" align="center">
                <template #default="scope">
                  <el-button size="large" @click="btn_gameDetail_click(scope.$index, scope.row)">详情
                  </el-button>
                  <span class="ml10">|</span>
                  <el-button size="large" type="danger" style="margin-left: 10px"
                    @click="btn_gameDelete_click(scope.$index, scope.row)">删除
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <!-- 分页 -->
            <el-pagination class="pagination mt10" @current-change="game_page_change" :current-page="game_page_index"
              background layout="prev, pager, next,total" :total="game_page_count" :page-size="game_page_size">
            </el-pagination>
          </div>
        </div>
      </div>
    </div>


  </div>
</template>

<script>
// debugger
module.exports = {
  setup() { },
  data: function () {
    return {
      activeTabName: "评估记录",

      //评估
      assess_list: [],
      assess_page_index: 1,
      assess_page_count: 0,
      assess_page_size: 10,
      assess_search_date: [],

      //训练
      train_list: [],
      train_page_index: 1,
      train_page_count: 0,
      train_page_size: 10,
      train_search_date: [],

      //游戏
      gamelist: [],
      game_page_index: 1,
      game_page_count: 0,
      game_page_size: 10,
      game_search_date: [],
    };
  },
  methods: {
    //分析结果
    analysisResult: function (result) {
      debugger
      var obj = JSON.parse(result);
      return '总步数：' + obj.StepCount;
    },
    //切换类型
    tab_item_click: function (name) {
      this.activeTabName = name;
      switch (this.activeTabName) {
        case "评估记录":
          this.assess_updateList();
          break;
        case "训练记录":
          this.train_updateList();
          break;
        default:
          this.game_updateList();
          break;
      }
    },
    //选择日期：同样的日期 不会触发
    dateChange: function (e) {
      if (e == 1) {
        this.assess_updateList();
      }
      if (e == 2) {
        this.train_updateList();
      }
      if (e == 3) {
        this.game_updateList();
      }
    },

    //评估列表翻页
    assess_page_change: function (e) {
      this.assess_page_index = e;
      this.assess_updateList();
    },
    //查看评估详情
    btn_assessDetail_click: function (e, data) {
      window.m_assessData = data; //从评估记录进入
      this.$root.assess_report_id = data.ID;
      this.$root.dialog_assessreport_visible = true;
    },
    //删除评估
    btn_assessDelete_click: function (index, row) {
      this.$confirm('确认删除此记录吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        CSharp_DB.AssessRecord_DeleteByID(row.ID).then(res => {
          res = JSON.parse(res);
          if (res.IsSuccess) {
            this.assess_updateList();
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

    //训练列表翻页
    train_page_change: function (e) {
      this.train_page_index = e;
      this.train_updateList();
    },
    //查看训练详情
    btn_trainDetail_click: function (e, data) {
      this.$root.train_report_id = data.ID;
      this.$root.dialog_trainreport_visible = true;
    },
    //删除训练
    btn_trainDelete_click: function (index, row) {
      this.$confirm('确认删除此记录吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        CSharp_DB.TrainRecord_DeleteByID(row.ID).then(res => {
          res = JSON.parse(res);
          if (res.IsSuccess) {
            this.train_updateList();
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


    //游戏列表翻页
    game_page_change: function (e) {
      this.game_page_index = e;
      this.game_updateList();
    },
    //查看游戏详情
    btn_gameDetail_click: function (e, data) {
      this.$root.train_report_id = data.ID;
      this.$root.dialog_trainreport_visible = true;
    },
    //删除游戏
    btn_gameDelete_click: function (index, row) {
      this.$confirm('确认删除此记录吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        CSharp_DB.TrainRecord_DeleteByID(row.ID).then(res => {
          res = JSON.parse(res);
          if (res.IsSuccess) {
            this.game_updateList();
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





    //评估更新数据
    assess_updateList: function () {
      let patientID = "";
      if (this.$root.currentPatient) {
        patientID = this.$root.currentPatient.ID;
      } else {
        return;
      }
      if (this.assess_search_date == null) {
        this.assess_search_date = [];
      }

      CSharp_DB.AssessRecord_GetPage(
        patientID,
        this.assess_page_index,
        this.assess_page_size,
        "StartTime DESC",
        this.assess_search_date[0],
        this.assess_search_date[1]
      ).then((res) => {
        res = JSON.parse(res);
        if (res.IsSuccess) {
          this.assess_list = res.data.Data;
          this.assess_page_count = res.data.Count;
          this.assess_page_index = res.data.PageIndex;
        }
      });
    },
    //训练更新数据
    train_updateList: function () {
      let patientID = 0;
      if (this.$root.currentPatient) {
        patientID = this.$root.currentPatient.ID;
      } else {
        return;
      }
      if (this.train_search_date == null) {
        this.train_search_date = [];
      }
      CSharp_DB.TrainRecord_GetPage(
        patientID,
        this.train_page_index,
        this.train_page_size,
        "StartTime DESC",
        this.train_search_date[0],
        this.train_search_date[1],
        1
      ).then((res) => {
        res = JSON.parse(res);
        if (res.IsSuccess) {
          this.train_list = res.data.Data;
          this.train_page_count = res.data.Count;
          this.train_page_index = res.data.PageIndex;
        }
      });
    },
    //游戏更新数据
    game_updateList: function () {
      let patientID = 0;
      if (this.$root.currentPatient) {
        patientID = this.$root.currentPatient.ID;
      } else {
        return;
      }
      if (this.game_search_date == null) {
        this.game_search_date = [];
      }
      CSharp_DB.TrainRecord_GetPage(
        patientID,
        this.game_page_index,
        this.game_page_size,
        "StartTime DESC",
        this.game_search_date[0],
        this.game_search_date[1],
        2
      ).then((res) => {
        res = JSON.parse(res);
        if (res.IsSuccess) {
          this.gamelist = res.data.Data;
          this.game_page_count = res.data.Count;
          this.game_page_index = res.data.PageIndex;
        }
      });
    },

  },
  watch: {

  },

  activated: function () {
    console.log("page_record activated");
    this.assess_updateList();
    this.train_updateList();
    this.game_updateList();
  },
};
</script>


<style>
.selectDateTitle {

  font-size: 16px;
}

.tab-content-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
}

.tab-content1，.tab-content2 ，.tab-content3 {
  position: absolute;
  left: 0px;
  top: 0px;
  width: 100%;
  height: 100%;
}

.tab-wrapper {
  width: calc(100% - 80px);
  height: 60px;
  box-sizing: border-box;
  border-bottom: 2px solid #e4e7ed;
  justify-content: center;
  flex-shrink: 0;
}

.tab-item {
  width: 100px;
  height: 60px;
  line-height: 60px;
  font-size: 22px;
  text-align: center;
}

.tab-item.active {
  box-sizing: border-box;
  border-bottom: 2px solid #1252f6;
}

.content-operate {
  height: 100px;
  width: calc(100% - 80px);
  flex-shrink: 0;
  justify-content: flex-end;
}

.table-border {
  width: calc(100% - 80px);
  height: calc(100% - 140px);
  background-color: #ffffff;
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

/* 弹窗 */
.dialogForm {
  display: grid;
  grid-template-columns: 45% 45%;
  grid-template-rows: repeat(6, 71px);
  align-items: center;
  column-gap: 5%;
  justify-content: center;
}
</style>