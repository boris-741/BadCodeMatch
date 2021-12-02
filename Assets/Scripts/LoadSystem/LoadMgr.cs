using UnityEngine;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class LoadMgr: BaseMgr
    {
        EcsPool<LoadComponent> load_pool;
        EcsPool<WaitComponent> wait_pool;

        EcsFilter load_wait_filter;
        EcsFilter load_act_filter;
        EcsFilter load_ready_filter;

        public LoadMgr(EcsWorld world) : base(world)
        {
            this.world = world;

            wait_pool = world.GetPool<WaitComponent>(); 
            load_pool = world.GetPool<LoadComponent>(); 

            load_wait_filter = world.Filter<LoadComponent>().Inc<WaitComponent>().End();
            load_act_filter = world.Filter<LoadComponent>().Inc<ActComponent>().End();
            load_ready_filter = world.Filter<LoadComponent>().Inc<ReadyComponent>().End();
        }

        public bool IsLoadExist(string path)
        {
            foreach (int entity in load_ready_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                if(load.path == path)
                {
                    return true;
                }
            }

            foreach (int entity in load_act_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                if(load.path == path)
                {
                    return true;
                }
            }

            foreach (int entity in load_wait_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                if(load.path == path)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsLoadInProgress(string path)
        {
            foreach (int entity in load_act_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                if(load.path == path)
                {
                    return true;
                }
            }

            foreach (int entity in load_wait_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                if(load.path == path)
                {
                    return true;
                }
            }

            return false;
        }

        public bool GetObject(string path, out Object obj)
        {
            obj = null;
            foreach (int entity in load_ready_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                if(load.path == path)
                {
                    obj = load.obj;
                    return true;
                }
            }

            return false;
        }

        public void Load(string path)
        {
            if( !IsLoadExist(path) )
            {
                int load_entity = world.NewEntity();
                wait_pool.Add(load_entity);
                ref LoadComponent load_cmp = ref load_pool.Add(load_entity);
                load_cmp.path = path;
            }
        }
    }
}