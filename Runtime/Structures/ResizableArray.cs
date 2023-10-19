using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBF.Structures
{
    public class ResizableArray<T>
    {
        T[] m_array;
        int m_count;

        public ResizableArray(int initialCapacity = 12)
        {
            m_array = new T[initialCapacity];
            m_count = 0;
        }

        public T[] InternalArray { get { return m_array; } }

        public int Count { get { return m_count; } }

        public void Add(T element)
        {
            if (m_count == m_array.Length)
                Array.Resize(ref m_array, m_array.Length * 3);

            m_array[m_count++] = element;
        }

        public void SetSize(int size)
        {
            if(size > m_array.Length)
                Array.Resize(ref m_array, size);
            m_count = size;
        }

        public void Clear()
        {
            m_count = 0;
        }
    }
}
