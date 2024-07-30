using System;
using System.Collections.Generic;
using System.Text;

namespace KT.TCP.Train
{
    public class tb_LogTrainUnity
    {

        public tb_LogTrainUnity()
        {

        }

        public string ID { get; set; }

        /// <summary>
        /// Desc:患者ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string PatientID { get; set; }

        public string PatientName { get; set; }

        /// <summary>
        /// Desc:训练ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string TrainID { get; set; }

        /// <summary>
        /// Desc:开始时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Desc:设置的难度
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int SetDifficulty { get; set; }


        /// <summary>
        /// Desc:得分
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public int? Score { get; set; }

        /// <summary>
        /// Desc:音乐ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MusicID { get; set; }

        /// <summary>
        /// Desc:准确率
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int AccuracyRate { get; set; }

    }
    
}
