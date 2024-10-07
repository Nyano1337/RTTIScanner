using EnvDTE;
using EnvDTE80;
using RTTIScanner.Memory;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RTTIScanner.RTTI
{
	public class Parser
	{
		private static Parser Instance;
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
	}
}
