<template>
    <div class="bt-dialog-wrapper flexC-center-lr musictrain">
        <div class="bt-dialog flexC-center-lr" style="width: 80vw;min-height:85vh;">
            <div class="header">
                <div>训练配置音乐</div>

                <div class="header-close" @click="btn_dialogClose_click">
                    <el-icon>
                        <CloseBold />
                    </el-icon>
                </div>
            </div>

            <div class="flexR-center-lr" style="width:100%;height: 100%;">

                <!-- 训练配置 -->
                <div style="width: 50%;height: 100%;">
                    <el-tree @node-click="node_click" :data="treeData" node-key="id" default-expand-all
                        :expand-on-click-node="false">
                        <template #default="{ node, data }">
                            <span class="custom-tree-node">
                                <span>{{ node.label }}</span>
                                <span>
                                    <!-- <a @click="appendMusic(node, data)" v-if="data.typee == 'difficulty'"> 添加音乐 </a> -->

                                    <a style="margin-left: 8px" @click="removeMusic(node, data)"
                                        v-if="data.typee == 'music'">
                                        删除 </a>
                                </span>
                            </span>
                        </template>
                    </el-tree>
                </div>
                <div class="line"></div>
                <!-- 音乐管理 -->
                <div class="flexC-center-lr" style="width: 50%;height: 100%;">
                    <div class="sw-operate flexR-center-lr">
                        <div class="flexR-center-lr">

                        </div>

                        <!-- 搜索 -->
                        <div class="sw-search ml20">
                            <el-input size="large" maxlength="20" v-model="search_key" placeholder="输入名称"
                                spellcheck="false">
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
                    <div class="sw-table-border flexC-center-lr">
                        <el-table ref="table" :data="list" class="sw-table" :stripe="true" :border="false">
                            <el-table-column type="index" width="70" label='序号' align="center" />
                            <el-table-column prop="Name" label="名称" width="180" align="center" />
                            <el-table-column prop="BPM" label="BPM" width="100" align="center" />
                            <!-- <el-table-column prop="BPM" label="歌曲信息" width="220" align="center" /> -->
                            <el-table-column prop="ShoeSize" label="操作" align="center" min-width="100">
                                <template #default="scope">

                                    <el-button @click="btn_tianjia_click(scope.$index, scope.row)">添加
                                    </el-button>
                                </template>
                            </el-table-column>
                        </el-table>

                        <!-- 分页 -->
                        <el-pagination class="sw-pagination mt10" @current-change="page_change" :current-page="page_index"
                            background layout="prev, pager, next,total" :total="page_count" :page-size="page_size">
                        </el-pagination>
                    </div>
                </div>

            </div>
        </div>

        <!-- 曲库管理 -->
        <bt-dialog-musicedit v-if="add_dialog_visible" style="position:absolute;" @close="musicedit_Close"
            :musicid="dialog_edit_musicid">
        </bt-dialog-musicedit>
    </div>
</template>

<script>
module.exports = {
    data() {
        return {
            page_index: 1,
            page_count: 0,
            page_size: 10,
            search_key: '',
            list: [],
            add_dialog_visible: false,
            dialog_edit_musicid: 0,
            treeData: [],
            currentTreeNode: null
        }
    },
    watch: {
        //搜索-关键字刷新，刷新数据
        search_key: function (filterKey) {
            this.page_index = 1;
            this.updateList();
        },
    },

    methods: {
        //关联音乐
        appendMusic: function (node, data) {


        },
        //取消关联
        removeMusic: function (node, data) {
            CSharp_DB.TrainMusic_Delete(data.parentid).then(res=>{
                res = JSON.parse(res);
                if (res.success) {
                    this.updateTree();
                }else
                {
                    this.$message.error(res.message);
                }
            });
        },

        //选择结点
        node_click: function (a, b, c, d) {
            if (a.typee == "difficulty") {
                this.currentTreeNode = a;
            } else {
                this.currentTreeNode = null;
            }
        },

        //点击添加
        btn_tianjia_click: function (index, row) {
            if (this.currentTreeNode == null) {
                this.$message.error({ message: '请先选择一个训练之中的一个难度' });
                return;
            }
            CSharp_DB.TrainMusic_Add(this.currentTreeNode.parentid, this.currentTreeNode.id, row.ID).then(res => {
                res = JSON.parse(res);
                if (res.success) {
                    this.updateTree();
                }else
                {
                    this.$message.error(res.message);
                }
            });
        },

        //更新数据
        updateList: function () {
            CSharp_DB.Music_GetAll(
            ).then((res) => {
                res = JSON.parse(res);
                if (res.success) {
                    this.list = res.data;
                    // this.page_count = res.data.Count;
                    // this.page_index = res.data.PageIndex;
                }
            });
        },
        //更新树数据
        updateTree: function () {
            var trains = this.$root.aa_localDB.data_xlList;

            CSharp_DB.TrainMusic_GetTrainMusic(JSON.stringify(trains)).then(res => {

                res = JSON.parse(res);
                if (res.success) {
                    this.treeData = res.data;
                }
            });
        },
    },
    mounted: function () {
        this.updateList();
        this.updateTree();
    },

};
</script>

<style>
.musictrain .line {
    border-left: 1px solid #666;
    height: calc(100% - 30px);
}

.musictrain .sw-operate {
    width: 100%;
    height: 70px;
    margin-right: 30px;
    justify-content: flex-end;
}

.musictrain .sw-search {
    width: 400px;

}

.musictrain .sw-table-border {
    width: calc(100% - 80px);
    height: calc(100% - 140px);
    background-color: #FFFFFF;
    justify-content: space-between;
    border-radius: 8px;
    min-height: 500px;
    flex-shrink: 0;
}

.musictrain .sw-table {
    width: calc(100% - 60px);
    flex-shrink: 0;
}

.musictrain .sw-pagination {
    position: relative;
    height: 60px;
    width: calc(100% - 0px);
    flex-shrink: 0;
    justify-content: flex-end;
}
</style>
