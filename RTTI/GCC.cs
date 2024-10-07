using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTTIScanner.RTTI
{
	public class GCC : Parser
	{
		public override async Task<string[]> ReadRuntimeTypeInformation(IntPtr address)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			throw new Exception("GCC ReadRuntimeTypeInformation not impl");
		}
	}
}
