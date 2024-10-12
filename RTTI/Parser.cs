using EnvDTE;
using EnvDTE80;
using RTTIScanner.ClassExtensions;
using RTTIScanner.Memory;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RTTIScanner.RTTI
{
	public class Parser : IDisposable
	{
		private static Parser Instance;
		private bool disposedValue;
		protected HashSet<string> m_hsTypeVisited;
		protected Dictionary<string, IntPtr> m_dictTypeAddress;
		protected Stack<string> m_sInheritance;

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

		public async Task<string[]> ReadRuntimeTypeInformation(IntPtr vtableAddress)
		{
			if (!vtableAddress.IsValid())
			{
				return null;
			}

			try
			{
				string[] rtti = null;
				var typeInfoPtr = await ReadRemoteIntPtr(vtableAddress - IntPtr.Size);
				if (typeInfoPtr.IsValid())
				{
#if RTTISCANNER64
					rtti = await ReadRemoteRuntimeTypeInformation64(typeInfoPtr);
#else
					rtti = await ReadRemoteRuntimeTypeInformation32(typeInfoPtr);
#endif
				}

				return rtti;
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}
		}

		public virtual async Task<string[]> ReadRemoteRuntimeTypeInformation64(IntPtr address)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			throw new NotImplementedException("ReadRemoteRuntimeTypeInformation64 pure call");
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

		public async Task<string> ReadRemoteString(IntPtr address)
		{
			try
			{
				return await Memory.Reader.GetInstance().GetValue<string>(address);
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}
		}

		public async Task<T> ReadRemote<T>(IntPtr address)
		{
			try
			{
				return await Memory.Reader.GetInstance().GetValue<T>(address);
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
