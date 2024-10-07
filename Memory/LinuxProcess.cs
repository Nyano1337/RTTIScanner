using EnvDTE;
using EnvDTE80;
using RTTIScanner.Ifaces;
using System;
using System.Threading.Tasks;

namespace RTTIScanner.Memory
{
	public class LinuxProcess : DebugProcess
	{
		const int PTRACE_ATTACH = 16;
		const int PTRACE_DETACH = 17;
		const int PTRACE_PEEKDATA = 2;
		const int SYS_PTRACE = 101;

		public LinuxProcess(DTE2 dte) : base(dte) { }

		public override async Task<byte[]> ReadMemory(IntPtr address, int size)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			// 附加到目标进程  
			//LinuxAPI.ptrace(PTRACE_ATTACH, targetPid, IntPtr.Zero, IntPtr.Zero);
			//System.Threading.Thread.Sleep(100); // 等待进程附加  

			// 读取内存  
			//int data = LinuxAPI.ptrace(PTRACE_PEEKDATA, CurrentProcess.Id, address, IntPtr.Zero);

			// 从目标进程分离  
			//LinuxAPI.ptrace(PTRACE_DETACH, targetPid, IntPtr.Zero, IntPtr.Zero);

			return await Reader.GetInstance().GetBytesFromVSAsync(address, size);
		}
	}
}
