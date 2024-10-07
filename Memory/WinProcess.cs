using EnvDTE;
using EnvDTE80;
using RTTIScanner.ClassExtensions;
using RTTIScanner.Ifaces;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace RTTIScanner.Memory
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MEMORY_BASIC_INFORMATION
	{
		public IntPtr BaseAddress;
		public IntPtr AllocationBase;
		public uint AllocationProtect;
		public IntPtr RegionSize;
		public uint State;
		public uint Protect;
		public uint Type;
	}

	public static class MemoryProtectionConstants
	{
		public const uint PAGE_NOACCESS = 0x01;
		public const uint PAGE_READONLY = 0x02;
		public const uint PAGE_READWRITE = 0x04;
		public const uint PAGE_WRITECOPY = 0x08;
		public const uint PAGE_EXECUTE = 0x10;
		public const uint PAGE_EXECUTE_READ = 0x20;
		public const uint PAGE_EXECUTE_READWRITE = 0x40;
		public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
		public const uint PAGE_GUARD = 0x100;
		public const uint PAGE_NOCACHE = 0x200;
		public const uint PAGE_WRITECOMBINE = 0x400;
	}

	public class WinProcess : DebugProcess
	{
		public WinProcess(DTE2 dte) : base(dte) { }

		public override async Task<byte[]> ReadMemory(IntPtr address, int size)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			if (!address.IsValid())
			{
				throw new Exception($"Address {address} is not valid.");
			}

			if (CurrentProcess == null)
			{
				throw new Exception("当前进程为空!");
			}

			byte[] data = new byte[size];
			try
			{
				MEMORY_BASIC_INFORMATION memInfo = new();
				if (WinAPI.VirtualQueryEx(CurrentProcess.Handle, address, out memInfo, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == 0)
				{
					throw new Exception($"VirtualQueryEx failed!. Error reading process memory");
				}

				if (!WinAPI.ReadProcessMemory(CurrentProcess.Handle, address, data, size, out int bytesRead))
				{
					int errorCode = Marshal.GetLastWin32Error();
					throw new Exception($"ReadProcessMemory failed!. Error reading process memory: {errorCode}");
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}

			return data;
		}
	}
}
