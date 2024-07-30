using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("tb_LogTrain")]
    public partial class tb_LogTrain
    {
           public tb_LogTrain(){


           }
           /// <summary>
           /// Desc:主键
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string ID {get;set;}

           /// <summary>
           /// Desc:患者ID
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public string PatientID {get;set;}

        [SugarColumn(IsIgnore =true)]           
        public string PatientName { get; set; }

        /// <summary>
        /// Desc:训练ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string TrainID {get;set;}

        /// <summary>
        /// 训练类型  1-训练 2-游戏
        /// </summary>
        public int TrainType { get; set; }
        /// <summary>
        /// Desc:训练名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TrainName {get;set;}

           /// <summary>
           /// Desc:开始时间
           /// Default:
           /// Nullable:False
           /// </summary>           
           public DateTime StartTime {get;set;}

           /// <summary>
           /// Desc:结束时间
           /// Default:
           /// Nullable:False
           /// </summary>           
           public DateTime EndTime {get;set;}

           /// <summary>
           /// Desc:设置信息
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SetInfo {get;set;}

           /// <summary>
           /// Desc:设置时长
           /// Default:
           /// Nullable:False
           /// </summary>           
           public int SetTime {get;set;}

           /// <summary>
           /// Desc:设置的难度
           /// Default:
           /// Nullable:False
           /// </summary>           
           public int SetDifficulty {get;set;}

           /// <summary>
           /// Desc:JSON结果
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Result {get;set;}

           /// <summary>
           /// Desc:JSON其它数据
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Data {get;set;}

        /// <summary>
        /// Desc:JSON其它数据
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Record { get; set; }

        /// <summary>
        /// Desc:得分
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public int? Score {get;set;}

           /// <summary>
           /// Desc:音乐ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MusicID {get;set;}

           /// <summary>
           /// Desc:准确率
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int AccuracyRate {get;set;}

    }

    public class tb_LogTrainSimple
    {
        public string PatientID { get; set; }
        public int? Score { get; set; }

    }
}
