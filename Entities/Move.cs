using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Klondike.Entities {
    [StructLayout(LayoutKind.Sequential, Size = 2, Pack = 1)]
    public struct Move {
        public byte Value1;
        public byte Value2;

        public Move(byte from, byte to, byte count = 1, bool flip = false) {
            Value1 = (byte)(from | (to << 4));
            Value2 = (byte)(count | (flip ? 0x80 : 0x00));
        }
        public Move(char from, char to, int count = 1, bool flip = false) {
            Value1 = (byte)(((byte)from - (byte)'A') | (((byte)to - (byte)'A') << 4));
            Value2 = (byte)(count | (flip ? 0x80 : 0x00));
        }
        public bool IsNull {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Value1 == 0; }
        }
        public byte From {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(Value1 & 0x0f); }
        }
        public byte To {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(Value1 >> 4); }
        }
        public byte Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)(Value2 & 0x7f); }
        }
        public bool Flip {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (Value2 & 0x80) != 0; }
        }
        public string Display {
            get {
                return string.Concat((char)((byte)'A' + From), (char)((byte)'A' + To));
            }
        }
        public override string ToString() {
            return string.Concat(From == Board.WastePile && Count != 0 ? Math.Abs(Count).ToString() : string.Empty, Display);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Move left, Move right) {
            return left.Value1 == right.Value1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Move left, Move right) {
            return left.Value1 != right.Value1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) {
            return obj is Move other && other.Value1 == Value1 && other.Value2 == Value2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return Value1 | (Value2 << 8);
        }
    }
}