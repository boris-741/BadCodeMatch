using UnityEngine;
using UnityEngine.UI;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class UIMgr : BaseMgr
    {
        Canvas canvas;
        Transform canvas_tr;
        //pool
        EcsPool<WaitComponent> wait_pool;
        EcsPool<UICloseComponent> close_pool;
        EcsPool<UIWaitCloseComponent> wait_close_pool;

        public UIMgr(EcsWorld world) : base(world)
        {
            this.world = world;
            wait_pool = world.GetPool<WaitComponent>();
            close_pool = world.GetPool<UICloseComponent>();
            wait_close_pool = world.GetPool<UIWaitCloseComponent>();
        }

        public void SetCanvas(Canvas canvas)
        {
            this.canvas = canvas;
            this.canvas_tr = canvas.transform;
        }

        public Canvas GetCanvas()
        {
            return canvas;
        }

        public Transform GetCanvasTransfrom()
        {
            return canvas_tr;
        }

        public ref T CreateWindow<T>() where T: struct
        {
            int entity = world.NewEntity();
            EcsPool<T> pool = world.GetPool<T>();
            ref T t = ref pool.Add(entity);
            wait_pool.Add(entity);
            return ref t;
        }

        public bool IsWindowReady<T>() where T: struct
        {
            EcsFilter filter = world.Filter<T>().Inc<ReadyComponent>().End();
            return filter.GetEntitiesCount() > 0;
        }

        public void CloseWindowReady<T>( float close_time = 0.3f ) where T: struct
        {
            EcsFilter filter = world.Filter<T>().Inc<ReadyComponent>().End();
            foreach(int entity in filter)
            {
                if( !close_pool.Has( entity ) )
                {
                    ref UICloseComponent close_cmp =  ref close_pool.Add( entity );
                    close_cmp.tm_total = close_time;
                    wait_close_pool.Add( entity );
                }
            }
        }
    }
}