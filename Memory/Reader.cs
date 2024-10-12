using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using RTTIScanner.Implement;
using Debugger = RTTIScanner.Ifaces.Debugger;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;

namespace RTTIScanner.Memory
{
	public class Reader
	{
		private static Reader Instance;
		public bool IsMinidump { get; set; }

		private Reader() { }

		public static Reader GetInstance()
		{
			return Instance ??= new Reader();
		}

		// 64bit
		public static IntPtr ParseAddress(string addressString)
		{
			if (addressString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				addressString = addressString.Substring(2);
			}

			try
			{
				return new IntPtr(long.Parse(addressString, System.Globalization.NumberStyles.HexNumber));
			}
			catch (Exception ex)
			{
				throw new Exception($"Error parsing address: {ex.Message}");
			}
		}

		public async Task<T> GetValue<T>(IntPtr pointer, int offset = 0)
		{
			if (typeof(T) == typeof(string))
			{
				byte[] buffer = await GetBytes(pointer + offset, 60);
				if (buffer == null)
				{
					return default;
				}

				int length = Array.IndexOf(buffer, (byte)0);
				if (length == -1)
				{
					length = buffer.Length;
				}

				return (T)(object)Encoding.UTF8.GetString(buffer, 0, length);
			}

			int size = Marshal.SizeOf<T>();
			byte[] data = await GetBytes(pointer + offset, size);
			if (data == null)
			{
				return default;
			}

			if (typeof(T) == typeof(ulong))
			{
				return (T)(object)BitConverter.ToUInt64(data, 0);
			}
			else if (size == sizeof(long))
			{
				return typeof(T) == typeof(IntPtr) ? (T)(object)(IntPtr)BitConverter.ToInt64(data, 0) : (T)(object)BitConverter.ToInt64(data, 0);
			}
			else if (typeof(T) == typeof(uint))
			{
				return (T)(object)BitConverter.ToUInt32(data, 0);
			}
			else if (size == sizeof(int))
			{
				return typeof(T) == typeof(IntPtr) ? (T)(object)(IntPtr)BitConverter.ToInt32(data, 0) : (T)(object)BitConverter.ToInt32(data, 0);
			}
			else if (typeof(T) == typeof(ushort))
			{
				return (T)(object)BitConverter.ToUInt16(data, 0);
			}
			else if (size == sizeof(short))
			{
				return (T)(object)BitConverter.ToInt16(data, 0);
			}
			else if (size == sizeof(byte))
			{
				return (T)(object)data[0];
			}
			else
			{
				throw new NotSupportedException($"Type size {size} is not supported.");
			}
		}

		public async Task<byte[]> GetBytes(IntPtr pointer, int size)
		{
			return IsMinidump ? await GetBytesFromVSAsync(pointer, size) : await DebugProcess.GetInstance().ReadMemory(pointer, size);
		}

		public async Task<byte[]> GetBytesFromVSAsync(IntPtr pointer, int size)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			string expression = "0x" + pointer.ToString("X");
			Debugger debugger = Debugger.GetInstance();
			if (debugger.MainThread == null)
			{
				throw new Exception("获取主线程失败!\n(这通常发生在扩展初始化晚于调试器启动, 请重启VS!)");
			}

			IDebugStackFrame2 debugStackFrame = GetTopStackFrame(debugger.MainThread);
			if (debugStackFrame == null)
			{
				throw new Exception("获取栈帧失败!");
			}

			if (debugStackFrame.GetExpressionContext(out IDebugExpressionContext2 expressionContext) != VSConstants.S_OK)
			{
				throw new Exception("获取IDebugExpressionContext2失败!");
			}

			if (expressionContext.ParseText(expression, enum_PARSEFLAGS.PARSE_EXPRESSION, 16, out IDebugExpression2 debugExpression, out _, out _) != VSConstants.S_OK)
			{
				throw new Exception("获取IDebugExpression2失败!");
			}

			if (debugExpression.EvaluateSync(enum_EVALFLAGS.EVAL_NOSIDEEFFECTS, uint.MaxValue, null, out IDebugProperty2 debugProperty) != VSConstants.S_OK)
			{
				throw new Exception("获取IDebugProperty2失败!");
			}

			if (debugProperty.GetMemoryContext(out IDebugMemoryContext2 memoryContext) != VSConstants.S_OK)
			{
				throw new Exception("获取IDebugMemoryContext2失败!");
			}

			if (debugger.Program == null)
			{
				throw new Exception("获取Program失败!");
			}

			if (debugger.Program.GetMemoryBytes(out IDebugMemoryBytes2 debugMemoryBytes2) != VSConstants.S_OK)
			{
				throw new Exception("获取IDebugMemoryBytes2失败!");
			}

			byte[] data = new byte[size];
			uint unreadableBytes = 0;
			if (debugMemoryBytes2.ReadAt(memoryContext, (uint)size, data, out _, ref unreadableBytes) != VSConstants.S_OK)
			{
				throw new Exception("从VS读取内存失败!");
			}

			return data;
		}

		private IDebugStackFrame2 GetTopStackFrame(IDebugThread2 thread)
		{
			thread.EnumFrameInfo(enum_FRAMEINFO_FLAGS.FIF_FRAME, 0, out IEnumDebugFrameInfo2 enumFrameInfo);
			FRAMEINFO[] frameInfo = new FRAMEINFO[1];
			uint fetched = 0;
			enumFrameInfo.Next(1, frameInfo, ref fetched);
			return fetched > 0 ? frameInfo[0].m_pFrame : null;
		}
	}
}
