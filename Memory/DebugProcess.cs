using EnvDTE;
using EnvDTE80;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace RTTIScanner.Memory
{
	public class DebugProcess
	{
		private static DebugProcess Instance;
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

		public async Task<Process> Init()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			OSPlatform platform = await GetPlatform();
			if (platform == OSPlatform.Windows)
			{
				Instance = (WinProcess)Instance;
			}
			else
			{
				Instance = (LinuxProcess)Instance;
			}

			if (IsMinidump)
			{
				CurrentProcess = Process.GetCurrentProcess();
				return CurrentProcess;
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
					CurrentProcess = Process.GetProcessById(process.ProcessID);
					return CurrentProcess;
				}
				catch (Exception ex)
				{
					throw new Exception($"Catched error getting process by id: {ex.Message}");
				}
			}

			CurrentProcess = null;
			return CurrentProcess;
		}
	}
}
