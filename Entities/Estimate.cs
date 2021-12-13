using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Klondike.Entities {
    [StructLayout(LayoutKind.Sequential, Size = 2, Pack = 1)]
    public struct Estimate {
        public byte Current;
        public byte Remaining;

        public byte Total {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                int total = Current + Remaining;
                if (total > 255) { total = 255; }
                return (byte)total;
            }
        }
        public override string ToString() {
            return $"{Current,3} + {Remaining,3} = {Total,3}";
        }
    }
}