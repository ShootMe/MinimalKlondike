using System;
using System.Runtime.CompilerServices;
namespace Klondike.Collections {
    public sealed class Heap<T> where T : IComparable<T> {
        private int size;
        private T[] nodes;

        public Heap(int maxNodes) {
            size = 0;
            nodes = new T[maxNodes];
        }
        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return size; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            size = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T node) {
            int current = size++;
            while (current > 0) {
                int child = current;
                current = (current - 1) >> 1;
                T value = nodes[current];
                if (node.CompareTo(value) >= 0) {
                    current = child;
                    break;
                }

                nodes[child] = value;
            }

            nodes[current] = node;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public T Dequeue() {
            T result = nodes[0];
            int current = 0;
            T end = nodes[--size];

            do {
                int last = current;
                int child = (current << 1) + 1;

                if (size > child && end.CompareTo(nodes[child]) > 0) {
                    current = child;

                    if (size > ++child && nodes[current].CompareTo(nodes[child]) > 0) {
                        current = child;
                    }
                } else if (size > ++child && end.CompareTo(nodes[child]) > 0) {
                    current = child;
                } else {
                    break;
                }

                nodes[last] = nodes[current];
            } while (true);

            nodes[current] = end;

            return result;
        }
        private void Resize() {
            T[] newNodes = new T[(int)(nodes.Length * 1.5)];
            for (int i = 0; i < nodes.Length; i++) {
                newNodes[i] = nodes[i];
            }
            nodes = newNodes;
        }

        public override string ToString() {
            return $"Count = {Count}";
        }
    }
}