using UnityEngine;
using UnityEngine.U2D;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class PoolSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        //pool
        EcsPool<InitPoolComponent> init_pool;
        EcsPool<WaitComponent> wait_pool;
        EcsPool<ReadyComponent> ready_pool;
        EcsPool<ActComponent> act_pool;
        EcsPool<AtlasComponent> atlas_pool;
        //ecs filter
        EcsFilter init_filter;
        EcsFilter act_filter;
        EcsFilter atlas_filter;
        //mgr
        LoadMgr load_mgr;
        //const
        const string atlas_path = "Atlas/MainAtlas";

        public void Init(EcsSystems systems) 
        {
            world = systems.GetWorld();
            //ecs pool
            wait_pool = world.GetPool<WaitComponent>(); 
            act_pool = world.GetPool<ActComponent>(); 
            ready_pool = world.GetPool<ReadyComponent>(); 
            init_pool = world.GetPool<InitPoolComponent>();
            atlas_pool = world.GetPool<AtlasComponent>();
            //ecs filter
            init_filter = world.Filter<InitPoolComponent>().Inc<WaitComponent>().End();
            act_filter = world.Filter<InitPoolComponent>().Inc<ActComponent>().End();  
            atlas_filter = world.Filter<AtlasComponent>().Inc<ActComponent>().End();  
            //mgr
            load_mgr = systems.GetShared<WorldMgr>().GetMgr<LoadMgr>();         
        }

        public void Run(EcsSystems systems) 
        {
            RunInit();
            RunAct();
        }

        void RunInit()
        {
            foreach(int entity in init_filter)
            {
                Debug.LogFormat("pool atlas: start load");
                //load tlas
                int atlas_entity = world.NewEntity();
                atlas_pool.Add(atlas_entity);
                act_pool.Add(atlas_entity);
                load_mgr.Load(atlas_path);
                //move to act
                wait_pool.Del(entity);
                act_pool.Add(entity);
            }
        }

        void RunAct()
        {
            foreach(int entity in act_filter)
            {
                Object atlas_obj;
                if( load_mgr.GetObject(atlas_path, out atlas_obj) )
                {
                    foreach(int atlas_entity in atlas_filter)
                    {
                        ref AtlasComponent atlas_cmp = ref atlas_pool.Get(atlas_entity);
                        atlas_cmp.atlas = (SpriteAtlas)atlas_obj;
                        act_pool.Del(atlas_entity);
                        ready_pool.Add(atlas_entity);
                        Debug.LogFormat("pool atlas: complete load");
                    }
                    ready_pool.Add(entity);
                }
            }
        }
    }
}