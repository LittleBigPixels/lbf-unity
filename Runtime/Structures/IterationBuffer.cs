using System;
using System.Collections;
using System.Collections.Generic;

namespace LBF.Structures
{
    public class IterationBuffer<T> : IEnumerable<T> where T : new()
    {
        const int ChunkSize = 128;

        public T[] Items;

        int m_count;

        public int Count
        {
            get { return m_count; }
        }

        public IterationBuffer()
        {
            m_count = 0;
            InitialiseBuffer(1);
        }

        public IterationBuffer(int capacity)
        {
            m_count = 0;
            int nChunk = capacity / ChunkSize + (capacity % ChunkSize == 0 ? 0 : 1);
            InitialiseBuffer(nChunk);
        }

        public IterationBuffer(T[] source)
        {
            int nChunk = source.Length / ChunkSize + (source.Length % ChunkSize == 0 ? 0 : 1);
            InitialiseBuffer(nChunk);

            Array.Copy(source, Items, source.Length);
            m_count = source.Length;
        }

        public void Clear()
        {
            m_count = 0;
        }

        public int Append()
        {
            if (m_count == Items.Length)
                ExpandBuffer();
            return m_count++;
        }

        public int Append(int count)
        {
            if (m_count + count >= Items.Length)
                ExpandBuffer();
            m_count+= count;
            return m_count;
        }

        public void SwapRemove(int index)
        {
            Items[index] = Items[m_count - 1];
            m_count--;
        }

        void InitialiseBuffer(int nChunk)
        {
            Items = new T[nChunk * ChunkSize];

            if (typeof(T).IsValueType == false)
            {
                for (int i = 0; i < Items.Length; i++)
                    Items[i] = new T();
            }
        }

        void ExpandBuffer()
        {
            int currentLength = Items.Length;
            int newLength = currentLength + ChunkSize;
            Array.Resize(ref Items, newLength);

            if (typeof(T).IsValueType == false)
            {
                for (int i = currentLength; i < newLength; i++)
                    Items[i] = new T();
            }
        }
    
        void ExpandBufferTo(int capacity)
        {
            int currentLength = Items.Length;
            int newLength = capacity;
            if (newLength < ChunkSize + currentLength)
                newLength = currentLength + ChunkSize;
        
            Array.Resize(ref Items, newLength);

            if (typeof(T).IsValueType == false)
            {
                for (int i = currentLength; i < newLength; i++)
                    Items[i] = new T();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new IterationBufferIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new IterationBufferIterator(this);
        }

        struct IterationBufferIterator : IEnumerator, IEnumerator<T>
        {
            IterationBuffer<T> m_buffer;
            int m_currentIndex;

            public IterationBufferIterator(IterationBuffer<T> buffer)
            {
                m_buffer = buffer;
                m_currentIndex = -1;
            }

            public object Current => m_buffer.Items[m_currentIndex];
            T IEnumerator<T>.Current => m_buffer.Items[m_currentIndex];

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (m_currentIndex == m_buffer.Count - 1)
                    return false;

                m_currentIndex++;
                return true;
            }

            public void Reset()
            {
                m_currentIndex = 0;
            }
        }
    }
}