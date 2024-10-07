using EnvDTE;
using EnvDTE80;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using RTTIScanner.ClassExtensions;
using RTTIScanner.vsix;
using RTTIScanner.Memory;
using Microsoft.Diagnostics.Runtime;
using RTTIScanner.RTTI;


namespace RTTIScanner.Implement
{
    [Command(PackageIds.Window)]
    internal sealed class RTTIScanner : BaseCommand<RTTIScanner>
    {
        private TextBox AddressInputBox;
        private static TextBox RTTIShowBox;
        private Form toolWindow;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            SetupUI();
        }

        private void SetupUI()
        {
            AddressInputBox = new TextBox
            {
                Multiline = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Consolas", 12, FontStyle.Regular)
            };
            AddressInputBox.KeyPress += OnKeyPress_Enter;

            RTTIShowBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Consolas", 12, FontStyle.Regular)
            };
            RTTIShowBox.TextChanged += RTTIShowBox_OnTextChanged;

            var tableLayoutPanel = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 1,
                Dock = DockStyle.Fill
            };
            tableLayoutPanel.Controls.Add(AddressInputBox, 0, 0);
            tableLayoutPanel.Controls.Add(RTTIShowBox, 0, 1);

            string projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            toolWindow = new Form
            {
                Text = "RTTI Scanner",
                Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + ".Resources" + ".ReClass.ico")),
                TopMost = true
            };

            toolWindow.Controls.Add(tableLayoutPanel);
            toolWindow.Show();
        }

#pragma warning disable VSTHRD100
        private async void OnKeyPress_Enter(object sender, KeyPressEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (e.KeyChar == (char)Keys.Enter)
            {
                // 禁用系统的默认回车键行为, 如 取消提示音
                e.Handled = true;

                string context = AddressInputBox.Text;
                if (context.Length == 0)
                {
                    return;
                }

                await DoScanRTTI(context);
            }
        }
#pragma warning restore VSTHRD100

        private async Task DoScanRTTI(string context)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                DebugProcess debugProcess = Memory.DebugProcess.GetInstance();
                if (debugProcess.DTE.Debugger.CurrentMode == dbgDebugMode.dbgDesignMode)
                {
                    await VS.MessageBox.ShowWarningAsync("调试器没有启动!");
                    return;
                }

                Memory.Reader memReader = Memory.Reader.GetInstance();

                if (debugProcess.DTE.Debugger.CurrentProcess?.Name.EndsWith(".mdmp") ?? false)
                {
                    debugProcess.IsMinidump = true;
                    memReader.IsMinidump = true;
                }

                await debugProcess.Init();

                IntPtr pointer = Memory.Reader.ParseAddress(context);
                if (!pointer.IsValid())
                {
                    ErrorResult($"Invalid Address {pointer}");
                    return;
                }


                IntPtr remotePtr = await memReader.GetPtr(pointer, 8);
                string[] rtti = await RTTI.Parser.GetInstace().ReadRuntimeTypeInformation(remotePtr);
                if (rtti == null || rtti.Length == 0)
                {
                    ErrorResult($"Unknown Structure");
                    return;
                }

                RTTIShowBox.Clear();
                foreach (string className in rtti)
                {
                    AppendResult(className);
                }
            }
            catch (Exception ex)
            {
                ErrorResult(ex.Message);
                return;
            }
        }

        private void RTTIShowBox_OnTextChanged(object sender, EventArgs e)
        {
            var preferredSize = RTTIShowBox.GetPreferredSize(new Size(int.MaxValue, int.MaxValue));
            toolWindow.Size = new Size(
                Math.Max(toolWindow.Width, preferredSize.Width + 30),
                Math.Max(toolWindow.Height, preferredSize.Height + 20)
            );
        }

        public static void ErrorResult(string text)
        {
            RTTIShowBox.Text = text;
        }

        public static void AppendResult(string text)
        {
            RTTIShowBox.Text += (RTTIShowBox.Text.Length > 0 ? Environment.NewLine : "") + text;
        }
    }
}
