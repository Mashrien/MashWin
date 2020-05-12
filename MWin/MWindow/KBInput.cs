using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MashWin {

    public static class KBInput {

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
            };

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
            };

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT {
            public int uMsg;
            public short wParamL;
            public short wParamH;
            };

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT no;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
            };

        [DllImport("user32.dll")]
        private extern static void SendInput(int nInputs, ref INPUT pInputs, int cbsize);
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        private extern static int MapVirtualKey(int wCode, int wMapType);

        private const int INPUT_KEYBOARD = 1;
        private const int KEYEVENTF_KEYDOWN = 0x0;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;

        public static void SendString(string s) {
            Keys modSave = 0;
            foreach (char c in s) {
                Keys k;
                if (char.IsUpper(c))
                    modSave = Keys.ShiftKey;

                switch (c) {
                    case ' ':
                        k = Keys.Space;
                        break;

                    case '@':
                        k = Keys.D2;
                        modSave = Keys.ShiftKey;
                        break;

                    case '.':
                        k = Keys.OemPeriod;
                        break;

                    case ',':
                        k = Keys.Oemcomma;
                        break;

                    case '^':
                        modSave = Keys.ControlKey;
                        continue;

                    case '\'':
                        k = Keys.OemBackslash;
                        break;

                    case '(':
                        k = Keys.D9;
                        modSave = Keys.ShiftKey;
                        break;

                    case ')':
                        k = Keys.D0;
                        modSave = Keys.ShiftKey;
                        break;

                    case '/':
                        k = Keys.Divide;
                        break;

                    case ':':
                        k = Keys.OemSemicolon;
                        modSave = Keys.ShiftKey;
                        break;

                    case ';':
                        k = Keys.OemSemicolon;
                        break;

                    case '`':
                        k = Keys.Return;
                        break;

                    case '-':
                        k = Keys.OemMinus;
                        break;

                    case '>':
                        k = Keys.OemPeriod;
                        modSave = Keys.ShiftKey;
                        break;

                    default:
                        k = (Keys)char.ToUpper(c);
                        break;
                    }

                Send(k, false, (short)modSave);
                modSave = 0;
                }
            }

        private static void Send(Keys key, bool isEXTEND, short mod = 0) {
            INPUT shift = new INPUT();
            if (mod > 0) {
                shift = new INPUT();
                shift.type = INPUT_KEYBOARD;
                shift.ki.wVk = mod;
                shift.ki.wScan = (short)MapVirtualKey(shift.ki.wVk, 0);
                shift.ki.dwFlags = ((isEXTEND) ? (KEYEVENTF_EXTENDEDKEY) : 0x0) | KEYEVENTF_KEYDOWN;
                shift.ki.time = 0;
                shift.ki.dwExtraInfo = 0;
                SendInput(1, ref shift, Marshal.SizeOf(shift));
                System.Threading.Thread.Sleep(10);
                }

            INPUT inp = new INPUT();

            // Keydown
            inp.type = INPUT_KEYBOARD;
            inp.ki.wVk = (short)key;
            inp.ki.wScan = (short)MapVirtualKey(inp.ki.wVk, 0);
            inp.ki.dwFlags = ((isEXTEND) ? (KEYEVENTF_EXTENDEDKEY) : 0x0) | KEYEVENTF_KEYDOWN;
            inp.ki.time = 0;
            inp.ki.dwExtraInfo = 0;
            SendInput(1, ref inp, Marshal.SizeOf(inp));

            // wait 10ms for the key to 'stay down'
            System.Threading.Thread.Sleep(10);

            // Keyup
            inp.ki.dwFlags = ((isEXTEND) ? (KEYEVENTF_EXTENDEDKEY) : 0x0) | KEYEVENTF_KEYUP;
            SendInput(1, ref inp, Marshal.SizeOf(inp));

            if (mod > 0) {
                System.Threading.Thread.Sleep(10);
                shift.ki.dwFlags = 0x0 | KEYEVENTF_KEYUP;
                SendInput(1, ref shift, Marshal.SizeOf(shift));
                System.Threading.Thread.Sleep(10);
                }
            }
        }
    }