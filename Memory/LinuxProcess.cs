using EnvDTE80;
using System;
using System.Threading.Tasks;

namespace RTTIScanner.Memory
{
	public class LinuxProcess : DebugProcess
	{
		public LinuxProcess(DTE2 dte) : base(dte) { }

	}
}
