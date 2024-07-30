//本地数据：为了便于修改名字，不用修改数据库
window.aa_localDB = {
    appName: "",

    /**
     * 评估项目
     */
    data_pcList: [
        {
            id: "scale1001",
            name: "步态评估",
            pcMethod: "请在平整的地面进行测试，受试者双手自然垂直放置，身体站立保持不动，点击开始采集，受试者开始行走。行走长度大于6m，行走结束后，点击结束采集按键。",
            configFile: "scale1001.json",
            img: "./resource/img/Train/评估.png",
            maxBuSu: 6.5,
            assessTime: 60,//需要评估时间
        },
    ],
    /**
     * 训练项目
     */
    data_xlList: [
        {
            id: 100001,
            name: '实景训练',
            maxBuSu: 3,
            maxBuPin: 120,
            maxTime: 30,
            maxDifficulty:1,
            img: "./resource/img/Train/实景训练.png",
            configFile: "Config.json",
            exePath: "Trains\\HYT-100001-实景训练.exe",
            runMode:"Unity"
        },
        {
            id: 200001,
            name: '叠叠高',
            maxBuSu: -1,
            maxBuPin: -1,
            maxTime: -1,
            maxDifficulty:5,
            img: "./resource/img/Train/叠叠高.png",
            configFile: "Config.json",
            exePath: "Trains\\HYT-200001-Jenga.exe",
            runMode:"Unity"
        },
        {
            id: 200002,
            name: '节奏大师',
            maxBuSu: -1,
            maxBuPin: -1,
            maxTime: -1,
            maxDifficulty:5,
            img: "./resource/img/Train/节奏大师.png",
            configFile: "Config.json",
            exePath: "Trains\\HYT-200002-RhythmMaster.exe",
            runMode:"Unity"
        },
    ],





    //通过编号查询训练
    getTrainByid: function (trainid) {
        return this.data_xlList.find(function (item) {
            return item.id == trainid;
        });
    },
    /**
     * 通过编号查询评估信息
     */
    getAssessBySN: function (assessSN, type) {
        if (type == 0) {
            return this.data_pcList.find(function (item) {
                return item.SN == assessSN;
            });
        } else if (type == 1) {
            return this.data_Paradigm.find(function (item) {
                return item.SN == assessSN;
            });
        }
    },
    getAssessByid: function (assessID) {
        return this.data_pcList.find(function (item) {
            return item.id == assessID;
        });
    },
};
