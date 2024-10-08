using RTTIScanner.ClassExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace __cxxabiv1
{
	public class type_info
	{
		// TODO: Full GCC __cxa_demangle
		public static string __cxa_demangle(string str)
		{
			return new Regex(@"^\d+").Replace(str, "");
		}
	}
	public class __class_type_info : type_info
	{

	}
	public class __si_class_type_info : __class_type_info
	{

	}
	public class __vmi_class_type_info : __class_type_info
	{

	}
}

namespace RTTIScanner.RTTI
{
	public class GCC : Parser
	{
		public override async Task<string[]> ReadRemoteRuntimeTypeInformation64(IntPtr typeInfo)
		{
			if (!typeInfo.IsValid())
			{
				return null;
			}

			string typeName = __cxxabiv1.type_info.__cxa_demangle(await ReadRemoteString(await ReadRemoteIntPtr(typeInfo + 0x8)));

			// TODO: Full RTTI
			return new string[] { typeName };
		}
	}
}
