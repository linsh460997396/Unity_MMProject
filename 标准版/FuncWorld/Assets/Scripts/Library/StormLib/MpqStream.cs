using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace StormLib
{
	// Token: 0x02000003 RID: 3
	public class MpqStream : Stream
	{
		// Token: 0x0600000C RID: 12
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileOpenFileEx(IntPtr hMpq, string szFileName, uint dwSearchScope, out IntPtr phFile);

		// Token: 0x0600000D RID: 13
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileCloseFile(IntPtr hFile);

		// Token: 0x0600000E RID: 14
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern uint SFileGetFileSize(IntPtr hFile, out uint pdwFileSizeHigh);

		// Token: 0x0600000F RID: 15
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern uint SFileSetFilePointer(IntPtr hFile, int lFilePos, ref int plFilePosHigh, uint dwMoveMethod);

		// Token: 0x06000010 RID: 16
		[DllImport("StormLib.dll", SetLastError = true)]
		private static extern bool SFileReadFile(IntPtr hFile, out byte lpBuffer, uint dwToRead, out uint pdwRead, IntPtr lpOverlapped);

		// Token: 0x06000011 RID: 17 RVA: 0x000021A8 File Offset: 0x000003A8
		internal MpqStream(IntPtr mpq, string filename)
		{
			if (!MpqStream.SFileOpenFileEx(mpq, filename, 0U, out this.file))
			{
				throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000021E4 File Offset: 0x000003E4
		protected override void Dispose(bool disposing)
		{
			if (this.file != IntPtr.Zero)
			{
				if (!MpqStream.SFileCloseFile(this.file))
				{
					throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
				}
				this.file = IntPtr.Zero;
			}
			base.Dispose(disposing);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000013 RID: 19 RVA: 0x00002239 File Offset: 0x00000439
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000014 RID: 20 RVA: 0x0000223C File Offset: 0x0000043C
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000015 RID: 21 RVA: 0x0000223F File Offset: 0x0000043F
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000016 RID: 22 RVA: 0x00002244 File Offset: 0x00000444
		public override long Length
		{
			get
			{
				uint num2;
				uint num = MpqStream.SFileGetFileSize(this.file, out num2);
				if (num == 4294967295U)
				{
					throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
				}
				return (long)((ulong)num);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000017 RID: 23 RVA: 0x0000227A File Offset: 0x0000047A
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002282 File Offset: 0x00000482
		public override long Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.Seek(value, SeekOrigin.Begin);
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x0000228D File Offset: 0x0000048D
		public override void Flush()
		{
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002290 File Offset: 0x00000490
		public override long Seek(long offset, SeekOrigin origin)
		{
			MpqStream.MoveMethod dwMoveMethod;
			switch (origin)
			{
			case SeekOrigin.Begin:
				dwMoveMethod = MpqStream.MoveMethod.FileBegin;
				break;
			case SeekOrigin.Current:
				dwMoveMethod = MpqStream.MoveMethod.FileCurrent;
				break;
			case SeekOrigin.End:
				dwMoveMethod = MpqStream.MoveMethod.FileEnd;
				break;
			default:
				throw new NotSupportedException();
			}
			int num = 0;
			uint num2 = MpqStream.SFileSetFilePointer(this.file, (int)offset, ref num, (uint)dwMoveMethod);
			if (num2 == 4294967295U)
			{
				throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
			}
			switch (origin)
			{
			case SeekOrigin.Begin:
				this.position = offset;
				break;
			case SeekOrigin.Current:
				this.position += offset;
				break;
			case SeekOrigin.End:
				this.position = (long)((ulong)num2 - (ulong)offset);
				break;
			}
			return this.position;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002332 File Offset: 0x00000532
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000233C File Offset: 0x0000053C
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (offset < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException("offset or count is negative");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is null");
			}
			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			}
			uint num = 0U;
			bool flag = MpqStream.SFileReadFile(this.file, out buffer[offset], (uint)count, out num, IntPtr.Zero);
			if (flag)
			{
				num = (uint)count;
			}
			else if (Marshal.GetLastWin32Error() != 38)
			{
				throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
			}
			this.position += (long)((ulong)num);
			return (int)num;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000023CF File Offset: 0x000005CF
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		// Token: 0x04000003 RID: 3
		private const uint SFILE_INVALID_SIZE = 4294967295U;

		// Token: 0x04000004 RID: 4
		private const int ERROR_HANDLE_EOF = 38;

		// Token: 0x04000005 RID: 5
		private IntPtr file;

		// Token: 0x04000006 RID: 6
		private long position;

		// Token: 0x02000004 RID: 4
		private enum MoveMethod
		{
			// Token: 0x04000008 RID: 8
			FileBegin,
			// Token: 0x04000009 RID: 9
			FileCurrent,
			// Token: 0x0400000A RID: 10
			FileEnd
		}
	}
}
