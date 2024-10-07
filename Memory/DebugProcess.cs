using EnvDTE;
using EnvDTE80;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace RTTIScanner.Memory
{
	public class DebugProcess : IDisposable
	{
		private static DebugProcess Instance;
		private bool disposedValue;

		public bool IsMinidump { get; set; }
		public Process CurrentProcess { get; private set; }
		public DTE2 DTE { get; private set; }

		public DebugProcess(DTE2 dte)
		{
			DTE = dte;
		}

		public static DebugProcess GetInstance(bool setup = false)
		{
			if (setup)
			{
				Instance = new DebugProcess((DTE2)Package.GetGlobalService(typeof(DTE)));
			}

			return Instance;
		}

		public async Task<string> GetProcessName()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			return DTE.Debugger.CurrentProcess?.Name;
		}
		public async Task<OSPlatform> GetPlatform()
		{
			string processName = await GetProcessName();
			if (processName == null)
			{
				throw new Exception("ProcessName is null");
			}

			return processName.EndsWith(".exe") ? OSPlatform.Windows : OSPlatform.Linux;
		}

		public virtual async Task<byte[]> ReadMemory(IntPtr address, int size)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			throw new Exception("ReadMemory pure call");
		}

		public async Task Init()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			OSPlatform platform = await GetPlatform();
			try
			{
				if (platform == OSPlatform.Windows)
				{
					Instance = new WinProcess(DTE);
				}
				else
				{
					Instance = new LinuxProcess(DTE);
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}

			if (IsMinidump)
			{
				Instance.CurrentProcess = Process.GetCurrentProcess();
			}

			// A weird way to get current process but i dont have no idea.
			// The debugger.CurrentProcess is always null.
			foreach (EnvDTE.Process process in DTE.Debugger.DebuggedProcesses)
			{
				if (process == null)
				{
					continue;
				}

				try
				{
					Instance.CurrentProcess = Process.GetProcessById(process.ProcessID);
					return;
				}
				catch (Exception ex)
				{
					throw new Exception($"Catched error getting process by id: {ex.Message}");
				}
			}

			Instance.CurrentProcess = null;
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
		// ~DebugProcess()
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
