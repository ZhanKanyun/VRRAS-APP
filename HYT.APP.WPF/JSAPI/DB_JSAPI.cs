using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CL.Common;
using Models;
//using System.Windows.Forms;
using KT.TCP;
using Microsoft.Win32;
using HYT.MidiManager;

namespace KCL
{
    public class DB_JSAPI
    {
        private DB_JSAPI() { }
        public static readonly DB_JSAPI Instance = new DB_JSAPI();

        #region ■------------------ 系统设置

        public string Setting_Get()
        {
            try
            {
                tb_Setting result = null;
                var list = DBHelper.Instance.GetAll<tb_Setting>();
                if (list.Count <= 0)
                {
                    tb_Setting temp = new tb_Setting();
                    temp.OrganizationName = "";
                    if (!DBHelper.Instance.Insert<tb_Setting>(temp))
                    {
                        return JSAPIResponse.Error("无数据").ToJson();
                    }
                    result = temp;
                }
                else
                {
                    result = list[0];
                }
                return JSAPIResponse.Success(new { setting = result }).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Setting_Set(string data)
        {
            try
            {
                tb_Setting temp = JsonConvert.DeserializeObject<tb_Setting>(data);
                if (DBHelper.Instance.Update<tb_Setting>(temp))
                {
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Setting_SetOrganizationName(string name)
        {
            try
            {
                if (name == null)
                {
                    return JSAPIResponse.Error("参数错误").ToJson();
                }
                if (name.Length > 20)
                {
                    return JSAPIResponse.Error("长度不能超过20字符").ToJson();
                }
                tb_Setting db = DBHelper.Instance.GetAll<tb_Setting>()[0];
                var oldName = db.OrganizationName;
                db.OrganizationName = name;
                if (DBHelper.Instance.Update<tb_Setting>(db))
                {
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    db.OrganizationName = oldName;
                    return JSAPIResponse.Error(data: db).ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Setting_GetOrganizationName()
        {
            try
            {
                tb_Setting result = null;
                var list = DBHelper.Instance.GetAll<tb_Setting>();
                if (list.Count <= 0)
                {
                    tb_Setting temp = new tb_Setting();
                    temp.OrganizationName = "";
                    if (!DBHelper.Instance.Insert<tb_Setting>(temp))
                    {
                        return JSAPIResponse.Error("无数据").ToJson();
                    }
                    result = temp;
                }
                else
                {
                    result = list[0];
                }
                return JSAPIResponse.Success(null, dataStr1: result.OrganizationName).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

        #region ■------------------ 曲库管理

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Music_GetWherePage(int currentPage, int pageSize, string key)
        {
            try
            {
                var count = 0;

                //string sql = $"select  * from tb_Patient where IsValid=1 and  (Name like '%{key}%' or SN like '%{key}%') order by LastLoginTime desc,CreateTime desc";
                //var list = DBHelper.Instance.GetPageWhere<tb_Patient>(ref currentPage, pageSize, sql, ref count);
                var list = DBHelper.Instance.GetPageWhere<tb_Music>(ref currentPage, pageSize, o => o.IsValid && o.Name.Contains(key), ref count, "CreateTime desc");
                return JSAPIResponse.Success(new JSAPIPage<tb_Music>(count, list, currentPage)).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Music_GetAll()
        {
            try
            {
                var musics = DBHelper.Instance.GetWhere<tb_Music>(o=>o.IsValid);
                if (musics == null)
                {
                    return JSAPIResponse.Error().ToJson();
                }
                else
                {
                    return JSAPIResponse.Success(musics).ToJson();
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Music_GetByID(string id)
        {
            try
            {
                var music = DBHelper.Instance.GetOneByID<tb_Music>(id);
                if (music == null)
                {
                    return JSAPIResponse.Error().ToJson();
                }
                else
                {
                    return JSAPIResponse.Success(music).ToJson();
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 【废弃】添加音乐，音乐由WPF读取文件流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Music_Add(string data)
        {
            try
            {
                tb_Music temp = JsonConvert.DeserializeObject<tb_Music>(data);

                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }

                if (temp.Name.Length < 1 || temp.Name.Length > 20)
                {
                    return JSAPIResponse.Error("名称只能1到20个字符").ToJson();
                }

                #endregion

                temp.IsValid = true;
                var d_now = DateTime.Now;
                temp.CreateTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);
                temp.UpdateTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);

                if (string.IsNullOrEmpty(temp.FilePath))
                {
                    return JSAPIResponse.Error("请上传音乐文件").ToJson();
                }
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music", temp.FilePath);
                if (!File.Exists(filePath))
                {
                    return JSAPIResponse.Error("音乐文件不存在，请重新上传").ToJson();
                }

                temp.ID = Guid.NewGuid().ToString();
                while (DBHelper.Instance.GetCountWhere<tb_Music>("ID=@id", new { id = temp.ID }) > 0)
                {
                    temp.ID = Guid.NewGuid().ToString();
                }
                MemoryStream memory = new MemoryStream();
                using (FileStream fs=File.OpenRead(filePath))
                {
                    fs.CopyTo(memory);
                }
                if (memory.Length<=0)
                {
                    return JSAPIResponse.Error("音乐文件大小异常，请重新上传").ToJson();
                }
                temp.FileData= memory.ToArray();
                temp.Size = (int)(memory.Length / 1024L);

                if (DBHelper.Instance.Insert<tb_Music>(temp))
                {
                    AppManager.Instance.AddLogOperate("添加音乐", temp.ID, JsonConvert.SerializeObject(temp));
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 添加音乐，音乐由网页上传文件并传字节流过来
        /// </summary>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public string Music_AddStream(string data,string stream)
        {
            try
            {
                if (string.IsNullOrEmpty(stream))
                {
                    return JSAPIResponse.Error("请上传音乐文件").ToJson();
                }
                string[] strArray = stream.Split(",");
                byte[] byteArray=new byte[strArray.Length];
                for (int i = 0; i < strArray.Length; i++)
                {
                    byteArray[i] = Convert.ToByte(strArray[i]);
                }
                tb_Music temp = JsonConvert.DeserializeObject<tb_Music>(data);

                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }
                if (temp.Name.Length < 1 || temp.Name.Length > 30)
                {
                    return JSAPIResponse.Error("名称只能1到30个字符").ToJson();
                }
                if (DBHelper.Instance.GetCount<tb_Music>() > 100)
                {
                    return JSAPIResponse.Error($"最多允许添加100首音乐").ToJson();
                }
                if (DBHelper.Instance.GetCountWhere<tb_Music>(o=>o.Name==temp.Name) > 0)
                {
                    return JSAPIResponse.Error($"{temp.Name} 已存在，名称不允许重复").ToJson();
                }
                if (temp.Size <= 0)
                {
                    return JSAPIResponse.Error($"MIDI文件过小").ToJson();
                }
                if (temp.Size >= 500)
                {
                    return JSAPIResponse.Error($"MIDI文件过大，不允许超过500KB").ToJson();
                }

                #endregion

                temp.IsValid = true;
                var d_now = DateTime.Now;
                temp.CreateTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);
                temp.UpdateTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);


                temp.ID = Guid.NewGuid().ToString();
                while (DBHelper.Instance.GetCountWhere<tb_Music>("ID=@id", new { id = temp.ID }) > 0)
                {
                    temp.ID = Guid.NewGuid().ToString();
                }
              
                temp.FileData = byteArray;
                //temp.Size = byteArray.Length/1000;

                if (DBHelper.Instance.Insert<tb_Music>(temp))
                {
                    AppManager.Instance.AddLogOperate("添加音乐", temp.ID, JsonConvert.SerializeObject(temp));
                    MidiControl.Instance.AddMusic(new MidiMusic()
                    {
                        ID = temp.ID,
                        Name = temp.Name,
                        MusicBeat = temp.BPM,
                        FileData = byteArray,
                    });
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 【废弃】
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Music_Edit(string data)
        {
            try
            {
                tb_Music temp = JsonConvert.DeserializeObject<tb_Music>(data);

                tb_Music old = DBHelper.Instance.GetOneByID<tb_Music>(temp.ID);
                if (old == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }
                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }

                if (temp.Name.Length < 1 || temp.Name.Length > 20)
                {
                    return JSAPIResponse.Error("名称只能1到20个字符").ToJson();
                }

                #endregion

                //重新上传文件
                if (temp.FilePath!=old.FilePath)
                {
                    if (string.IsNullOrEmpty(temp.FilePath))
                    {
                        return JSAPIResponse.Error("请上传音乐文件").ToJson();
                    }
                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music", temp.FilePath);
                    if (!File.Exists(filePath))
                    {
                        return JSAPIResponse.Error("音乐文件不存在，请重新上传").ToJson();
                    }
                    MemoryStream memory = new MemoryStream();
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        fs.CopyTo(memory);
                    }
                    if (memory.Length <= 0)
                    {
                        return JSAPIResponse.Error("音乐文件大小异常，请重新上传").ToJson();
                    }
                    temp.FileData = memory.ToArray();
                    temp.Size = (int)(memory.Length / 1024L);
                }
                

                temp.UpdateTime = DateTime.Now;
                if (DBHelper.Instance.Update<tb_Music>(temp))
                {
                    AppManager.Instance.AddLogOperate("修改音乐", temp.ID, JsonConvert.SerializeObject(temp));
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 修改文件，音乐由网页上传文件并传字节流过来
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Music_EditStream(string data, string stream)
        {
            try
            {
                tb_Music temp = JsonConvert.DeserializeObject<tb_Music>(data);

                if (AppManager.Instance.CurrentWebTestMusic != null && AppManager.Instance.CurrentWebTestMusic.ID == temp.ID)
                {
                    return JSAPIResponse.Error("正在播放的音乐无法编辑").ToJson();
                }

                tb_Music old = DBHelper.Instance.GetOneByID<tb_Music>(temp.ID);
                if (old == null)
                {
                    return JSAPIResponse.Error("歌曲不存在").ToJson();
                }
                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }

                if (temp.Name.Length < 1 || temp.Name.Length > 50)
                {
                    return JSAPIResponse.Error("名称只能1到50个字符").ToJson();
                }
                if (DBHelper.Instance.GetCountWhere<tb_Music>(o => o.ID!=temp.ID&&  o.Name == temp.Name) > 0)
                {
                    return JSAPIResponse.Error($"{temp.Name} 已存在，名称不允许重复").ToJson();
                }

                #endregion

                //重新上传文件
                if (!string.IsNullOrEmpty(stream))
                {
                    string[] strArray = stream.Split(",");
                    byte[] byteArray = new byte[strArray.Length];
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        byteArray[i] = Convert.ToByte(strArray[i]);
                    }

                    temp.FileData = byteArray;
                    temp.Size = byteArray.Length;
                }
                DateTime d_now  = DateTime.Now;
                temp.UpdateTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);
                //temp.UpdateTime = DateTime.Now;
                if (DBHelper.Instance.Update<tb_Music>(temp))
                {
                    AppManager.Instance.AddLogOperate("修改音乐", temp.ID, JsonConvert.SerializeObject(temp));

                    //更改音乐之后，更改已经加载的曲库
                    var libMusic =MidiControl.Instance.GetMidiMusicByID(temp.ID);
                    if (libMusic!=null)
                    {
                        libMusic.Name = temp.Name;
                        libMusic.MusicBeat = temp.BPM;
                        if (libMusic.FileData.Length!=temp.FileData.Length)
                        {
                            libMusic.FileData = temp.FileData;
                            libMusic.LoadState = MidiMusicLoadStates.None;
                        }
                    }
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 硬删除
        /// 已关联的训练：取消关联
        /// 已训练的训练记录：不处理  只是之后查不到音乐名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Music_Delete(string id)
        {
            try
            {
                if (AppManager.Instance.CurrentWebTestMusic != null && AppManager.Instance.CurrentWebTestMusic.ID == id)
                {
                    return JSAPIResponse.Error("正在播放的音乐无法删除").ToJson();
                }
                tb_Music temp = DBHelper.Instance.GetOneByID<tb_Music>(id);

              

                if (DBHelper.Instance.Delete<tb_Music>(temp))
                {
                    DBHelper.Instance.DeleteWhere<tb_TrainMusic>(o=>o.MusicID==temp.ID);//取消训练关联
                    AppManager.Instance.AddLogOperate("删除音乐", temp.ID, JsonConvert.SerializeObject(temp));
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 【会闪退 废弃】选择音乐文件
        /// </summary>
        /// <returns></returns>
        public string JS_MusicSelectFile()
        {
            try
            {
                //if (AppManager.Instance.Window != null)
                //{
                    //AppManager.Instance.Window.Dispatcher.Invoke(() => {
                        var cachePath = AppDomain.CurrentDomain.BaseDirectory + "Music";
                        if (!Directory.Exists(cachePath))
                        {
                            Directory.CreateDirectory(cachePath);
                        }
                        OpenFileDialog openFileDialog1 = new OpenFileDialog();
                        openFileDialog1.Multiselect = false;
                        openFileDialog1.Title = "请选择MIDI文件";
                        openFileDialog1.Filter = "MIDI文件(*.mid;)|*.mid;";

                        if (openFileDialog1.ShowDialog()==true)
                        {
                            var newFileName = $"{cachePath}\\{Guid.NewGuid()}.mid";
                            File.Copy(openFileDialog1.FileName, newFileName);

                            return JSAPIResponse.Success(Path.GetFileName(newFileName),"上传成功").ToJson();
                        }
                        else
                        {
                            return JSAPIResponse.Error("未选择文件").ToJson();
                        }
                    //});
                //}

                return JSAPIResponse.Error("上传失败").ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }


        /// <summary>
        /// 训练关联音乐
        /// </summary>
        /// <returns></returns>
        public string TrainMusic_GetTrainMusic(string trains)
        {
            try
            {
                MusicTreeNode root = new MusicTreeNode("训练配置", "训练配置", "root", null);

                List<tb_Train> listTrain = JsonConvert.DeserializeObject<List<tb_Train>>(trains);

                List<tb_Music> db_musics = DBHelper.Instance.GetWhere<tb_Music>(o => o.IsValid);

                foreach (var trainConfig in listTrain)
                {
                    MusicTreeNode train = new MusicTreeNode(trainConfig.id, trainConfig.name, "train", root.id);

                    List<tb_TrainMusic> db_trainMusics = DBHelper.Instance.GetWhere<tb_TrainMusic>(o => o.TrainID == trainConfig.id);
                    foreach (var trainMusic in db_trainMusics)
                    {
                        tb_Music db_music = null;
                        if (db_musics.Count(o => o.ID == trainMusic.MusicID&&o.IsValid) > 0)
                        {
                            db_music = db_musics.First(o => o.ID == trainMusic.MusicID && o.IsValid);
                            MusicTreeNode music = new MusicTreeNode(db_music.ID, db_music.Name, "music", trainMusic.ID);
                            train.children.Add(music);
                        }
                        else
                        {
                            LogHelper.Info($"【曲库】GetTrainMusic 音乐ID（{trainMusic.MusicID}）已移除");
                        }
                    }
                    root.children.Add(train);
                }

                return JSAPIResponse.Success(root.children).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 应用
        /// </summary>
        /// <param name="trainid"></param>
        /// <param name="musicid"></param>
        /// <returns></returns>
        public string TrainMusic_Add(string trainid,string musicid)
        {
            try
            {
                tb_Music db_music = DBHelper.Instance.GetOneByID<tb_Music>(musicid);
                if (db_music==null|| db_music.IsValid==false)
                {
                        return JSAPIResponse.Error("音乐不存在").ToJson();
                }


                tb_TrainMusic temp = new tb_TrainMusic();
                temp.Difficulty = 88;
                temp.TrainID = trainid;
                temp.MusicID = musicid;
                temp.MusicName = db_music.Name;
                temp.ID = Guid.NewGuid().ToString();
                while (DBHelper.Instance.GetCountWhere<tb_TrainMusic>("ID=@id", new { id = temp.ID }) > 0)
                {
                    temp.ID = Guid.NewGuid().ToString();
                }
                if (DBHelper.Instance.GetCountWhere<tb_TrainMusic>(o => o.TrainID == temp.TrainID) >= 30)
                {
                    return JSAPIResponse.Error("每个训练最多包含30首音乐").ToJson();
                }

                if (DBHelper.Instance.GetCountWhere<tb_TrainMusic>(o=>o.MusicID== temp.MusicID&& o.TrainID==temp.TrainID)>0)
                {
                    return JSAPIResponse.Error("该训练已存在此音乐").ToJson();
                }
                if (!DBHelper.Instance.Insert<tb_TrainMusic>(temp))
                {
                    return JSAPIResponse.Error("应用失败").ToJson();
                }
                AppManager.Instance.AddLogOperate("应用音乐", temp.ID, JsonConvert.SerializeObject(temp));
                return JSAPIResponse.Success(temp,"应用成功").ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }


        /// <summary>
        /// 取消应用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string TrainMusic_Delete(string id)
        {
            try
            {
                var temp = DBHelper.Instance.GetOneByID<tb_TrainMusic>(id);
                if (temp!=null)
                {
                    DBHelper.Instance.Delete<tb_TrainMusic>(temp);
                }
                AppManager.Instance.AddLogOperate("取消应用音乐", temp.ID, JsonConvert.SerializeObject(temp));
                return JSAPIResponse.Success(temp,"删除成功").ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }


        /// <summary>
        /// 开始试听音乐
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Music_StartTest(string id)
        {
            try
            {
                tb_Music music = DBHelper.Instance.GetOneByID<tb_Music>(id);
                if (music == null)
                {
                    return JSAPIResponse.Error("音乐不存在").ToJson();
                }

                MidiControl.Instance.PlayMusic(music.ID, music.BPM, true);
                //MidiControl.Instance.PlayMusicbeat(2f, 1f);

                AppManager.Instance.CurrentWebTestMusic = music;
                return JSAPIResponse.Success("播放成功").ToJson();

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 停止试听音乐
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Music_StopTest()
        {
            try
            {
                if (AppManager.Instance.CurrentWebTestMusic != null)
                {
                    MidiControl.Instance.StopMusic();
                    AppManager.Instance.CurrentWebTestMusic = null;
                }

                return JSAPIResponse.Success("停止成功").ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 获取当前试听音乐
        /// </summary>
        /// <returns></returns>
        public string Music_GetTest()
        {
            try
            {
                if (AppManager.Instance.CurrentWebTestMusic == null)
                {
                    return JSAPIResponse.Error().ToJson();
                }

                return JSAPIResponse.Success(AppManager.Instance.CurrentWebTestMusic).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }


        #endregion

        #region ■------------------ 用户管理

        /// <summary>
        /// 分页获取用户数据
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Patient_GetWherePage(int currentPage, int pageSize, string key)
        {
            try
            {
                var count = 0;

                //string sql = $"select  * from tb_Patient where IsValid=1 and  (Name like '%{key}%' or SN like '%{key}%') order by LastLoginTime desc,CreateTime desc";
                //var list = DBHelper.Instance.GetPageWhere<tb_Patient>(ref currentPage, pageSize, sql, ref count);
                var list = DBHelper.Instance.GetPageWhere<tb_Patient>(ref currentPage, pageSize, o => o.IsValid && (o.Name.Contains(key) || o.SN.Contains(key)), ref count, "LastLoginTime desc , CreateTime desc");
                return JSAPIResponse.Success(new JSAPIPage<tb_Patient>(count, list, currentPage)).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Patient_GetByID(string id)
        {
            try
            {
                var patient = DBHelper.Instance.GetOneByID<tb_Patient>(id);
                if (patient == null)
                {
                    return JSAPIResponse.Error().ToJson();
                }
                else
                {
                    return JSAPIResponse.Success(patient).ToJson();
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Patient_Add(string data)
        {
            try
            {
                tb_Patient temp = JsonConvert.DeserializeObject<tb_Patient>(data);

                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }

                if (temp.Name.Length < 2 || temp.Name.Length > 8)
                {
                    return JSAPIResponse.Error("姓名只能2到8个字符").ToJson();
                }
                if (string.IsNullOrEmpty(temp.DiseaseType))
                {
                    return JSAPIResponse.Error("请选择病症类型").ToJson();
                }

                temp.ID = Guid.NewGuid().ToString();
                while (DBHelper.Instance.GetCountWhere<tb_Patient>("ID=@id", new { id = temp.ID }) > 0)
                {
                    temp.ID = Guid.NewGuid().ToString();
                    //return JSAPIResponse.Error("用户ID重复").ToJson();
                }

                temp.SN = DateTime.Now.ToString("yyyMMddHHmmssFFF") + new Random().Next(10);

                while (DBHelper.Instance.GetCountWhere<tb_Patient>("SN=@sn", new { sn = temp.SN }) > 0)
                {
                    temp.SN = DateTime.Now.ToString("yyyMMddHHmmssFFF") + new Random().Next(10);
                }

                #endregion

                temp.IsValid = true;
                var d_now = DateTime.Now;
                temp.CreateTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);
                temp.LastLoginTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);

                if (DBHelper.Instance.Insert<tb_Patient>(temp))
                {
                    AppManager.Instance.AddLogOperate("添加用户", temp.ID, JsonConvert.SerializeObject(temp));

                    //自动登录
                    if (ConfigManager.Instance.SSetting.AddUserIsAutoLogin)
                    {
                        App_JSAPI.Instance.JS_ChangeCurrentPatient(temp.ID);
                        BrowserManager.Instance.ExecuteJSAsync("API_CSharp.newUserAutoLoginAssess()");
                        //return JSAPIResponse.Success(new { currentPatient= temp }).ToJson();
                    }
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string Patient_Edit(string data)
        {
            try
            {
                tb_Patient temp = JsonConvert.DeserializeObject<tb_Patient>(data);

                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }

                if (temp.Name.Length < 2 || temp.Name.Length > 8)
                {
                    return JSAPIResponse.Error("姓名只能2到8个字符").ToJson();
                }
                if (string.IsNullOrEmpty(temp.DiseaseType))
                {
                    return JSAPIResponse.Error("请选择病症类型").ToJson();
                }

                #endregion

                if (DBHelper.Instance.Update<tb_Patient>(temp))
                {
                    if (AppManager.Instance.CurrentPatient != null && AppManager.Instance.CurrentPatient.ID == temp.ID)
                    {
                        AppManager.Instance.CurrentPatient = temp;
                    }
                    AppManager.Instance.AddLogOperate("修改用户", temp.ID, JsonConvert.SerializeObject(temp));
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 硬删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Patient_Delete(string id)
        {
            try
            {
                tb_Patient temp = DBHelper.Instance.GetOneByID<tb_Patient>(id);
                if (DBHelper.Instance.Delete<tb_Patient>(temp))
                {
                    AppManager.Instance.AddLogOperate("删除用户", temp.ID, JsonConvert.SerializeObject(temp));
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

        #region ■------------------ 评测记录

        public string AssessRecord_Add(string data)
        {
            try
            {
                tb_LogAssess temp = JsonConvert.DeserializeObject<tb_LogAssess>(data);

                temp.ID = Guid.NewGuid().ToString();
                while (DBHelper.Instance.GetCountWhere<tb_LogAssess>("ID=@id", new { id = temp.ID }) > 0)
                {
                    temp.ID = Guid.NewGuid().ToString();                  
                }

                if (DBHelper.Instance.Insert<tb_LogAssess>(temp))
                {
                    return JSAPIResponse.Success(temp).ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 分页获取评测记录
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="pateintID"></param>
        /// <returns></returns>
        public string AssessRecord_GetPage(string pateintID, int currentPage, int pageSize, string OrderBy, string startTime, string endTime)
        {
            try
            {
                int count = 0;
                List<tb_LogAssess> list = null;
                if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {
                    list = DBHelper.Instance.GetPageWhere<tb_LogAssess>(ref currentPage, pageSize, o => (o.PatientID == pateintID), ref count, OrderBy);
                }
                else
                {
                    DateTime start = DateTime.Parse(startTime);
                    DateTime end = DateTime.Parse(endTime);
                    end = end.AddDays(1).AddSeconds(-1);

                    list = DBHelper.Instance.GetPageWhere<tb_LogAssess>(ref currentPage, pageSize, o => (o.PatientID == pateintID && o.StartTime >= start && o.EndTime <= end), ref count, OrderBy);
                }

                //List<tb_LogAssess> list = DBHelper.Instance.GetPageWhereIgnore<tb_LogAssess>(ref currentPage, pageSize, o => (o.PatientID == pateintID), o => o.Result, ref count, OrderBy);

                return JSAPIResponse.Success(new JSAPIPage<tb_LogAssess>(count, list, currentPage)).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string AssessRecord_GetByID(string reportID)
        {
            try
            {
                tb_LogAssess temp = DBHelper.Instance.GetOneByID<tb_LogAssess>(reportID);

                if (temp != null)
                {
                    return JSAPIResponse.Success(temp).ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string AssessRecord_GetNew(string pateintID, int size)
        {
            try
            {
         
                var list = DBHelper.Instance.GetWhereOrderIgnoreNew<tb_LogAssess>(size, o => (o.PatientID == pateintID), o => o.Result, "StartTime DESC");
                return JSAPIResponse.Success(list).ToJson();
                //}
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string AssessRecord_GetByIDS(string ids)
        {
            try
            {
                //var a = AssessRecord_GetByIDS("1|5|8|9|10|50");
                List<string> list_str = new List<string>();
                if (ids.Contains("|"))
                {
                    list_str = ids.Split('|').ToList();
                }
                else
                {
                    list_str.Add(ids);
                }
        
                var list = DBHelper.Instance.GetWhere<tb_LogAssess>(o => list_str.Contains(o.ID));
                return JSAPIResponse.Success(list).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string AssessRecord_Edit(string data)
        {
            try
            {
                tb_LogAssess temp = JsonConvert.DeserializeObject<tb_LogAssess>(data);

                #region 〓〓〓〓〓〓〓 有效性验证

                if (temp == null)
                {
                    return JSAPIResponse.Error("数据解析异常").ToJson();
                }

                #endregion

                if (DBHelper.Instance.Update<tb_LogAssess>(temp))
                {
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string AssessRecord_DeleteByID(string id)
        {
            try
            {
                if (DBHelper.Instance.DeleteByID<tb_LogAssess>(id))
                {
                    AppManager.Instance.AddLogOperate("删除评估记录", id,"");
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error("删除失败").ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

        #region ■------------------ 训练记录

        /// <summary>
        /// 分页获取评测记录
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="pateintID"></param>
        /// <returns></returns>
        public string TrainRecord_GetPage(string pateintID, int currentPage, int pageSize, string OrderBy, string startTime, string endTime,int type)
        {
            try
            {
                int count = 0;
                List<tb_LogTrain> list = null;
                if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {

                    list = DBHelper.Instance.GetPageWhere<tb_LogTrain>(ref currentPage, pageSize, o => (o.PatientID == pateintID && o.TrainType == type), ref count, OrderBy);
                }
                else
                {
                    DateTime start = DateTime.Parse(startTime);
                    DateTime end = DateTime.Parse(endTime);
                    end = end.AddDays(1).AddSeconds(-1);

                    list = DBHelper.Instance.GetPageWhere<tb_LogTrain>(ref currentPage, pageSize, o => (o.PatientID == pateintID && o.TrainType == type && o.StartTime >= start&&o.EndTime<=end), ref count, OrderBy);
                }


                return JSAPIResponse.Success(new JSAPIPage<tb_LogTrain>(count, list, currentPage)).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string TrainRecord_GetByID(string reportID)
        {
            try
            {
                tb_LogTrain temp = DBHelper.Instance.GetOneByID<tb_LogTrain>(reportID);

                if (temp != null)
                {
                    //var newData = new Unity.TrainDataBase();

                    //var obj1 = JsonConvert.DeserializeObject<Unity.TrainDataBase>(temp.ResultMain);
                    //var obj2 = JsonConvert.DeserializeObject<Unity.TrainDataBase>(temp.Result);

                    //newData.Dic_KV = obj1.Dic_KV.Concat(obj2.Dic_KV).ToList();
                    //newData.ListEchart = obj1.ListEchart.Concat(obj2.ListEchart).ToList();

                    //temp.ResultMain = JsonConvert.SerializeObject(newData);

                    return JSAPIResponse.Success(temp).ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string TrainRecord_DeleteByID(string id)
        {
            try
            {
                if (DBHelper.Instance.DeleteByID<tb_LogTrain>(id))
                {
                    AppManager.Instance.AddLogOperate("删除训练记录", id, "");
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error("删除失败").ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string TrainRecord_GetNew(string pateintID, int size)
        {
            try
            {
                int count = 0;
                var list = DBHelper.Instance.GetWhere<tb_LogTrain>(1, size, "StartTime Desc", o => (o.PatientID == pateintID), ref count);
                return JSAPIResponse.Success(list).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 获取用户当前治疗模式的最新n条数据
        /// </summary>
        /// <param name="size">获取大小</param>
        /// <param name="pateintID">用户ID</param>
        /// <param name="PingCeID">治疗模式ID</param>
        /// <returns></returns>
        public string TrainRecord_GetNewPage(int size, string pateintID, string trainID)
        {
            try
            {
                int count = 0;
                var list = DBHelper.Instance.GetWhere<tb_LogTrain>(1, size, "StartTime Desc", o => (o.PatientID == pateintID && o.TrainID == trainID), ref count);
                return JSAPIResponse.Success(new JSAPIPage<tb_LogTrain>(count, list, 0)).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

    }

    /// <summary>
    /// 训练音乐配置
    /// </summary>
    public class MusicTreeNode
    {
        public MusicTreeNode(string i, string l, string t, string pid)
        {
            parentid = pid;
            id = i;
            label = l;
            typee = t;
        }
        public string id { get; set; }

        public string parentid { get; set; }

        public string label { get; set; }

        public string typee { get; set; }

        public List<MusicTreeNode> children { get; set; } = new List<MusicTreeNode>();
    }


}
