using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("tb_LogOperate")]
    public partial class tb_LogOperate
    {
        public tb_LogOperate()
        {


        }
        /// <summary>
        /// Desc:主键
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }

        /// <summary>
        /// Desc:开始时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime Time { get; set; }


        /// <summary>
        /// Desc:开始时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Type { get; set; }

        /// <summary>
        /// Desc:开始时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string DataID { get; set; }

        /// <summary>
        /// Desc:开始时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Old { get; set; }
    }
}
