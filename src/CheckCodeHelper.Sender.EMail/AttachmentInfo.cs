using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCodeHelper.Sender.EMail
{
    /// <summary>
    /// 邮件附件信息
    /// </summary>
    public sealed class AttachmentInfo : IDisposable
    {
        /// <summary>
        /// 释放<see cref="Stream"/>
        /// </summary>
        ~AttachmentInfo()
        {
            this.Dispose();
        }

        /// <summary>
        /// 附件类型
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件传输编码方式
        /// </summary>
        public ContentEncoding ContentTransferEncoding { get; set; } = ContentEncoding.Default;
        /// <summary>
        /// 文件数组
        /// </summary>
        public byte[] Data { get; set; }
        private Stream _stream;
        /// <summary>
        /// 文件数据流
        /// </summary>
        public Stream Stream
        {
            get
            {
                if (this._stream == null && this.Data != null)
                {
                    _stream = new MemoryStream(this.Data);
                }
                return this._stream;
            }
            set { this._stream = value; }
        }
        /// <summary>
        /// 释放Stream
        /// </summary>
        public void Dispose()
        {
            if (this._stream != null)
            {
                this._stream.Dispose();
            }
        }
    }
}
