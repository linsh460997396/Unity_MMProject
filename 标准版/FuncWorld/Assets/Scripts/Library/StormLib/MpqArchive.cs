using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace StormLib
{
    /// <summary>
	/// MPQ档案管理
	/// </summary>
    public class MpqArchive : IDisposable
	{
        // Token: 0x06000001 RID: 1
        [DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileOpenArchive(string szMpqName, uint dwPriority, uint dwFlags, out IntPtr phMPQ);

		// Token: 0x06000002 RID: 2
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileCloseArchive(IntPtr hMPQ);

		// Token: 0x06000003 RID: 3
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileRemoveFile(IntPtr hMpq, string szFileName, uint dwSearchScope);

		// Token: 0x06000004 RID: 4
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileAddFileEx(IntPtr hMpq, string szFileName, string szArchivedName, uint dwFlags, uint dwCompression, uint dwCompressionNext);

		/// <summary>
		/// [构造函数]MPQ档案管理
		/// </summary>
		/// <param name="filename"></param>
		/// <exception cref="IOException"></exception>
		public MpqArchive(string filename)
		{
            if (!SFileOpenArchive(filename, 0U, 0U, out this.mpq))
			{
				throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
			}
		}

        /// <summary>
        /// [析构函数]对象被垃圾回收(GC)之前自动调用,用于执行清理操作如释放非托管资源.
		/// disposing为true时显式地释放资源,否则表示正在由析构函数调用(隐式释放资源)
        /// </summary>
        ~MpqArchive()
		{
            //调用时间是不确定的,这取决于垃圾回收器的运行时机
            this.Dispose(false);//不释放那些可能仍在使用中的托管资源
                                //GC.SuppressFinalize(this) 可让垃圾回收器不要再调用析构函数,从而提高性能
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000020C8 File Offset: 0x000002C8
        public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020D8 File Offset: 0x000002D8
		protected void Dispose(bool disposing)
		{
			if (this.mpq != IntPtr.Zero)
			{
				if (!MpqArchive.SFileCloseArchive(this.mpq))
				{
					throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
				}
				this.mpq = IntPtr.Zero;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002126 File Offset: 0x00000326
		public MpqStream OpenFile(string filename)
		{
			return new MpqStream(this.mpq, filename);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002134 File Offset: 0x00000334
		public void RemoveFile(string filename)
		{
			if (!MpqArchive.SFileRemoveFile(this.mpq, filename, 0U))
			{
				throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002168 File Offset: 0x00000368
		public void AddFile(string filename, string archivedName, bool replaceExisting)
		{
			if (!MpqArchive.SFileAddFileEx(this.mpq, filename, archivedName, replaceExisting ? 2147483648U : 0U, 0U, 0U))
			{
				throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
			}
		}

		// Token: 0x04000001 RID: 1
		private const uint MPQ_FILE_REPLACEEXISTING = 2147483648U;

		// Token: 0x04000002 RID: 2
		private IntPtr mpq = IntPtr.Zero;
	}
}
