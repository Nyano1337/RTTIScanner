global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using RTTIScanner.vsix;
using System.Runtime.InteropServices;
using System.Threading;

namespace RTTIScanner
{
	[Guid(PackageGuids.RTTIScannerString)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(UIContextGuids80.NoSolution)] // sync autoload will be deprecated in the future
	[ProvideAutoLoad(UIContextGuids80.SolutionBuilding)] // but at least it works perfects now
	[ProvideAutoLoad(UIContextGuids80.Debugging)] // i have no idea at now.
	public sealed class RTTIScannerPackage : ToolkitPackage
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			await this.RegisterCommandsAsync();

			IVsDebugger debugger = (IVsDebugger)await GetServiceAsync(typeof(SVsShellDebugger));
			ErrorHandler.ThrowOnFailure(debugger?.AdviseDebugEventCallback(new DebugEventCallback()) ?? -1);
			ErrorHandler.ThrowOnFailure(debugger?.AdviseDebuggerEvents(new DebugEventCallback(), out uint pdwCookie) ?? -1);
		}

		public class DebugEventCallback : IDebugEventCallback2, IVsDebuggerEvents
		{
			public int Event(IDebugEngine2 engine, IDebugProcess2 process, IDebugProgram2 program, IDebugThread2 thread, IDebugEvent2 debugEvent, ref Guid riidEvent, uint attributes)
			{
				Ifaces.Debugger.GetInstance().Update(engine, process, program, thread, debugEvent);
				return VSConstants.S_OK;
			}

			public int OnModeChange(DBGMODE dbgmodeNew)
			{
				switch (dbgmodeNew)
				{
					case DBGMODE.DBGMODE_Break:
					{
						ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
						{
							await Ifaces.Debugger.GetInstance().OnEnterBreakMode();
						}).FileAndForget("RTTIScanner/OnEnterBreakMode");
						break;
					}
					case DBGMODE.DBGMODE_Design:
					{
						Ifaces.Debugger.GetInstance().OnEnterDesignMode();
						break;
					}
				}

				return VSConstants.S_OK;
			}
		}
	}
}