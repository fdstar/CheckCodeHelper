using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.Sms.Utils
{
    /// <summary>
    /// GZip辅助类
    /// </summary>
    public static class GZipHelper
    {
        /// <summary>
        /// Gzip压缩
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (var stream = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                    return ms.ToArray();
                }
            }
        }
        /// <summary>
        /// Gzip解压
        /// </summary>
        /// <param name="data">待解密的数据</param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (Stream inStream = new GZipStream(ms, CompressionMode.Decompress))
                using (MemoryStream outStream = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    while (true)
                    {
                        int bytesRead = inStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead <= 0)
                            break;
                        else
                            outStream.Write(buffer, 0, bytesRead);
                    }
                    return outStream.ToArray();
                }
            }
        }
    }
}
