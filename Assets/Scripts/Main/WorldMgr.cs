using System;
using System.Collections.Generic;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class BaseMgr
    {
        protected EcsWorld world;

        public BaseMgr(EcsWorld world)
        {
            this.world = world;
        }

        public EcsWorld GetWorld()
        {
            return world;
        }
    }
    
    public class WorldMgr
    {
        Dictionary<System.Type, BaseMgr> dic_mgr;

        EcsWorld world;
        
        public WorldMgr(EcsWorld world)
        {
            this.world = world;
            this.dic_mgr = new Dictionary<System.Type, BaseMgr>();
        }

        public T GetMgr<T>() where T: BaseMgr
        {
            BaseMgr base_mgr;
            System.Type type = typeof(T);
            if( dic_mgr.TryGetValue(type, out base_mgr) )
            {
                return base_mgr as T;   
            }
            else
            {
                T t =  (T) Activator.CreateInstance (typeof (T), new object[] { world });
                dic_mgr.Add(type, t);
                return t;
            }
        }
    }
}