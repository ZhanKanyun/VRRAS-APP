<template>
    <div class="bt-dialog-wrapper flexC-center-lr musicmananger">
        <div class="bt-dialog flexC-center-lr" style="width: 80vw;min-height:85vh;">
            <div class="header">
                <div>曲库管理</div>

                <div class="header-close" @click="btn_dialogClose_click">
                    <el-icon>
                        <CloseBold />
                    </el-icon>
                </div>
            </div>

            <div class="flexR-center-lr" style="width:100%;height: 100%;">
                <!-- 音乐管理 -->
                <div class="flexC-center-lr" style="width: 57%;height: 100%;">
                    <div class="sw-operate flexR-center-lr">

                        <div class="musicLibrary-title">曲库</div>

                        <div class="flexR-center-lr">
                            <el-button size="large" type="primary" @click="btn_addMusic_click">
                                <el-icon class="el-icon--right mr10">
                                    <Plus />
                                </el-icon> 上传音乐
                            </el-button>
                        </div>

                        <!-- 搜索 -->
                        <div class="sw-search ml20">
                            <el-input size="large" maxlength="20" v-model="search_key" placeholder="输入音乐名称搜索"
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
                            <el-table-column prop="Name" label="音乐名称" width="180" align="center" />
                            <el-table-column prop="BPM" label="BPM" width="80" align="center" />

                            <el-table-column prop="" label="" width="48" align="center">
                                <template #default="scope">
                                    <el-image v-if="currentTestMusic != null && scope.row.ID == currentTestMusic.ID"
                                        style="width: 45px; height: 40px;top: 5px;" src="../Web/resource/img/playing.gif"
                                        fit="cover" />
                                </template>
                            </el-table-column>

                            <!-- <el-table-column prop="BPM" label="歌曲信息" width="220" align="center" /> -->
                            <el-table-column prop="ShoeSize" label="操作" align="center" min-width="100">
                                <template #default="scope">
                                    <el-button size="large"
                                        v-if="currentTestMusic != null && scope.row.ID == currentTestMusic.ID"
                                        @click="btn_stopTest_click(scope.$index, scope.row)">停止
                                    </el-button>
                                    <el-button size="large"
                                        v-if="currentTestMusic == null || scope.row.ID != currentTestMusic.ID"
                                        @click="btn_startTest_click(scope.$index, scope.row)">试听
                                    </el-button>
                                    <span class="ml10">|</span>
                                    <el-button size="large" class="ml10"
                                        :disabled="currentTestMusic != null && scope.row.ID == currentTestMusic.ID"
                                        @click="btn_edit_click(scope.$index, scope.row)">编辑
                                    </el-button>
                                    <span class="ml10">|</span>
                                    <el-button size="large" class="ml10"
                                        :disabled="currentTestMusic != null && scope.row.ID == currentTestMusic.ID"
                                        type="danger" @click="btn_delete_click(scope.$index, scope.row)">删除
                                    </el-button>
                                    <span class="ml10">|</span>
                                    <el-button size="large" class="ml10"
                                        @click="btn_apply_click(scope.$index, scope.row)">添加到
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

                <div class="line"></div>

                <!-- 训练配置 -->
                <div class="trainConfig flexC">
                    <div class="trainConfig-title">训练和游戏配置</div>
                    <div class="trainConfig-hint">操作说明：先选中训练或游戏，然后在音乐列表中点击“添加到”按钮。</div>
                    <el-tree class="trainConfig-tree" @node-click="treenode_click" :data="treeData" node-key="id"
                        :check-on-click-node="true" :highlight-current="true" accordion :expand-on-click-node="true"
                        @node-expand="treenode_expand">
                        <template #default="{ node, data }">
                            <span class="flexR-center-lr mytreeNode">

                                <svg v-if="data.typee == 'train'" aria-hidden="true" style="width: 18px;height: 18px;">
                                    <use xlink:href="#icon-xunlian"></use>
                                </svg>

                                <svg v-if="data.typee == 'music'" aria-hidden="true" style="width: 18px;height: 18px;">
                                    <use xlink:href="#icon-yinle"></use>
                                </svg>

                                <span style="margin-left: 5px;">{{ node.label }}</span>
                                <span v-if="data.typee == 'train'" style="margin-left: 5px;">({{ data.children.length
                                }})</span>
                                <span>
                                    <a style="margin-left: 8px;color: orangered;" @click="removeMusic(node, data)"
                                        v-if="data.typee == 'music'">
                                        删除 </a>
                                </span>
                            </span>
                        </template>
                    </el-tree>
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
            currentTreeNode: null,
            currentnodekey: null,
            currentTestMusic: null,//当前试听音乐
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
        //训练配置：选择结点
        treenode_click: function (a, b, c, d) {
            console.log(a);
            if (a.typee == "train") {
                this.currentTreeNode = a;
            }
            else if (a.typee == "music") {
                // this.currentTreeNode = a;
            }
            else {
                this.currentTreeNode = null;
            }
        },
        treenode_expand: function (a, b, c, d) {
            console.log(a);

            if (a.typee == "train") {
                this.currentTreeNode = a;
                //TODO
            }
            else {
                this.currentTreeNode = null;
            }
        },
        //试听音乐
        btn_startTest_click: function (index, row) {
            if (this.currentTestMusic == null || this.currentTestMusic.ID != row.ID) {
                CSharp_DB.Music_StartTest(row.ID).then(res => {
                    res = JSON.parse(res);
                    if (res.success) {
                        this.currentTestMusic = row;
                    } else {
                        this.$message.error(res.message);
                    }
                });
            }
        },
        //停止试听
        btn_stopTest_click: function (index, row) {
            if (this.currentTestMusic != null) {
                CSharp_DB.Music_StopTest().then(res => {
                    res = JSON.parse(res);
                    if (res.success) {
                        this.currentTestMusic = null;
                    } else {
                        this.$message.error(res.message);
                    }
                });
            }
        },
        //训练配置：关联音乐
        btn_apply_click: function (index, row) {
            if (this.currentTreeNode == null) {
                this.$message.error({ message: '请先选中训练' });
                return;
            }
            CSharp_DB.TrainMusic_Add(this.currentTreeNode.id, row.ID).then(res => {
                res = JSON.parse(res);
                if (res.success) {

                    var newData = res.data;

                    this.currentTreeNode.children.push({
                        id: newData.MusicID,
                        label: newData.MusicName,
                        parentid: newData.ID,
                        typee: 'music'
                    });
                } else {
                    this.$message.error(res.message);
                }
            });
        },
        //训练配置：取消关联音乐
        removeMusic: function (node, data) {
            CSharp_DB.TrainMusic_Delete(data.parentid).then(res => {
                res = JSON.parse(res);
                if (res.success) {
                    var trainMusic = res.data;
                    var train = this.treeData.find(o => o.id == trainMusic.TrainID);

                    train.children.remove(o => o.id == trainMusic.MusicID);
                } else {
                    this.$message.error(res.message);
                }
            });
        },
        //添加音乐
        btn_addMusic_click: function (e) {
            this.dialog_edit_musicid = "";
            this.add_dialog_visible = true;
        },
        //删除音乐
        btn_delete_click: function (index, row) {

            this.$confirm('确认删除 [' + row.Name + '] 吗?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                CSharp_DB.Music_Delete(row.ID).then(res => {
                    res = JSON.parse(res);
                    if (res.IsSuccess) {
                        this.updateList();
                        this.updateTree();
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
        //编辑音乐打开
        btn_edit_click: function (index, row) {
            this.dialog_edit_musicid = row.ID;
            this.add_dialog_visible = true;
        },
        //编辑音乐关闭
        musicedit_Close: function (e) {
            this.add_dialog_visible = false;
            if (e) {
                this.updateList();
                this.updateTree();
            }

        },
        page_change: function (e) {
            this.page_index = e;
            this.updateList();
        },
        btn_dialogClose_click: function (e) {

            if (this.currentTestMusic != null) {
                CSharp_DB.Music_StopTest().then(res => {
                    res = JSON.parse(res);
                    if (res.success) {
                        this.currentTestMusic = null;
                    } else {
                        this.$message.error(res.message);
                    }
                });
            }

            this.$emit('close', false)
        },
        btn_clearSearchKey_click: function (e) {
            this.search_key = "";
        },
        //更新数据
        updateList: function () {
            CSharp_DB.Music_GetWherePage(
                this.page_index,
                this.page_size,
                this.search_key
            ).then((res) => {
                res = JSON.parse(res);
                if (res.success) {
                    this.list = res.data.Data;
                    this.page_count = res.data.Count;
                    this.page_index = res.data.PageIndex;
                }
            });

            //获取当前试听音乐
            CSharp_DB.Music_GetTest().then((res) => {
                res = JSON.parse(res);
                if (res.success) {
                    this.currentTestMusic = res.data;
                } else {
                    this.currentTestMusic = null;
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
                    this.currentnodekey = this.treeData[0];
                    this.currentTreeNode = null;
                }
            });
        },
        //修改 更新树数据其中一首音乐
        updateTreeOne: function (musicID) {
            var trains = this.$root.aa_localDB.data_xlList;
            CSharp_DB.Music_GetByID(musicID).then(res => {
                res = JSON.parse(res);
                
                if (res.success) {
                    var a = this.treeData.find(o => o.ID == musicID)
                    a.Name = res.data.Name;
                }
            });
        },
    },
    mounted: function () {
        this.updateList();
        this.updateTree();
    },

    activated: function () {
        this.updateList();
    },

};
</script>

<style>
.mytreeNode {
    font-size: 16px;
}

.trainConfig {
    width: 40%;
    height: 100%;
    font-size: 16px;
}

.trainConfig-title {
    font-size: 24px;
    margin-top: 20px;
    margin-left: 20px;
}

.musicLibrary-title {
    font-size: 24px;
    margin-right: 300px;
}

.trainConfig-hint {
    margin-top: 5px;
    margin-left: 21px;
}

.trainConfig-tree {
    margin-top: 10px;
    margin-left: 10px;
    height: 690px;
    overflow-y: scroll;
}

.musicmananger .line {
    border-left: 1px solid #666;
    height: calc(100% - 30px);
}

.musicmananger .sw-operate {
    width: calc(100% - 20px);
    height: 70px;
    margin-right: 30px;
    justify-content: flex-end;
}

.musicmananger .sw-search {
    width: 300px;

}

.musicmananger .sw-table-border {
    width: calc(100% - 30px);
    height: calc(100% - 140px);
    background-color: #FFFFFF;
    justify-content: space-between;
    border-radius: 8px;
    min-height: 500px;
    flex-shrink: 0;
}

.musicmananger .sw-table {
    width: calc(100% - 40px);
    flex-shrink: 0;
}

.musicmananger .sw-pagination {
    position: relative;
    height: 60px;
    width: calc(100% - 0px);
    flex-shrink: 0;
    justify-content: flex-end;
}

.musicmananger .el-tree--highlight-current .el-tree-node.is-current>.el-tree-node__content {
    background-color: var(--el-color-primary-light-3);
}
</style>
