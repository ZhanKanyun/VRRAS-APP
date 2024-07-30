using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT.TCP
{
    /// <summary>
    /// 训练记录基类
    /// 部分游戏记录如关卡用时由Unity记录
    /// 其余记录如动作坚持时间记录由主程序记录
    /// </summary>
    public class MsgData_RoundDone
    {
        #region ■------------------ 键值对

        /// <summary>
        /// 数据字典：字符串键值对
        /// </summary>
        public List<Result_KV> Dic_KV = new List<Result_KV>();

        #endregion

        #region ■------------------ 图表

        /// <summary>
        /// 图表集合
        /// </summary>
        public List<KEchartBase> ListEchart = new List<KEchartBase>();

        /// <summary>
        /// 获取折线图，没有会新建
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public KEchartLine EchartGetLine(string name,string xAxisName="",string yAxisName="")
        {
            KEchartLine echart = null;
            var list = ListEchart.FindAll(o => o.type == "line");
            if (list.Count(o => o.name == name) > 0)
            {
                echart = list.Find(o => o.name == name) as KEchartLine;
            }
            else
            {
                echart = new KEchartLine() { name = name, type = "line" };
                echart.title.text = name;
                echart.xAxis.name = xAxisName;
                echart.yAxis.name = yAxisName;
                echart.series.Add(new KEchartSeries()
                {
                    type = "line"
                });
                echart.id = ListEchart.Count + 1;
                ListEchart.Add(echart);
            }

            return echart;
        }

        /// <summary>
        /// 获取柱状图，没有会新建
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public KEchartBar EchartGetBar(string name, string xAxisName = "", string yAxisName = "")
        {
            KEchartBar echart = null;
            var list = ListEchart.FindAll(o => o.type == "bar");
            if (list.Count(o => o.name == name) > 0)
            {
                echart = list.Find(o => o.name == name) as KEchartBar;
            }
            else
            {
                echart = new KEchartBar() { name = name, type = "bar" };
                echart.title.text = name;
                echart.xAxis.name = xAxisName;
                echart.yAxis.name = yAxisName;
                echart.series.Add(new KEchartSeries()
                {
                    type = "bar"
                });
                echart.id = ListEchart.Count + 1;
                ListEchart.Add(echart);
            }

            return echart;
        }

        public void EchartClear()
        {
            ListEchart.Clear();
        }

        #endregion

        #region ■------------------ 其它特有

        /// <summary>
        /// 音乐ID
        /// </summary>
        public string MusicID { get; set; }

        /// <summary>
        /// 音乐名称
        /// </summary>
        public string MusicName { get; set; }

        /// <summary>
        /// 选择难度
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        /// 得分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 准确率
        /// </summary>
        public int AccuracyRate { get; set; }

        /// <summary>
        /// 获得金币数
        /// </summary>
        public int GetGold { get; set; }

        /// <summary>
        /// JSON数据
        /// </summary>
        public string JSON { get; set; }
    }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class Result_KV
    {
        public string N { get; set; }
        public string V { get; set; }
    }

    /// <summary>
    /// 图表标题
    /// </summary>
    public class KEchartTitle
    {
        public string text { get; set; }
    }


    /// <summary>
    /// 图表X轴
    /// </summary>
    public class KEchartXAxis
    {
        public string type { get; set; } = "category";
        public List<string> data { get; set; } = new List<string>();


        /// <summary>
        /// 坐标轴名称
        /// </summary>
        public string name { get; set; } = "";
    }

    /// <summary>
    /// 图表Y轴
    /// </summary>
    public class KEchartYAxis
    {
        public string type { get; set; } = "value";

        /// <summary>
        /// 坐标轴名称
        /// </summary>
        public string name { get; set; } = "";
    }

    /// <summary>
    /// 图表图例
    /// </summary>
    public class KEchartLegend
    {
        public List<string> data { get; set; } =new List<string>();
    }

    /// <summary>
    /// 图表Y轴
    /// </summary>
    public class KEchartSeries
    {
        public string type { get; set; } = "line";
        public List<float> data { get; set; } = new List<float>();
    }

    /// <summary>
    /// 图表基类
    /// </summary>
    public class KEchartBase
    {
        public KEchartTitle title { get; set; }=new KEchartTitle();

        public string name { get; set; }

        public string type { get; set; }

        public int id { get; set; }

        /// <summary>
        /// 图例
        /// </summary>
        public KEchartLegend legend { get; set; } = new KEchartLegend();
    }

    /// <summary>
    /// 折线图
    /// </summary>
    public class KEchartLine: KEchartBase
    {
        public KEchartXAxis xAxis { get; set; } = new KEchartXAxis();
        public KEchartYAxis yAxis { get; set; } = new KEchartYAxis();
        public List<KEchartSeries> series { get; set; } = new List<KEchartSeries>();
    }

    /// <summary>
    /// 柱状图
    /// </summary>
    public class KEchartBar: KEchartBase
    {
        public KEchartXAxis xAxis { get; set; } = new KEchartXAxis();
        public KEchartYAxis yAxis { get; set; } = new KEchartYAxis();
        public List<KEchartSeries> series { get; set; } = new List<KEchartSeries>();
    }


