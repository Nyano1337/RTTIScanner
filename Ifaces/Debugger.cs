using Microsoft.VisualStudio.Debugger.Interop;

namespace RTTIScanner.Ifaces
{
    public class Debugger
    {
        private static Debugger Instance;
        private Debugger() { }
        public static Debugger GetInstance()
        {
            return Instance ??= new Debugger();
        }

        public IDebugEngine2 Engine { get; private set; }
        public IDebugProcess2 Process { get; private set; }
        public IDebugProgram2 Program { get; private set; }
        public IDebugThread2 MainThread { get; private set; }
        public IDebugEvent2 DebugEvent { get; private set; }

        public void Update(IDebugEngine2 engine, IDebugProcess2 process, IDebugProgram2 program, IDebugThread2 thread, IDebugEvent2 debugEvent)
        {
            if (engine != null) Engine = engine;
            if (process != null) Process = process;
            if (program != null) Program = program;
            if (thread != null) MainThread ??= thread;
            if (debugEvent != null) DebugEvent = debugEvent;
        }
    }
}
