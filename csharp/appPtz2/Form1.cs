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
        class DPConv
        {
            public int index { get; set; }
            public string str { get; set; }
        }

        const string DLLPath = "RemoteCli.dll";

        [DllImport(DLLPath)]
        public extern static int RemoteCli_init();

        // 2025/09/29 Liveview cbとchanged cbを別にするとLiveview cbが来なくなる
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LiveviewCbDelegate(int eventId);

        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RegisterLiveviewCb(LiveviewCbDelegate liveviewCb);

        private static int SPEED_MAX = 127; // AM7=127, FR7=50. connect時に読み込み

        private static Form1 _instance;

        private static LiveviewCbDelegate liveviewCb = new LiveviewCbDelegate(OnLiveviewCb);

        private static System.Timers.Timer timer;

        private static DirectInput directInput = new DirectInput();
        private static Joystick joystick = null;
        private static int[] xyOffset;
        private static int[] xyLast = new int[4];
        private static bool[] buttonLast = new bool[12];
        private static int povLast = 0;
        private static int BlindZone = 0;
        private static int CameraIndex = 0;
        private static bool[] CameraConnected = { false, false, false };

        enum ButtonIndex
        {
            Default = -1,
            Iris = 0,
            FPS = 1,
            Gain = 2,
            Shutter = 3,
            WB = 4,
        };
        private static ButtonIndex page = ButtonIndex.Default;

        private static string[] buttonLabels = new string[] {
            "","","Assign1","Assign2","Assign3",
            "Iris\n","FPS\n","Gain\n0db","Shutter\n1/250","WB\n5600K",
            "","","Preset1","Preset2","Preset3",
        };

        private static IMacroBoard streamDeck;

        public Form1()
        {
            InitializeComponent();
            _instance = this;

            RemoteCli_init();
            RegisterLiveviewCb(liveviewCb);

            var joystickGuid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                break;
            }

            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("no gamepad");
            }
            else
            {
                joystick = new Joystick(directInput, joystickGuid);
                joystick.Acquire();
            }

            try
            {
                streamDeck = StreamDeck.OpenDevice();
                streamDeck.SetBrightness(80);

                streamDeck.KeyStateChanged += keyChanged;
                updateButton();
            }
            catch
            {
                Console.WriteLine("no streamdeck");
            }

            // gamepadポーリングtimer
            timer = new System.Timers.Timer(50);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            // debug
            /*
            joystick.Poll();
            var state = joystick.GetCurrentState();
            xyOffset = new int[4] { state.X, state.Y, state.Z, state.RotationZ };
            BlindZone = int.Parse(textBlindZone.Text);
            timer.Enabled = true;
            */                               
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static int RemoteCli_connect(int index, [MarshalAs(UnmanagedType.LPStr)] string inputLine);

        [DllImport(DLLPath)]
        public extern static int RemoteCli_disconnect(int index);

       private void connectX_Click(int index)
        {
            System.Windows.Forms.Button connect = null;
            System.Windows.Forms.TextBox txtConnect = null;

            switch (index)
            {
                case 0:
                    connect = connect0;
                    txtConnect = txtConnect0;
                    break;
                case 1:
                    connect = connect1;
                    txtConnect = txtConnect1;
                    break;
                default:
                    return;
            }

            if (CameraConnected[index] == false)
            {
                SPEED_MAX = int.Parse(txtSpeedMax.Text);
                BlindZone = int.Parse(textBlindZone.Text);

                // OFF->ON
                connect.Enabled = false;
                int ret = RemoteCli_connect(index, txtConnect.Text);
                connect.Enabled = true;
                if (ret == 0)
                {
                    connect.BackColor = Color.Orange;
                    connect.Text = "disconnect";
                    CameraConnected[index] = true;

                    if (joystick != null)
                    {
                        joystick.Poll();
                        var state = joystick.GetCurrentState();
                        xyOffset = new int[4] { state.X, state.Y, state.Z, state.RotationZ };
                        timer.Enabled = true;
                    }
                }
            }
            else
            {
                // ON->OFF
                connect.BackColor = SystemColors.Control;
                connect.Text = "connect";
                CameraConnected[index] = false;

                timer.Enabled = false;
                liveview.Image = null;
                connect.Enabled = false;
                int ret = RemoteCli_disconnect(index);
                connect.Enabled = true;
            }
        }

        private void connect0_Click(object sender, EventArgs e)
        {
            connectX_Click(0);
        }

        private void connect1_Click(object sender, EventArgs e)
        {
            connectX_Click(1);
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
        public extern static Int64 getDeviceProperty([MarshalAs(UnmanagedType.LPStr)] string code);

        private void getDP_Click(object sender, EventArgs e)
        {
            Int64 ret = getDeviceProperty(txtCode.Text);
            txtData.Text = ret.ToString();
        }

        [DllImport(DLLPath, CharSet = CharSet.Ansi)]
        public extern static Int64 incDeviceProperty([MarshalAs(UnmanagedType.LPStr)] string code, int incDev, bool blocking);

        private void incDP_Click(object sender, EventArgs e)
        {
            Int64 ret = incDeviceProperty(txtCode.Text, 1, true/*blocking*/);
            txtData.Text = ret.ToString();
        }

        private void decDP_Click(object sender, EventArgs e)
        {
            Int64 ret = incDeviceProperty(txtCode.Text, 0, true/*blocking*/);
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
            updateLiveView(0);
        }

        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public extern static int getLiveview(out IntPtr instance, out UInt32 lv_size);


        [DllImport(DLLPath, CallingConvention = CallingConvention.Cdecl)]
        public extern static void deleteUint8Array(IntPtr instance);


        private int _running = 0;   // 0:アイドル, 1:実行中
        private void updateLiveView(int eventId)    // 0:liveview, 1:changedDP
        {
            if (Interlocked.Exchange(ref _running, 1) == 1)
            {
                return;
            }
            try
            {
                if(eventId == 0)
                {
                    if (checkLiveview.Checked)
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
                }
                else if (eventId == 1)
                {
                    Console.WriteLine($"onChanged");
                    if (streamDeck != null)
                    {
                        updateButton();
                    }
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
                _instance.Invoke((MethodInvoker)(() => _instance.updateLiveView(eventId)));
            }
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            joystick.Poll();
            var state = joystick.GetCurrentState();

            // stick
            int[] xy = new int[4] { state.X, state.Y, state.Z, state.RotationZ };
            for (int i = 0; i < 4; i++) {
                xy[i] -= xyOffset[i];
                if (Math.Abs(xy[i]) < BlindZone) xy[i] = 0;
            }

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
                        /*
                        case 0: controlPTZF("4");                               break;
                        case 1: sendCommand("RemoteKeyMenuButton 1 0");         break;
                        case 2: sendCommand("RemoteKeyCancelBackButton 1 0");   break;
                        case 3: sendCommand("RemoteKeySet 1 0");                break;
                        case 11: sendCommand("RemoteKeyDisplayButton 1 0");     break;
                        */
                        case 0: controlPTZF("4"); break;
                        case 3: sendCommand("RemoteKeyMenuButton 1 0"); break;
                        case 2: sendCommand("RemoteKeyCancelBackButton 1 0"); break;
                        case 1: sendCommand("RemoteKeySet 1 0"); break;
                        case 9: sendCommand("RemoteKeyDisplayButton 1 0"); break;
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

        public string format_f_number(Int64 f_number)
        {
            const int CrFnumber_IrisClose = 0xFFFD; // Iris Close
            const int CrFnumber_Unknown = 0xFFFE; // Display "--"
	        const int CrFnumber_Nothing = 0xFFFF; // Nothing to display

            string msg = $"unknown";
            if ((0x0000 == f_number) || (CrFnumber_Unknown == f_number))
            {
                return $"--";
            }
            else if (CrFnumber_Nothing == f_number)
            {
                return $"";
            }
            else if ((f_number % 100) > 0)
            {
                return $"F{(f_number / 100.0):F1}";
            }
            else
            {
                return $"F{(f_number / 100)}";
            }
        }

        public string format_shutter_speed(Int64 shutter_speed)
        {
            Int32 numerator = (Int32)((shutter_speed >> 32) & 0xFFFFFFFF);
            Int32 denominator = (Int32)(shutter_speed & 0xFFFFFFFF);

            if (0 == shutter_speed)
            {
                return $"Bulb";
            }
            else if (0 == denominator)
            {
                return $"error";
            }
            else if (1 == numerator)
            {
                return $"{numerator}/{denominator}";
            }
            else if (0 == (numerator % denominator))
            {
                return $"{numerator/denominator}\"";
            }
            else
            {
                Int32 numdivision = numerator / denominator;
                Int32 numremainder = numerator % denominator;
                return $"{numdivision}.{numremainder}";
            }
        }
/*
        public string format_whiteBalance(int data)
        {
            string msg = "unknown";
            DPConv[] convTable = new DPConv[]
            {
            new DPConv { index = 0x0000, str = "AWB" },
            new DPConv { index = 0x0001, str = "Underwater_Auto" },
            new DPConv { index = 0x0011, str = "Daylight" },
            new DPConv { index = 0x0012, str = "Shadow" },
            new DPConv { index = 0x0013, str = "Cloudy" },
            new DPConv { index = 0x0014, str = "Tungsten" },
            new DPConv { index = 0x0020, str = "Fluorescent" },
            new DPConv { index = 0x0021, str = "Fluorescent_WarmWhite" },
            new DPConv { index = 0x0022, str = "Fluorescent_CoolWhite" },
            new DPConv { index = 0x0023, str = "Fluorescent_DayWhite" },
            new DPConv { index = 0x0024, str = "Fluorescent_Daylight" },
            new DPConv { index = 0x0030, str = "Flush" },
            new DPConv { index = 0x0100, str = "ColorTemp" },
            new DPConv { index = 0x0101, str = "Custom_1" },
            new DPConv { index = 0x0102, str = "Custom_2" },
            new DPConv { index = 0x0103, str = "Custom_3" },
            new DPConv { index = 0x0104, str = "Custom" },
            };

            int i = 0;
            for (i = 0; i < convTable.Length; i++)
            {
                if (convTable[i].index == data)
                {
                    msg = convTable[i].str;
                    break;
                }
            }
            return msg;
        }
*/
        private void updateButton()
        {
            if (CameraConnected[CameraIndex])
            {
                Int64 data;
                data = getDeviceProperty($"FNumber");
                buttonLabels[5 + (int)ButtonIndex.Iris] = $"Iris\n{format_f_number(data)}";

                data = getDeviceProperty($"SQFrameRate");
                buttonLabels[5 + (int)ButtonIndex.FPS] = $"FPS\n{data}";

                data = getDeviceProperty($"GaindBValue");
                buttonLabels[5 + (int)ButtonIndex.Gain] = $"Gain\n{data}db";

                data = getDeviceProperty($"ShutterSpeedValue");
                buttonLabels[5+(int)ButtonIndex.Shutter] = $"Shutter\n{format_shutter_speed(data)}";

                data = getDeviceProperty($"Colortemp");
                buttonLabels[5+(int)ButtonIndex.WB] = $"WB\n{data}K";
            }

            var _labels = new string[15];
            switch (page)
            {
                default:
                case ButtonIndex.Default:
                    for(int i = 0; i < 15; i++)
                    {
                        _labels[i] = buttonLabels[i];
                    }
                    break;
                case ButtonIndex.Iris:
                case ButtonIndex.FPS:
                case ButtonIndex.Gain:
                case ButtonIndex.Shutter:
                case ButtonIndex.WB:
                    _labels[0 + (int)page] = $"↑";
                    _labels[5 + (int)page] = buttonLabels[5 + (int)page];
                    _labels[10 + (int)page] = $"↓";
                    break;
            }

            for (int i = 0; i < streamDeck.Keys.Count && i < buttonLabels.Length; i++)
            {
                var bmp = RenderKeyBitmap(_labels[i]);
                var keyBmp = KeyBitmap.Create.FromBitmap(bmp);
                streamDeck.SetKeyBitmap(i, keyBmp);
            }
        }

        private Bitmap RenderKeyBitmap(string text)
        {
            const int size = 144;

            var bmp = new Bitmap(size, size);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(30, 30, 30));
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var font = new Font("Segoe UI", 20, FontStyle.Bold);
            var brush = new SolidBrush(Color.White);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, brush, new RectangleF(0, 0, size, size), sf);

            return bmp;
        }

        private void keyChanged(object sender, OpenMacroBoard.SDK.KeyEventArgs e)
        {
            Console.WriteLine($"Key {e.Key} {(e.IsDown ? $"DOWN" : $"UP")}");
            if (!e.IsDown) return;

            string[] DPTable = new string[5] { $"FNumber", $"SQFrameRate", $"GaindBValue", $"ShutterSpeedValue", $"Colortemp" };

            switch (page)
            {
                default:
                case ButtonIndex.Default:
                    switch(e.Key)
                    {
                        case 0 + 2:
                            setDeviceProperty("AssignableButton1", 2, true/*blocking*/);
                            Thread.Sleep(10);
                            setDeviceProperty("AssignableButton1", 1, true/*blocking*/);
                            break;
                        case 0 + 3:
                            setDeviceProperty("AssignableButton2", 2, true/*blocking*/);
                            Thread.Sleep(10);
                            setDeviceProperty("AssignableButton2", 1, true/*blocking*/);
                            break;
                        case 0 + 4:
                            setDeviceProperty("AssignableButton3", 2, true/*blocking*/);
                            Thread.Sleep(10);
                            setDeviceProperty("AssignableButton3", 1, true/*blocking*/);
                            break;
                        case 5 + (int)ButtonIndex.Iris:
                        case 5 + (int)ButtonIndex.FPS:
                        case 5 + (int)ButtonIndex.Gain:
                        case 5 + (int)ButtonIndex.Shutter:
                        case 5 + (int)ButtonIndex.WB:
                            page = (ButtonIndex)(e.Key - 5);
                            break;
                        case 10 + 2:
                            setDeviceProperty("PresetPTZFSlotNumber", 1, false/*blocking*/);
                            break;
                        case 10 + 3:
                            setDeviceProperty("PresetPTZFSlotNumber", 2, false/*blocking*/);
                            break;
                        case 10 + 4:
                            setDeviceProperty("PresetPTZFSlotNumber", 3, false/*blocking*/);
                            break;
                        default:
                            page = ButtonIndex.Default;
                            break;
                    }
                    break;
                case ButtonIndex.Iris:
                case ButtonIndex.FPS:
                case ButtonIndex.Gain:
                case ButtonIndex.Shutter:
                case ButtonIndex.WB:
                    if(e.Key == 0 + (int)page)
                    {
                        incDeviceProperty(DPTable[(int)page], 1, true);
                    }
                    else if (e.Key == 10 + (int)page)
                    {
                        incDeviceProperty(DPTable[(int)page], 0, true);
                    }
                    else
                    {
                        page = ButtonIndex.Default;
                    }
                    break;

            }
            updateButton();
        }
    }
}
