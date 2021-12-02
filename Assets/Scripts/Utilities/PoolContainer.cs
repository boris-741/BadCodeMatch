using System;

namespace Pool
{
    public class PoolContainer<T> where T: class
    {
        protected struct Info
        {
            public T item;
            public bool is_used;
        }
        protected sPool<Info> info_pool;
        protected sPool<T> used_pool;

        public PoolContainer()
        {
            info_pool = new sPool<Info>();
            used_pool = new sPool<T>();
            CreateNewItem();
        }

        public virtual T Get()
        {
            T item;
            if( !GetUnusedItem( out item ) )
            {
                item = CreateNewItem();
            }
            return item;
        }

        public virtual sPool<T> GetUsed()
        {
            used_pool.Clear();
            for(int i=0; i<info_pool.Count; i++)
            {
                ref Info info = ref info_pool.GetByRef( i );
                if( info.is_used )
                {
                    used_pool.Add( info.item );
                }
            }
            return used_pool;
        }

        public virtual void Recycle( T item )
        {
            for(int i=0; i<info_pool.Count; i++)
            {
                ref Info info = ref info_pool.GetByRef( i );
                if(info.item == item )
                {
                    info.is_used = false;
                }
            }
        }

        public virtual void Recycle()
        {
            for(int i=0; i<info_pool.Count; i++)
            {
                ref Info info = ref info_pool.GetByRef( i );
                info.is_used = false;
            }
        }

        bool GetUnusedItem( out T item )
        {
            item = null;
            for(int i=0; i<info_pool.Count; i++)
            {
                ref Info info = ref info_pool.GetByRef( i );
                if( !info.is_used )
                {
                    item = info.item;
                    return true;
                }
            }
            return false;
        }

        T CreateNewItem()
        {
            T item = (T)Activator.CreateInstance (typeof (T), true);
            Info info = new Info{ item = item, is_used = false };
            info_pool.Add( info );
            return item;
        }
    }

    public class sPoolContainer<T> : PoolContainer<sPool<T>>
    {
        public override void Recycle( sPool<T> item )
        {
            item.Clear();
            base.Recycle( item );
        }

        public override void Recycle()
        {
            for(int i=0; i<info_pool.Count; i++)
            {
                ref Info info = ref info_pool.GetByRef( i );
                info.item.Clear();
                info.is_used = false;
            }
        }
    }
}