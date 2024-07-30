using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYT.MidiManager
{
    public class MidiFile : IDisposable
    {

        ~MidiFile()
        {
            Dispose(false);//仅释放非托管资源
        }

        public MidiMusic Music { get; set; }

        /// <summary>
        /// 检测冗余调用
        /// </summary>
        private bool _isDispose = false;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDispose)
            {
                if (disposing)
                {
                    if (Tracks != null)
                    {
                        Tracks.Clear();
                        Tracks = null;
                    }
                    _properties = null;
                }

                //释放非托管资源


                _isDispose = true;// 标识此对象已释放
                GC.Collect();
            }
        }



        #region ■------------------ 字段属性

        private bool disposed = false;

        /// <summary>
        /// 音轨数量
        /// </summary>
        public int Count
        {
            get
            {
                #region Require

                if (disposed)
                {
                    throw new ObjectDisposedException("Sequence");
                }

                #endregion

                return Tracks.Count;
            }
        }

        public int Division
        {
            get { return _properties.Division; }
        }

        /// <summary>
        /// 每四分音符的时长 微秒
        /// </summary>
        public double Tempo { get; set; } = 500000;

        /// <summary>
        /// MIDI 每个Tick对应的微秒数
        /// </summary>
        public double PerTickMicrosecond
        {
            get 
            {
                return Tempo / Division;
            }
        }

        /// <summary>
        /// 音轨集合
        /// </summary>
        public List<Track> Tracks = new List<Track>();

        /// <summary>
        /// 文件信息
        /// </summary>
        private MidiFileProperties _properties = new MidiFileProperties();

        #endregion

        #region ■------------------ 从MIDI文件或流读取音轨和文件属性

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open,
                FileAccess.Read, FileShare.Read);
            Load(stream);
        }

        public void Load(byte[] filedata)
        {
            MemoryStream memory = new MemoryStream(filedata);
            Load(memory);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="stream"></param>
        public void Load(Stream stream)
        {
            using (stream)
            {
                MidiFileProperties newProperties = new MidiFileProperties();
                TrackReader reader = new TrackReader();
                List<Track> newTracks = new List<Track>();

                newProperties.Read(stream);
                //MidiControl.Instance.OnLog($"【MIDI控制器】文件属性 ");
                for (int i = 0; i < newProperties.TrackCount; i++)
                {
                    reader.Read(stream);
                    newTracks.Add(reader.Track);

                    if (reader.Track.Tempo!=-1)
                    {
                        MidiControl.Instance.OnLog($"【音轨加载】{Music.Name}  {reader.Track.Tempo}");
                        Tempo = reader.Track.Tempo;
                    }
                }

                _properties = newProperties;
                Tracks = newTracks;
            }

            #region Ensure

            Debug.Assert(Count == _properties.TrackCount);

            #endregion
        }

        #endregion

    }
}
