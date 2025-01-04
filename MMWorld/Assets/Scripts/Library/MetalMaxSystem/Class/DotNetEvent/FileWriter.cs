using System.IO;
using System.Text;

namespace MetalMaxSystem
{
    /// <summary>
    /// 处理文件写入的类。平时在StringBuilder中积累字符，最后将StringBuilder中的字符写入文件。
    /// </summary>
    public class FileWriter
    {
        /// <summary>
        /// StringBuilder
        /// </summary>
        private StringBuilder _buffer = new StringBuilder();
        /// <summary>
        /// 写入文件路径
        /// </summary>
        private string _path;
        /// <summary>
        /// false覆盖文件，true向文件末尾追加文本
        /// </summary>
        private bool _fileAppend;
        /// <summary>
        /// 编码
        /// </summary>
        private Encoding _encode;
        /// <summary>
        /// StreamWriter缓冲区大小（若填-1或不设置则默认8192个字节，满时自动写入文件）
        /// </summary>
        private int _bufferSize;
        /// <summary>
        /// false是StreamWriter，true是File.WriteAllText，默认为true
        /// </summary>
        private bool _writeStyle = true;

        public StringBuilder Buffer { get => _buffer; set => _buffer = value; }
        public string Path { get => _path; set => _path = value; }
        public bool FileAppend { get => _fileAppend; set => _fileAppend = value; }
        public Encoding Encode { get => _encode; set => _encode = value; }
        public int BufferSize { get => _bufferSize; set => _bufferSize = value; }
        public bool WriteStyle { get => _writeStyle; set => _writeStyle = value; }

        /// <summary>
        /// 在StringBuilder中写入字符每行。函数在行尾自动添加一个换行符（通常是\n，但在Windows上可能是\r\n，即回车加换行）。
        /// </summary>
        /// <param name="value">字符每行</param>
        /// <param name="cover">是否覆盖缓冲区（即写入前是否清理StringBuilder）</param>
        public void WriteLine(string value, bool cover = false)
        {
            if (cover) Buffer.Clear();
            Buffer.AppendLine(value);
        }

        /// <summary>
        /// 在StringBuilder中写入字符。
        /// </summary>
        /// <param name="value">字符</param>
        /// <param name="cover">是否覆盖缓冲区（即写入前是否清理StringBuilder）</param>
        public void Write(string value, bool cover = false)
        {
            if (cover) Buffer.Clear();
            Buffer.Append(value);
        }

        /// <summary>
        /// 若StringBuilder有内容则立即将其全部写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        /// <param name="encoding"></param>
        /// <param name="clean">最后是否清理StringBuilder缓冲区</param>
        /// <param name="writeStyle"></param>
        /// <param name="bufferSize"></param>
        private void FlushBuffer(string path, bool fileAppend, Encoding encoding, bool clean = false, bool writeStyle = true, int bufferSize = 8192)
        {
            //属性刷新
            if (Path != path) Path = path;
            if (FileAppend != fileAppend) FileAppend = fileAppend;
            if (Encode != encoding) Encode = encoding;
            if (BufferSize != bufferSize) BufferSize = bufferSize;
            if (WriteStyle != writeStyle) WriteStyle = writeStyle;

            if (Buffer.Length > 0)
            {
                if (WriteStyle)
                {
                    if (FileAppend)
                    {//追加（不会自动换行，需自行处理添加内容）
                        File.AppendAllText(Path, Buffer.ToString(), Encode);
                    }
                    else
                    {//覆盖
                        File.WriteAllText(Path, Buffer.ToString(), Encode);
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(Path, FileAppend, Encode, BufferSize))
                    {//不会自动换行，需自行处理添加内容
                        sw.Write(Buffer.ToString());
                        //using块：动作末尾当Stream文件流对象被销毁时，Dispose会检查是否已调用Flush，如果没有它会自动调用Flush确保所有缓冲数据都被写入到文件或其他Stream文件流中
                        //如果不写using块，那么本类需要实现IDisposable接口方法来手动Dispose
                    }

                }
                if (clean)
                {
                    //清空StringBuilder缓冲区
                    Buffer.Clear();
                }
            }
        }

        /// <summary>
        /// 强制将缓冲区内容写入文件，不清理StringBuilder缓冲区
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        /// <param name="encoding"></param>
        /// <param name="writeStyle"></param>
        /// <param name="bufferSize"></param>
        public void Flush(string path, bool fileAppend, Encoding encoding, bool writeStyle = true, int bufferSize = 8192)
        {
            FlushBuffer(path, fileAppend, encoding, false, writeStyle, bufferSize);
        }

        /// <summary>
        /// 强制将缓冲区内容写入文件，最后清理StringBuilder缓冲区
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        /// <param name="encoding"></param>
        /// <param name="writeStyle"></param>
        /// <param name="bufferSize"></param>
        public void Close(string path, bool fileAppend, Encoding encoding, bool writeStyle = true, int bufferSize = 8192)
        {
            FlushBuffer(path, fileAppend, encoding, true, writeStyle, bufferSize);
        }

        /// <summary>
        /// 清空StringBuilder缓冲区
        /// </summary>
        public void Clear()
        {
            Buffer.Clear();
        }
    }
}
