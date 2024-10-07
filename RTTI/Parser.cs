using EnvDTE;
using EnvDTE80;
using RTTIScanner.Memory;
using System.Threading.Tasks;

namespace RTTIScanner.RTTI
{
    public class Parser
    {
        private static Parser Instance;
        public static Parser GetInstace()
        {
            // FIXME: GCC
            Instance ??= new MSVC();
            return Instance;
        }

        public virtual async Task<string[]> ReadRuntimeTypeInformation(IntPtr address)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return null;
        }
    }
}
