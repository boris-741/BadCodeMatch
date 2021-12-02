using UnityEngine;
using Leopotam.EcsLite;


namespace BadCode.Main
{
    public class LoadSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        EcsPool<WaitComponent> wait_pool;
        EcsPool<ActComponent> act_pool;
        EcsPool<ReadyComponent> ready_pool;
        EcsPool<LoadComponent> load_pool;
        EcsPool<LoadRequestComponent> request_pool;

        EcsFilter load_wait_filter;
        EcsFilter load_act_filter;
        EcsFilter load_ready_filter;
        public void Init (EcsSystems systems) 
        {
            world = systems.GetWorld();
            //pool
            wait_pool = world.GetPool<WaitComponent>(); 
            act_pool = world.GetPool<ActComponent>(); 
            ready_pool = world.GetPool<ReadyComponent>(); 
            load_pool = world.GetPool<LoadComponent>(); 
            request_pool = world.GetPool<LoadRequestComponent>(); 
            //filter's
            load_wait_filter = world.Filter<LoadComponent>().Inc<WaitComponent>().End();
            load_act_filter = world.Filter<LoadComponent>().Inc<ActComponent>().End();
            load_ready_filter = world.Filter<LoadComponent>().Inc<ReadyComponent>().End();
        }
        
        public void Run (EcsSystems systems) 
        {
            RunActLoad();
            RunLoadWait();
        }

        void RunLoadWait()
        {
            foreach (int entity in load_wait_filter) 
            {
                ref LoadComponent load = ref load_pool.Get(entity);
                ResourceRequest res_request = Resources.LoadAsync(load.path);
                ref LoadRequestComponent request = ref request_pool.Add(entity);
                request.request = res_request;
                //start action
                act_pool.Add(entity);
                //clear wait
                wait_pool.Del(entity);
            }
        }

        void RunActLoad()
        {
            foreach (int entity in load_act_filter) 
            {
                ref LoadRequestComponent request = ref request_pool.Get(entity);
                request.progress = request.request.progress;
                if(request.request.isDone)
                {
                    ref LoadComponent load = ref load_pool.Get(entity);
                    load.obj = request.request.asset;
                    //add ready
                    ready_pool.Add(entity);
                    //clear request
                    request.request = null;
                    request_pool.Del(entity);
                    //clear act
                    act_pool.Del(entity);
                }
            }
        }
    }
}
