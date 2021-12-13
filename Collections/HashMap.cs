using System;
using System.Runtime.CompilerServices;
namespace Klondike.Collections {
    public sealed class HashMap<T> where T : IEquatable<T> {
        private struct HashValue<Q> where Q : IEquatable<Q> {
            public uint Hash;
            public Q Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(Q value, uint hash) {
                Value = value;
                Hash = hash;
            }
        }

        private readonly HashValue<T>[] table;
        private uint count, capacity;

        public HashMap(int maxCount) {
            capacity = (uint)maxCount;
            count = 0;
            table = new HashValue<T>[capacity];
        }

        public int Count => (int)count;
        public void Clear() {
            count = 0;
            Array.Clear(table, 0, (int)capacity);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int Add(T key) {
            uint hash = (uint)key.GetHashCode();
            uint i = hash % capacity;
            HashValue<T> node = table[i];

            while (node.Hash != 0) {
                if (node.Hash == hash && key.Equals(node.Value)) { return (int)i; }
                if (++i >= capacity) {
                    i = 0;
                }
                node = table[i];
            }

            ++count;
            table[i].Set(key, hash);

            return -1;
        }
        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ref table[index].Value; }
        }
    }
}