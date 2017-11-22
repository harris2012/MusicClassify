using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MusicClassifyLibrary
{
    public class Mp3Classifier
    {
        private readonly byte[] ID3 = { 73, 68, 51 };

        private const int ContentMaxSize = 1024 * 1024 * 30;

        public MusicInfo GetMusicInfo(byte[] MusicContent)
        {
            if (MusicContent == null)
                throw new ArgumentNullException("MusicContent");

            if (MusicContent.Length < 21)
                throw new Exception("文件内容太短");

            if (MusicContent.Length > ContentMaxSize)
                throw new Exception("文件内容太大");

            for (int i = 0; i < ID3.Length; i++)
            {
                if (MusicContent[i] != ID3[i])
                    throw new Exception("文件格式不正确");
            }

            MusicInfo info = new MusicInfo();

            //byte ID3Version = content[3];//使用的ID3V2的版本,如果文件使用的是ID3V2.3,就记录3
            //byte ID2Version2 = content[4];//ID3V2的副版本号
            //byte flag = content[5];标志字节

            int FrameSize = MusicContent[6] * 0x200000 + MusicContent[7] * 0x4000 + MusicContent[8] * 0x80 + MusicContent[9];
            List<byte> Frames = MusicContent.Skip(10).Take(FrameSize).ToList();
            int index = 0;
            while (index < Frames.Count)
            {
                byte[] Id = Frames.Skip(index).Take(4).ToArray();
                if (Id.All(v => v == 0))
                    break;
                byte[] Size = Frames.Skip(index + 4).Take(4).ToArray();
                byte[] Flag = Frames.Skip(index + 8).Take(2).ToArray();
                int ContentSize = Size[0] * 0x1000000 + Size[1] * 0x10000 + Size[2] * 0x100 + Size[3];
                byte[] FrameContent = Frames.Skip(index + 10).Take(ContentSize).ToArray();
                switch (Encoding.ASCII.GetString(Id))
                {
                    case MusicClassifyConst.TIT2:
                        if (FrameContent[0] == 1)
                        {
                            info.Title = Encoding.Unicode.GetString(FrameContent, 3, FrameContent.Length - 3);
                        }
                        else if (FrameContent[0] == 0)
                        {
                            info.Title = Encoding.GetEncoding(MusicClassifyConst.GB2312).GetString(FrameContent, 1, FrameContent.Length - 1);
                        }
                        break;
                    case MusicClassifyConst.TPE1:
                        if (FrameContent[0] == 1)
                        {
                            info.Artist = Encoding.Unicode.GetString(FrameContent, 3, FrameContent.Length - 3);
                        }
                        else if (FrameContent[0] == 0)
                        {
                            info.Artist = Encoding.GetEncoding(MusicClassifyConst.GB2312).GetString(FrameContent, 1, FrameContent.Length - 1);
                        }
                        break;
                    case MusicClassifyConst.TALB:
                        if (FrameContent[0] == 1)
                        {
                            info.Album = Encoding.Unicode.GetString(FrameContent, 3, FrameContent.Length - 3);
                        }
                        else if (FrameContent[0] == 0)
                        {
                            info.Album = Encoding.GetEncoding(MusicClassifyConst.GB2312).GetString(FrameContent, 1, FrameContent.Length - 1);
                        }
                        break;
                    case MusicClassifyConst.TYER:
                        if (FrameContent[0] == 1)
                        {
                            info.Year = Encoding.Unicode.GetString(FrameContent, 3, FrameContent.Length - 3);
                        }
                        else
                        {
                            info.Year = Encoding.GetEncoding(MusicClassifyConst.GB2312).GetString(FrameContent, 1, FrameContent.Length - 1);
                        }
                        break;
                    default:
                        break;
                }
                index = index + ContentSize + 10;
            }

            return info;
        }
    }
}
