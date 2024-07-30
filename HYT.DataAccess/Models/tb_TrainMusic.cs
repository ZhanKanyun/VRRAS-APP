using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("tb_TrainMusic")]
    public partial class tb_TrainMusic
    {
           public tb_TrainMusic(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string ID {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string TrainID {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string MusicID {get;set;}

        [SugarColumn(IsIgnore =true)]
        public string MusicName { get;set;}

           /// <summary>
           /// Desc:【20230407弃用】音乐直接绑定到训练，不再是训练难度
           /// Default:
           /// Nullable:False
           /// </summary>           
           public int Difficulty {get;set;}


    }
}
