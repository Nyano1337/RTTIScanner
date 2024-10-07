using EnvDTE;
using EnvDTE80;
using RTTIScanner.Memory;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RTTIScanner.RTTI
{
	public class Parser : IDisposable
	{
		private static Parser Instance;
		private bool disposedValue;

		public static Parser GetInstace()
		{
			return Instance;
		}

		public static void Init(OSPlatform platform)
		{
			if (platform == OSPlatform.Windows)
			{
				Instance = new MSVC();
			}
			else
			{
				Instance = new GCC();
			}
		}

		public virtual async Task<string[]> ReadRuntimeTypeInformation(IntPtr address)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			throw new Exception("ReadRuntimeTypeInformation pure call");
		}

		public async Task<IntPtr> ReadRemoteIntPtr(IntPtr address)
		{
			try
			{
#if RTTISCANNER64
				return (IntPtr)await ReadRemoteInt64(address);
#else
				return (IntPtr)await ReadRemoteInt32(address);
#endif
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}
		}

		public async Task<long> ReadRemoteInt64(IntPtr address)
		{
			try
			{
				var data = await Memory.Reader.GetInstance().GetBytes(address, sizeof(long));

				return BitConverter.ToInt64(data, 0);
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}
		}

		public async Task<int> ReadRemoteInt32(IntPtr address)
		{
			try
			{
				var data = await Memory.Reader.GetInstance().GetBytes(address, sizeof(int));

				return BitConverter.ToInt32(data, 0);
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)
				}

				// TODO: 释放未托管的资源(未托管的对象)并重写终结器
				// TODO: 将大型字段设置为 null
				disposedValue = true;
			}
		}

		// // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
		// ~Parser()
		// {
		//     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
