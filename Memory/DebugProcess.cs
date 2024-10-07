using EnvDTE;
using EnvDTE80;
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

        public static DebugProcess GetInstance()
        {
            // FIXME: linux
            Instance ??= new WinProcess((DTE2)Package.GetGlobalService(typeof(DTE)));
            return Instance;
        }

        public virtual async Task<byte[]> ReadMemory(IntPtr address, int size)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return null;
        }

        public async Task<Process> InitCurrentDebugProcess()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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
