using System;
using System.Collections.Generic;

namespace Pool
{
    public class sPool<T>
    {
        T[] arr;
        int _count;
        int addcount;

        public int Count {
            get{ return _count; }
        } 

        public sPool(int count, int addcount)
        {
            CreateArray( count, addcount );
        }

        public sPool()
        {
            CreateArray( 0, 8 );
        }

        void CreateArray( int init_count, int addcount)
        {
            this.addcount = addcount;
            this.arr = new T[init_count];
            Clear();
        }

        public T this[int index]
        {
            get { return Get( index ); }
            set { Set( index, value ); }
        }
        
        public void Add(T t)
        {
            if(_count >= arr.Length)
            {
                int oldlength = arr.Length;
                int newlength = oldlength + addcount;
                Array.Resize(ref arr, newlength);
            }
            arr[_count] = t;
            _count += 1;
        }

        public void Remove(int index)
        {
            if(index < 0) return;
            if(index >= _count) return;

            Array.Copy(arr, index + 1, arr, index, arr.Length - index - 1);
            arr[arr.Length - 1] = default;
            _count -= 1;
        }

        public T Get(int index)
        {
            if( index >= 0 && index < _count )
            {
                return arr[index];
            }
            return default;
        }
        
        public void Set(int index, T val)
        {
            if( index >= 0 && index < _count )
            {
                arr[index] = val;
            }
        }

        public ref T GetByRef(int index)
        {
            return ref arr[index];
        }

        public void Clear()
        {
            for(int i=_count - 1; i>=0; i--)
            {
                arr[i] = default;
            }
            _count = 0;
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(arr, 0, _count, comparer);
        }
    }
}