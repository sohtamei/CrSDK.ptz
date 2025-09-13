using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace appPtz2
{
    public partial class Form1 : Form
    {
        const string DLLPath = "RemoteCli.dll";


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LiveviewCbDelegate(int eventId);

        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RegisterLiveviewCb(LiveviewCbDelegate liveviewCb);


        private static Form1 _instance;

        private static LiveviewCbDelegate liveviewCb = new LiveviewCbDelegate(OnLiveviewCb);


        public Form1()
        {
            InitializeComponent();
            _instance = this;

            RegisterLiveviewCb(liveviewCb);
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int RemoteCli_connect([MarshalAs(UnmanagedType.LPStr)] string inputLine);

        private void connect_Click(object sender, EventArgs e)
        {
            disconnect.Enabled = false;
            connect.Enabled = false;
            int data = RemoteCli_connect(txtConnect.Text);
            if(data == 0) {
                disconnect.Enabled = true;
            } else
            {
                connect.Enabled = true;
            }
        }

        [DllImport(DLLPath)]
        public extern static int RemoteCli_disconnect();

        private void disconnect_Click(object sender, EventArgs e)
        {
            disconnect.Enabled = false;
            liveview.Image = null;
            int ret = RemoteCli_disconnect();
            connect.Enabled = true;
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int controlPTZF([MarshalAs(UnmanagedType.LPStr)] string type);

        private void panTilt_Click(object sender, EventArgs e)
        {
            int ret = controlPTZF(txtType.Text);
        }

        [DllImport(DLLPath)]
        public extern static int presetPTZFSet(Int32 index);

        private void setPreset_Click(object sender, EventArgs e)
        {
            int index = Int32.Parse(txtPreset.Text);
            int ret = presetPTZFSet(index);
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int setDeviceProperty([MarshalAs(UnmanagedType.LPStr)] string code, Int64 data, bool blocking);

        private void setDP_Click(object sender, EventArgs e)
        {
            int data = Int32.Parse(txtData.Text);
            int ret = setDeviceProperty(txtCode.Text, data, false/*blocking*/);
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int sendCommand([MarshalAs(UnmanagedType.LPStr)] string type);

        private void command_Click(object sender, EventArgs e)
        {
            int ret = sendCommand(txtCommand.Text);
        }

        private void updateLiveview_Click(object sender, EventArgs e)
        {
            updateLiveView();
        }

        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public extern static int getLiveview(out IntPtr instance, out UInt32 lv_size);


        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public extern static void deleteUint8Array(IntPtr instance);


        private int _running = 0;   // 0:アイドル, 1:実行中
        private void updateLiveView()
        {
            if (Interlocked.Exchange(ref _running, 1) == 1)
            {
                return;
            }
            try
            {
                int ret = getLiveview(out IntPtr imagePtr, out uint size);
                if (ret != 0 || imagePtr == IntPtr.Zero || size <= 0) return;

                try
                {
                    byte[] managedBuffer = new byte[size];
                    Marshal.Copy(imagePtr, managedBuffer, 0, (int)size);
                    deleteUint8Array(imagePtr);

                    using (MemoryStream ms = new MemoryStream(managedBuffer))
                    {
                        liveview.Image?.Dispose();
                        liveview.Image = Image.FromStream(ms);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"error: {ex.Message}");
                }
            }
            finally
            {
                Volatile.Write(ref _running, 0);
            }
        }

        static int d = 0;
        static void OnLiveviewCb(int eventId)
        {
            if (_instance != null)
            {
                // UI スレッドで updateLiveView を呼ぶ
                _instance.Invoke((MethodInvoker)(() => _instance.updateLiveView()));
            }
        }
    }
}
