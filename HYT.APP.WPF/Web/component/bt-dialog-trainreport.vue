<template>
    <div id="trainreport" class="bt-dialog-wrapper flexC-center-lr trainreport">
        <div id="reportDialog" class="bt-dialog flexC-center-lr" style="width: 762px; min-height: 1077px">
            <div class="header">
                <div>{{ reportTitle }}</div>
                <el-button id="btnPrint_Report" class="btnPrint_Report" @click="btn_print_click">打印</el-button>
                <el-button id="btnExit_Report" class="btnExit_Report" @click="btn_dialogClose_click">退出</el-button>
            </div>
            <div class="bt-dialog-scorll" id="dialogScorll">
                <div class="bt-dialog-content flexC-center-lr">
                    <!-- 用户信息 -->
                    <div v-if="userinfo != null && trainConfig != null" style="width: calc(100% - 44px); margin-top: 10px">
                        <el-descriptions title="用户信息" :column="3" border>
                            <el-descriptions-item :width="130" label="姓名">
                                {{ userinfo.Name }}
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="性别">
                                {{ userinfo.Sex == 0 ? "男" : "女" }}
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="年龄">
                                {{ userinfo.Birthday ? new Date().getFullYear() - new Date(userinfo.Birthday).getFullYear()
                                    : "未填写"
                                }}
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="病症">
                                {{ userinfo.DiseaseType }}
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="项目">{{ trainConfig.name }}</el-descriptions-item>
                        </el-descriptions>
                    </div>
                    <!-- 训练结果 -->
                    <div v-if="resultData != null" style="width: calc(100% - 44px); margin-top: 10px">
                        <el-descriptions title="训练结果" :column="3" border>
                            <el-descriptions-item :width="130" v-for="result in resultData.Dic_KV" :Key="result.N"
                                :label="result.N">{{
                                    result.V
                                }}
                            </el-descriptions-item>
                        </el-descriptions>
                    </div>
                    <div v-if="dataResult != null" style="width: calc(100% - 44px); margin-top: 10px">
                        <el-descriptions title="训练数据" :column="3" border>
                            <el-descriptions-item :width="130" label="总步数">
                                {{ Math.round(dataResult.StepCount * 100) / 100 }}
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均步态周期">
                                {{ Math.round(dataResult.T_Average * 100) / 100 }}秒
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均速度">
                                {{ Math.round(dataResult.Speed_Average * 100) / 100 }}米/秒
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均步频">
                                {{ Math.round(dataResult.Rhythm_Average * 100) / 100 }}次/分钟
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均对称性">
                                {{ Math.round(dataResult.Symm_Average * 100) / 100 }}%
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均步时">
                                {{ Math.round(dataResult.StepTime_Average * 100) / 100 }}毫秒
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均步宽">
                                {{ Math.round(dataResult.StepWidth_Average * 100) / 100 }}厘米
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均跨步长">
                                {{ Math.round(dataResult.BF_Average * 100) / 100 }}厘米
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="平均步长">
                                {{ Math.round(dataResult.StepLength_Average * 100) / 100 }}厘米
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="左脚平均步长">
                                {{ Math.round(dataResult.StepLengthL_Average * 100) / 100 }}厘米
                            </el-descriptions-item>
                            <el-descriptions-item :width="130" label="右脚平均步长">
                                {{ Math.round(dataResult.StepLengthR_Average * 100) / 100 }}厘米
                            </el-descriptions-item>
                        </el-descriptions>
                    </div>

                    <!-- <template v-if="train_Data_echart != null">
                        <div v-show="dataResult != null" style="width: calc(100% - 40px);padding-bottom: 5px;"
                            v-for="echart in train_Data_echart" :key="echart.id">
                            <v-chart style="width: 710px;height:400px;" :ref="'ec' + echart.id" />
                        </div>
                    </template> -->

                    <div v-show="dataResult != null && !isGameReport" style="width: calc(100% - 44px);padding-bottom: 5px;">
                        <v-chart class="eachar" style="width: 710px;height:475px;" ref="zheXianResult1" />
                        <v-chart class="eachar" style="width: 710px;height:475px;" ref="zheXianResult2" />
                    </div>
                    <div v-show="dataResult != null && isGameReport"
                        style="width: calc(100% - 44px);padding-bottom: 5px;margin-top: 30px">
                        <v-chart style="width: 710px;height:450px;" ref="zheXianResult" />
                    </div>
                </div>


                <!-- <div style="height:100px"></div> -->
            </div>
        </div>
    </div>
</template>

<script>

module.exports = {
    data() {
        return {
            userinfo: null,
            train_Data: null,
            dataResult: null,
            resultData: null,
            train_Data_echart: null,

            trainConfig: null,
            printTime: "",
            reportTitle: "训练详情",

            isGameReport: false
        };
    },
    props: ["reportid"],
    watch: {
        // train_Data_echart: function (val, old) {
        //     this.$nextTick(function () {
        //         val.forEach((echart) => {
        //             this.$refs["ec" + echart.id].setOption(echart);
        //         });
        //     });
        // },
    },

    methods: {

        btn_dialogClose_click: function () {
            this.$emit("close", false);
        },

        btn_print_click: function () {

            document.getElementById("dialogScorll").style.overflowY = "visible";
            document.getElementById("btnPrint_Report").style.opacity = 0;
            document.getElementById("btnExit_Report").style.opacity = 0;

            try {
                if (window.aa_config.isDebug) {
                    document.getElementById("alpha_version").style.opacity = 0;
                }
            } catch (error) {

            }



            document.getElementById("currentPatientPanel").style.display = "none";
            var app = document.getElementById("app");
            app.style.width = "100vw";

            var reportDialog = document.getElementById("reportDialog")
            var old = window.getComputedStyle(reportDialog, null).boxShadow;
            reportDialog.style.boxShadow = "rgb(169, 169, 169) 0px 0px 0px 0px";


            setTimeout(() => {
                window.print();
                document.getElementById("btnPrint_Report").style.opacity = 1;
                document.getElementById("btnExit_Report").style.opacity = 1;

                try {
                    if (window.aa_config.isDebug) {
                        document.getElementById("alpha_version").style.opacity = 1;
                    }
                } catch (error) {

                }

                document.getElementById("dialogScorll").style.overflowY = "scroll";
                document.getElementById("reportDialog").style.boxShadow = old;

                document.getElementById("currentPatientPanel").style.display = "flex";
                app.style.width = "100vw";
            }, 50);
        },

        getTrainData: function () {
            CSharp_DB.TrainRecord_GetByID(this.reportid).then((res) => {
                res = JSON.parse(res);
                if (res.IsSuccess) {

                    var obj = res.data;
                    this.train_Data = obj;
                    this.dataResult = JSON.parse(res.data.Record);
                    this.resultData = JSON.parse(res.data.Data);

                    this.trainConfig = window.aa_localDB.getTrainByid(this.train_Data.TrainID);

                    var new_ListEchart = this.resultData.ListEchart;
                    //配置图表
                    //防止id一样
                    new_ListEchart.forEach(echart => {
                        echart.id += new_ListEchart.length;
                        echart.tooltip = {
                            trigger: "item"
                        };
                    });

                    this.train_Data_echart = new_ListEchart;


                    if (this.train_Data.TrainID == 100001)
                        this.finishEcharts();


                    if (this.train_Data.TrainID > 200000) {
                        this.reportTitle = "游戏详情";
                        this.isGameReport = true;
                        this.TrainEcharts();
                    }
                }
            });
        },
        finishEcharts: function () {
            var result = this.dataResult;


            var xData = [];
            var xData_l = [];
            var xData_r = [];
            for (let l = 0; l < result.StepCountL; l++) {
                xData_l.push(l + 1);
            }
            for (let r = 0; r < result.StepCountR; r++) {
                xData_r.push(r + 1);
            }

            var bc_l = [];
            var bc_r = [];
            var bs_l = [];
            var bs_r = [];

            var bp = [];
            var dcx = [];

            for (let index = 0; index < result.Steps.length; index++) {
                const element = result.Steps[index];
                xData.push(index + 1);
                bp.push(element.BP);
                dcx.push(element.DCX);

                if (element.F == 1) {
                    bc_l.push(element.BC);
                    bs_l.push(element.BS);

                } else {
                    bc_r.push(element.BC);
                    bs_r.push(element.BS);
                }
            }

            const zhexian = this.$refs.zheXianResult1;
            if (zhexian) {
                var option = {
                    tooltip: {
                        trigger: "axis",
                    },
                    title: [
                        { left: "20%", top: "3%", text: "左脚步长变化" },
                        { left: "65%", top: "3%", text: "右脚步长变化" },

                        { left: "20%", top: "51%", text: "左脚步时变化" },
                        { left: "65%", top: "51%", text: "右脚步时变化" },
                    ],
                    grid: [
                        { left: "12%", top: "13%", width: "35%", height: "32%" },
                        { left: "57%", top: "13%", width: "35%", height: "32%" },

                        { left: "12%", top: "61%", width: "35%", height: "32%" },
                        { left: "57%", top: "61%", width: "35%", height: "32%" },
                    ],
                    xAxis:
                        [{

                            name: '步',
                            gridIndex: 0,
                            data: xData_l,
                            boundaryGap: false,
                            axisLabel: {
                                // interval: 4
                            },
                            splitLine: {
                                show: false
                            },
                            axisLine: {
                                onZero: false
                            }
                        },
                        {
                            name: '步',
                            gridIndex: 1,
                            data: xData_r,
                            boundaryGap: false,
                            axisLabel: {
                                // interval: 4
                            },
                            splitLine: {
                                show: false
                            },
                            axisLine: {
                                onZero: false
                            }
                        },
                        {
                            name: '步',
                            gridIndex: 2,
                            data: xData_l,
                            boundaryGap: false,
                            axisLabel: {
                                // interval: 4
                            },
                            splitLine: {
                                show: false
                            },
                            axisLine: {
                                onZero: false
                            }
                        },
                        {
                            name: '步',
                            gridIndex: 3,
                            data: xData_r,
                            boundaryGap: false,
                            axisLabel: {
                                // interval: 4
                            },
                            splitLine: {
                                show: false
                            },
                            axisLine: {
                                onZero: false
                            }
                        }
                        ],
                    yAxis:
                        [{
                            name: '厘米',
                            gridIndex: 0,
                            axisLine: { show: true },
                            minInterval: 1,
                            axisLabel: {
                                formatter: "{value}",
                            },
                            type: "value",
                        },
                        {
                            name: '厘米',
                            gridIndex: 1,
                            axisLine: { show: true },
                            minInterval: 1,
                            axisLabel: {
                                formatter: "{value}",
                            },
                            type: "value",
                        },
                        {
                            name: '毫秒',
                            gridIndex: 2,
                            axisLine: { show: true },
                            minInterval: 1,
                            axisLabel: {
                                formatter: "{value}",
                            },
                            type: "value",
                        },
                        {
                            name: '毫秒',
                            gridIndex: 3,
                            axisLine: { show: true },
                            minInterval: 1,
                            axisLabel: {
                                formatter: "{value} ",
                            },
                            type: "value",
                        }
                        ],
                    series: [
                        {
                            name: "左脚步长",
                            data: bc_l,
                            type: "line",
                            // smooth: true,
                            showSymbol: false,
                            color: ["#91cc75"],
                            xAxisIndex: 0,
                            yAxisIndex: 0

                        },
                        {
                            name: "右脚步长",
                            data: bc_r,
                            type: "line",
                            // smooth: true,
                            showSymbol: false,
                            color: ["#91cc75"],
                            xAxisIndex: 1,
                            yAxisIndex: 1

                        },
                        {
                            name: "左脚步时",
                            data: bs_l,
                            type: "line",
                            // smooth: true,
                            showSymbol: false,
                            color: ["#91cc75"],
                            xAxisIndex: 2,
                            yAxisIndex: 2
                        },
                        {
                            name: "右脚步时",
                            data: bs_r,
                            type: "line",
                            // smooth: true,
                            showSymbol: false,
                            color: ["#91cc75"],
                            xAxisIndex: 3,
                            yAxisIndex: 3
                        }
                    ],
                };

                zhexian.setOption(option);
            }
            const zhexian2 = this.$refs.zheXianResult2;
            if (zhexian2) {
                var option = {
                    tooltip: {
                        trigger: "axis",
                    },
                    title: [
                        { left: "45%", top: "3%", text: "步频变化" },
                        { left: "45%", top: "51%", text: "对称性变化" }
                    ],
                    grid: [
                        { left: "12%", top: "13%", width: "80%", height: "32%" },
                        { left: "12%", top: "61%", width: "80%", height: "32%" },
                    ],
                    xAxis:
                        [
                            {
                                name: '步',
                                gridIndex: 0,
                                data: xData,
                                boundaryGap: false,
                                axisLabel: {
                                    // interval: 4
                                },
                                splitLine: {
                                    show: false
                                },
                                axisLine: {
                                    onZero: false
                                }
                            },
                            {
                                name: '步',
                                gridIndex: 1,
                                data: xData,
                                boundaryGap: false,
                                axisLabel: {
                                    // interval: 4
                                },
                                splitLine: {
                                    show: false
                                },
                                axisLine: {
                                    onZero: false
                                }
                            }],
                    yAxis:
                        [
                            {
                                name: '次/分钟',
                                gridIndex: 0,
                                axisLine: { show: true },
                                minInterval: 1,
                                axisLabel: {
                                    formatter: "{value}",
                                },
                                type: "value",
                            },
                            {
                                name: '百分比',
                                gridIndex: 1,
                                axisLine: { show: true },
                                minInterval: 1,
                                axisLabel: {
                                    formatter: "{value}",
                                },
                                type: "value",
                            },],
                    series: [
                        {
                            name: "步频",
                            data: bp,
                            type: "line",
                            // smooth: true,
                            showSymbol: false,
                            color: ["#91cc75"],
                            xAxisIndex: 0,
                            yAxisIndex: 0
                        },
                        {
                            name: "对称性",
                            data: dcx,
                            type: "line",
                            // smooth: true,
                            showSymbol: false,
                            color: ["#91cc75"],
                            xAxisIndex: 1,
                            yAxisIndex: 1
                        },
                    ],
                };

                zhexian2.setOption(option);
            }
        },
        TrainEcharts: function () {
            var res = JSON.parse(this.resultData.JSON);
            console.oldLog(res);
            var leftAccuracyRate = res.leftAccuracyRate_List;
            var rightAccuracyRate = res.rightAccuracyRate_List;
            var xData = [];
            var len = rightAccuracyRate.length;
            if (leftAccuracyRate.length >= rightAccuracyRate.length) len = leftAccuracyRate.length;

            for (let i = 1; i <= len; i++) {
                xData.push(i)
            }
            const zhexian = this.$refs.zheXianResult;
            if (zhexian) {
                var option = {
                    // title: {
                    //     text: '准确率',
                    //     textStyle: {
                    //         fontSize: 16,
                    //         fontWeight: 700,
                    //         color: '#303133',
                    //         // fontStyle:'Alibaba-R'
                    //     }
                    // },
                    tooltip: {
                        trigger: 'axis'
                    },
                    grid: { top: 80 },
                    legend: {},
                    toolbox: {
                        show: true,
                        feature: {
                            dataView: {
                                readOnly: true,
                                title: '数据视图',
                                lang: [
                                    '<div>准确率数据</div>',
                                    '<div>关闭</div>'
                                ],
                                optionToContent: function (opt) {
                                    var axisData = opt.xAxis[0].data;
                                    var series = opt.series;
                                    var table = '<table style="width:100%;text-align:center"><tbody><tr>'
                                        + '<td>' + '步数' + '</td>'
                                        + '<td>' + series[0].name + '(%)' + '</td>'
                                        + '<td>' + series[1].name + '(%)' + '</td>'
                                        + '</tr>';
                                    for (var i = 0, l = axisData.length; i < l; i++) {
                                        var left = series[0].data[i];
                                        var right = series[1].data[i];
                                        if (left == undefined) left = '-';
                                        if (right == undefined) right = '-';
                                        table += '<tr>'
                                            + '<td>' + axisData[i] + '</td>'
                                            + '<td>' + left + '</td>'
                                            + '<td>' + right + '</td>'
                                            + '</tr>';
                                    }
                                    table += '</tbody></table>';
                                    return table;

                                }
                            },
                            magicType: {
                                type: ['line', 'bar'],
                                title: {
                                    line: '切换为折线图',
                                    bar: '切换为柱状图'
                                }
                            },
                        }
                    },
                    xAxis: {
                        name: '步数',
                        nameLocation: 'end',
                        data: xData,
                        boundaryGap: false
                    },
                    yAxis: {
                        name: '准确率',
                        nameLocation: 'end',
                        type: 'value',
                        axisLabel: {
                            formatter: '{value} %'
                        }
                    },
                    dataZoom: { type: "inside" },
                    series: [
                        {
                            name: '左脚准确率',
                            type: 'line',
                            data: leftAccuracyRate,
                        },
                        {
                            name: '右脚准确率',
                            type: 'line',
                            data: rightAccuracyRate,
                            // markPoint: {
                            //     data: [
                            //         { type: 'max', name: 'Max' },
                            //         { type: 'min', name: 'Min' }
                            //     ]
                            // },
                            // markLine: {
                            //     data: [{ type: 'average', name: 'Avg' }]
                            // }
                        }
                    ]
                }
                zhexian.setOption(option);
            }
        }
    },
    mounted: function () {
        this.userinfo = this.$root.currentPatient;

        this.printTime = "打印时间：" + new Date().Format("yyyy-MM-dd hh:mm:ss");
        this.getTrainData();
    },
};
</script>

<style >
.trainreport .printTime {
    padding-bottom: 20px;
}

.trainreport .printTime div {
    font-size: 14px;
    width: 100%;
    text-align: right;
}


.btnPrint_Report {
    position: absolute;
    right: 90px;
    top: 15px;
}

.btnExport_Report {
    position: absolute;
    right: 96px;
    top: 15px;
}


.btnExit_Report {
    position: absolute;
    right: 20px;
    top: 15px;
}

.infos_box {
    width: calc(100% - 80px);
    margin-top: 20px;
}

.eachar-til {
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 0px;
    gap: 20px;
    height: 25px;

    font-family: "Alibaba PuHuiTi";
    font-style: normal;
    font-weight: 500;
    font-size: 18px;
    line-height: 25px;

    color: #666666;
}

.eachar {
    /* 如不够全部显示  在元素后插入分页符 */
    page-break-after: auto;
    /* 避免在 内部插入分页符 */
    page-break-inside: avoid;
}


.trainreport .el-table tr {
    font-weight: normal;
    height: 45px;
    line-height: 45px;
    color: #606266;
    font-size: 14px;
}

.trainreport .el-table th {
    font-weight: 600;
}

.trainreport .el-table .el-table__cell {
    padding: 0;
}

.trainreport .el-table .cell {
    padding: 0px 11px;
}

.trainreport .tabletitle {
    font-weight: 700;
    font-size: 16px;
}

.trainreport .el-descriptions__body .el-descriptions__table .descLabelname_250 {
    width: 250px;
}
</style>
