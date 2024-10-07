using System.Runtime.InteropServices;

namespace RTTIScanner.Ifaces
{
	public static class LinuxAPI
	{
		[DllImport("libc.so.6")]
		public static extern int ptrace(int request, int pid, IntPtr addr, IntPtr data);

		[DllImport("libc.so.6")]
		public static extern int syscall(int number, int request, int pid, IntPtr addr, IntPtr data);

	}
}
