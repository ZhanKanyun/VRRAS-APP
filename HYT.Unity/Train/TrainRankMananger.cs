using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT.TCP
{
    /// <summary>
    /// 训练排行管理模块
    /// 1个游戏有多个排行榜TrainRank，比如不同的关卡或难度
    /// 1个排行榜中有多个人 TrainRankItem ，默认3个
    /// </summary>
    public class TrainRankMananger
    {
        #region ■------------------ 单例

        private TrainRankMananger() { }
        public static readonly TrainRankMananger Instance = new TrainRankMananger();

        #endregion

        /// <summary>
        /// 排行榜列表
        /// </summary>
        public List<TrainRank> Ranks = new List<TrainRank>();

        /// <summary>
        /// 排行榜人数
        /// </summary>
        private int _rankPersonCount = 3;
        /// <summary>
        /// 排名模式 排名模式  1=从大到小  2=从小到大
        /// </summary>
        private int _rankMode { get; set; }= 1;

        /// <summary>
        /// 数据是否初始化
        /// </summary>
        public bool IsInit { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="current">当前排行榜信息</param>
        /// <param rankMode="rankMode">排名模式  1=值从大到小  2=值从小到大</param>
        /// <param name="rankPersonCount">排行榜人数设置</param>
        public void Init(string current, int rankMode = 1, int rankPersonCount = 3)
        {
            if (!IsInit)
            {
                IsInit = true;
                _rankMode = rankMode;
                _rankPersonCount = rankPersonCount;

                try
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        //获取数据
                        Ranks = JsonConvert.DeserializeObject<List<TrainRank>>(current);
                        //值排序
                        foreach (var rank in Ranks)
                        {
                            if (rank.RankMode == 1)
                            {
                                rank.RankItems = rank.RankItems.OrderByDescending(o => o.Value).ToList();
                            }
                            else
                            {
                                rank.RankItems = rank.RankItems.OrderBy(o => o.Value).ToList();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                if (Ranks == null)
                {
                    Ranks = new List<TrainRank>();
                }
            }
        }

        /// <summary>
        /// 检测并更新记录，返回是否打破记录
        /// </summary>
        ///  <param key="key">排行榜关键字，关卡或难度</param>
        /// <param name="name">用户姓名</param>
        /// <param name="value">记录值</param>
        public bool CheckUpdate(int key, string name, float value)
        {
            if (IsInit)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "玩家";
                }
                //获取排行榜信息
                TrainRank rank = null;
                if (Ranks.Count(o => o.Key == key) <= 0)//没有指定Key的排行榜，创建
                {
                    rank = new TrainRank();
                    rank.Key = key;
                    rank.RankMode = _rankMode;
                    rank.RankCount = _rankPersonCount;
                    Ranks.Add(rank);
                }
                else
                {
                    rank = Ranks.Find(o => o.Key == key);
                }

                if (rank.RankItems.Count >= rank.RankCount)//记录已满，检测是否打破记录，相同不算打破记录
                {
                    //取出最后排名记录与传入记录对比
                    var min = rank.RankItems.Min(o => o.Value);
                    var max = rank.RankItems.Max(o => o.Value);
                    if ((rank.RankMode == 1 && value <= min)||(rank.RankMode == 2 && value >= max))//未打破纪录
                    {
                        return false;
                    }
                    else//打破纪录，移除最后排名记录的，加入新纪录
                    {
                        if (rank.RankMode == 1)
                        {
                            rank.RankItems.RemoveAll(o => o.Value == min);
                            rank.RankItems.Add(new TrainRankItem() { PName = name, Value = value, Time = DateTime.Now });
                            rank.RankItems = rank.RankItems.OrderByDescending(o => o.Value).ToList();
                        }
                        else
                        {
                            rank.RankItems.RemoveAll(o => o.Value == max);
                            rank.RankItems.Add(new TrainRankItem() { PName = name, Value = value, Time = DateTime.Now });
                            rank.RankItems = rank.RankItems.OrderBy(o => o.Value).ToList();
                        }
                        return true;
                    }
                }
                else//记录未满，加入然后排序
                {
                    rank.RankItems.Add(new TrainRankItem() { PName = name, Value = value, Time = DateTime.Now });
                    if (rank.RankMode == 1)
                    {
                        rank.RankItems = rank.RankItems.OrderByDescending(o => o.Value).ToList();
                    }
                    else
                    {
                        rank.RankItems = rank.RankItems.OrderBy(o => o.Value).ToList();
                    }
                    return true;
                }
            }
            return false;
        }
    }


    /// <summary>
    /// 排行榜：标志可以选关卡或难度
    /// </summary>
    public class TrainRank
    {
        /// <summary>
        /// 排行榜唯一关键字：关卡或难度
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 排名模式  1=从大到小  2=从小到大
        /// </summary>
        public int RankMode { get; set; } = 1;

        /// <summary>
        /// 排名数量
        /// </summary>
        public int RankCount { get; set; } = 3;

        /// <summary>
        /// 排行信息
        /// </summary>
        public List<TrainRankItem> RankItems = new List<TrainRankItem>();

        public override string ToString()
        {
            if (RankMode == 1)
            {
                return $"Key={Key}   排名模式=值从大到小   排名数量={RankCount}";
            }
            else
            {
                return $"Key={Key}   排名模式=值从小到大   排名数量={RankCount}";
            }
        }
    }

    /// <summary>
    /// 排行榜项
    /// </summary>
    public class TrainRankItem
    {
        /// <summary>
        /// 用户名字，即使用户被删除记录还是这个名字
        /// </summary>
        public string PName { get; set; }

        /// <summary>
        /// 排名顺序 1 2 3
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 排名数据 时间用秒，分数用分
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime Time { get; set; }

        public override string ToString()
        {
             return $"名字={PName}   值={Value}   记录时间={Time.ToString()}";
        }
    }
}
