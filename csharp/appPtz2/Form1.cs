using SharpDX.DirectInput;  
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using OpenMacroBoard.SDK;
using StreamDeckSharp;


namespace appPtz2
{
    public partial class Form1 : Form
    {
        const string DLLPath = "RemoteCli.dll";


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LiveviewCbDelegate(int eventId);

        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RegisterLiveviewCb(LiveviewCbDelegate liveviewCb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ChangedCbDelegate(int eventId);

        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RegisterChangedCb(ChangedCbDelegate changedCb);

        const int SPEED_MAX = 50; // 127;

        private static Form1 _instance;

        private static LiveviewCbDelegate liveviewCb = new LiveviewCbDelegate(OnLiveviewCb);

        private static ChangedCbDelegate changedCb = new ChangedCbDelegate(OnChangedCb);

        private static System.Timers.Timer timer;

        private static DirectInput directInput = new DirectInput();
        private static Joystick joystick = null;
        private static int[] xyOffset;
        private static int[] xyLast = new int[4];
        private static bool[] buttonLast = new bool[12];
        private static int povLast = 0;

        public Form1()
        {
            InitializeComponent();
            _instance = this;

            RegisterLiveviewCb(liveviewCb);

            RegisterChangedCb(changedCb);

            var joystickGuid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                break;
            }

            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("ゲームパッドが見つかりません");
            }
            else
            {
                joystick = new Joystick(directInput, joystickGuid);
                joystick.Acquire();
            }

            var buttonLabels = new[] {
                "","","Assign1","Assign2","Assign3",
                "","FPS\n23.97","Gain\n0db","Shutter\n1/250","WB\n5600K",
                "","","Preset1","Preset2","Preset3",
            };

            var deck = StreamDeck.OpenDevice();
            deck.SetBrightness(80);

            for (int i = 0; i < deck.Keys.Count && i < buttonLabels.Length; i++)
            {
                var bmp = RenderKeyBitmap(buttonLabels[i]);
                var keyBmp = KeyBitmap.Create.FromBitmap(bmp);
                deck.SetKeyBitmap(i, keyBmp);
            }

            deck.KeyStateChanged += (sender, e) =>
            {
                Console.WriteLine($"Key {e.Key} {(e.IsDown ? "DOWN" : "UP")}");

                if (e.IsDown)
                {
                    // deck.SetKeyBitmap(e.Key, KeyBitmap.Create.FromRgb(255, 255, 255)); など
                }
            };

            // gamepadポーリングtimer
            timer = new System.Timers.Timer(50);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int RemoteCli_connect([MarshalAs(UnmanagedType.LPStr)] string inputLine);

        private void connect_Click(object sender, EventArgs e)
        {
            disconnect.Enabled = false;
            connect.Enabled = false;
            int data = RemoteCli_connect(txtConnect.Text);
            if (data == 0) {
                disconnect.Enabled = true;

                if (joystick != null) {
                    joystick.Poll();
                    var state = joystick.GetCurrentState();
                    xyOffset = new int[4] { state.X, state.Y, state.Z, state.RotationZ };
                    timer.Enabled = true;
                }
            }
            else
            {
                connect.Enabled = true;
            }
        }

        [DllImport(DLLPath)]
        public extern static int RemoteCli_disconnect();

        private void disconnect_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
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
        public extern static int getDeviceProperty([MarshalAs(UnmanagedType.LPStr)] string code);

        private void getDP_Click(object sender, EventArgs e)
        {
            int ret = getDeviceProperty(txtCode.Text);
            txtData.Text = ret.ToString();
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int incDeviceProperty([MarshalAs(UnmanagedType.LPStr)] string code, int incDev, bool blocking);

        private void incDP_Click(object sender, EventArgs e)
        {
            int ret = incDeviceProperty(txtCode.Text, 1, true/*blocking*/);
            txtData.Text = ret.ToString();
        }

        private void decDP_Click(object sender, EventArgs e)
        {
            int ret = incDeviceProperty(txtCode.Text, 0, true/*blocking*/);
            txtData.Text = ret.ToString();
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

        static void OnLiveviewCb(int eventId)
        {
            if (_instance != null)
            {
                // UI スレッドで updateLiveView を呼ぶ
                _instance.Invoke((MethodInvoker)(() => _instance.updateLiveView()));
            }
        }

        private void updateDP()
        {
            if (Interlocked.Exchange(ref _running, 1) == 1)
            {
                return;
                Console.WriteLine($"onChanged");
            }
        }

        static void OnChangedCb(int eventId)
        {
            if (_instance != null)
            {
                // UI スレッドで updateDP を呼ぶ
                _instance.Invoke((MethodInvoker)(() => _instance.updateDP()));
            }
            Console.WriteLine($"onChanged");
        }


        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            int ret;
            joystick.Poll();
            var state = joystick.GetCurrentState();

            // stick
            int[] xy = new int[4] { state.X, state.Y, state.Z, state.RotationZ };
            for (int i = 0; i < 4; i++) { xy[i] -= xyOffset[i];}

            if (Math.Abs(xyLast[2] - xy[2]) > 5000 || Math.Abs(xyLast[3] - xy[3]) > 5000) {
                int pan = -(int)(xy[2] * SPEED_MAX / 32768.0);
                pan = Math.Min(SPEED_MAX, Math.Max(-SPEED_MAX, pan));

                int tilt = -(int)(xy[3] * SPEED_MAX / 32768.0);
                tilt = Math.Min(SPEED_MAX, Math.Max(-SPEED_MAX, tilt));

                string str2 = $"3 0 0 {pan} {tilt}";  // direction
                Console.WriteLine(str2);
                controlPTZF(str2);
            } else if(Math.Abs(xyLast[1] - xy[1]) > 5000)
            {
                int zoom = -xy[1];
                zoom = Math.Min(32767, Math.Max(-32767, zoom));

                setDeviceProperty("ZoomOperationWithInt16", zoom, false/*blocking*/);
            }
            for (int i = 0; i < 4; i++) { xyLast[i] = xy[i]; }

            // button
            string str = "";
            for (int i = 0; i < 12; i++)
            {
                str += (state.Buttons[i] ? "X" : "_");
                if (buttonLast[i] == false && state.Buttons[i] == true)
                {
                    switch(i)
                    {
                        case 0: controlPTZF("4");                               break;
                        case 1: sendCommand("RemoteKeyMenuButton 1 0");         break;
                        case 2: sendCommand("RemoteKeyCancelBackButton 1 0");   break;
                        case 3: sendCommand("RemoteKeySet 1 0");                break;
                        case 11: sendCommand("RemoteKeyDisplayButton 1 0");     break;
                    }
                    break;
                }


            }
            for (int i = 0; i < 12; i++) { buttonLast[i] = state.Buttons[i]; }

            // POV
            int pov = state.PointOfViewControllers[0];
            if (pov >= 0) pov = pov / 4500;

            if(povLast != pov)
            {
                switch (pov)
                {
                    case 0: sendCommand("RemoteKeyUp 1 0"); break;
                    case 1: sendCommand("RemoteKeyRightUp 1 0"); break;
                    case 2: sendCommand("RemoteKeyRight 1 0"); break;
                    case 3: sendCommand("RemoteKeyRightDown 1 0"); break;
                    case 4: sendCommand("RemoteKeyDown 1 0"); break;
                    case 5: sendCommand("RemoteKeyLeftDown 1 0"); break;
                    case 6: sendCommand("RemoteKeyLeft 1 0"); break;
                    case 7: sendCommand("RemoteKeyLeftUp 1 0"); break;
                }
            }
            povLast = pov;

            Console.WriteLine($"{xy[0]},{xy[1]},{xy[2]},{xy[3]},{str},{pov}");
        }

        public Bitmap RenderKeyBitmap(string text)
        {
            const int size = 144;

            var bmp = new Bitmap(size, size);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(30, 30, 30));
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var font = new Font("Segoe UI", 24, FontStyle.Bold);
            var brush = new SolidBrush(Color.White);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, brush, new RectangleF(0, 0, size, size), sf);

            return bmp;
        }
    }
}
