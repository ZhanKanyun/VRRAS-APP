using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("tb_Setting")]
    public partial class tb_Setting
    {
           public tb_Setting(){


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
           /// Nullable:True
           /// </summary>           
           public string OrganizationName {get;set;}

           /// <summary>
           /// Desc:
           /// Default:3
           /// Nullable:False
           /// </summary>           
           public int ProductType {get;set;}

    }
}
