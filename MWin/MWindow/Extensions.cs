using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MashWin {
    public static class Extensions {
        public static Color Lerp(this Color colour, Color to, float amount) {
            // start colours as lerp-able floats
            float sr = colour.R, sg = colour.G, sb = colour.B;

            // end colours as lerp-able floats
            float er = to.R, eg = to.G, eb = to.B;

            // lerp the colours to get the difference
            byte r = (byte)sr.Lerp(er, amount),
                 g = (byte)sg.Lerp(eg, amount),
                 b = (byte)sb.Lerp(eb, amount);

            // return the new colour
            return Color.FromArgb(r, g, b);
            }

        public static float Lerp(this float start, float end, float amount) {
            float difference = end - start;
            float adjusted = difference * amount;
            return start + adjusted;
            }

        public static string VersionString() {
            return $"{Assembly.GetEntryAssembly().GetName().Version.Major}.{Assembly.GetEntryAssembly().GetName().Version.Minor}.{Assembly.GetEntryAssembly().GetName().Version.Build}-{Assembly.GetEntryAssembly().GetName().Version.Revision}";
            }

        }
    }
