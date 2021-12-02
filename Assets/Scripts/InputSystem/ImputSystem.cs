using UnityEngine;
using Leopotam.EcsLite;


namespace BadCode.Main
{
    public class ImputSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;

        //manager's
        InputMgr input_mgr;
        //pool's
        EcsPool<ImputComponent> input_pool;
        EcsPool<TapComponent> tap_pool;
        EcsPool<DragComponent> drag_pool;
        //filter's
        EcsFilter input_filter;
        EcsFilter tap_filter;
        EcsFilter drag_filter;
        uint touch_id;
        float delta_hover_pos = 5f;
        
        public void Init (EcsSystems systems) 
        {
            world = systems.GetWorld();
            //manager's
            input_mgr = systems.GetShared<WorldMgr>().GetMgr<InputMgr>();
            //pool's
            input_pool = world.GetPool<ImputComponent>(); 
            tap_pool = world.GetPool<TapComponent>(); 
            drag_pool = world.GetPool<DragComponent>(); 
            //filter's
            input_filter = world.Filter<ImputComponent>().End();
            tap_filter = world.Filter<TapComponent>().End();
            drag_filter = world.Filter<DragComponent>().End();
        }

        public void Run (EcsSystems systems) 
        {
            ClearTap();
            ClearDrag();
            
            #if UNITY_STANDALONE || UNITY_EDITOR
                if(Input.GetMouseButtonDown(0))
                {   
                    OnTapStart(Input.mousePosition);
                }
                else if(Input.GetMouseButton(0) || Input.GetMouseButton(1))   
                {
                    OnTapHover(Input.mousePosition);
                }
                else if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    OnTapEnd();
                }
            #elif UNITY_IOS || UNITY_ANDROID
                if(Input.touchCount == 1)
                {
                    switch (Input.touches[0].phase)
                    {
                        case TouchPhase.Began:
                            OnTapStart(Input.touches[0].position);
                        break;
                        case TouchPhase.Stationary:
                        case TouchPhase.Moved:
                            OnTapHover(Input.touches[0].position);
                        break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            OnTapEnd();
                        break;
                    }
                }
            #endif
        }

        void OnTapStart(Vector2 pos)
        {
            if(input_filter.GetEntitiesCount() == 0)
            {
                touch_id += 1;
                int entity = world.NewEntity();
                ref ImputComponent input_cmp = ref input_pool.Add( entity );
                input_cmp.start_pos = pos;
                input_cmp.last_pos = pos;
                input_cmp.touch_id = touch_id;
            }
            else
            {
                foreach(int entity in input_filter)
                {
                    ref ImputComponent input_cmp = ref input_pool.Get( entity );
                    input_cmp.start_pos = pos;
                    input_cmp.last_pos = pos;
                }
            }
        }

        void OnTapHover(Vector2 pos)
        {
            foreach(int entity in input_filter)
            {
                ref ImputComponent input_cmp = ref input_pool.Get( entity );
                //send drag
                int drag_entity = world.NewEntity();
                ref DragComponent drag_cmp = ref drag_pool.Add( drag_entity );
                drag_cmp.touch_id = input_cmp.touch_id;
                drag_cmp.start_pos = input_cmp.start_pos;
                drag_cmp.last_pos = pos;
                drag_cmp.delta_pos = pos - input_cmp.last_pos;
                //set last pos
                input_cmp.last_pos = pos;
            }
        }

        void OnTapEnd()
        {
            foreach(int entity in input_filter)
            {
                ref ImputComponent input_cmp = ref input_pool.Get( entity );
                float pos_dist = Vector2.Distance(input_cmp.start_pos, input_cmp.last_pos);
                if(  pos_dist < delta_hover_pos )
                {
                    //send tap
                    int tap_entity = world.NewEntity();
                    ref TapComponent tap_cmp = ref tap_pool.Add( tap_entity );
                    tap_cmp.touch_id = input_cmp.touch_id;
                    tap_cmp.pos = input_cmp.start_pos;
                }
                //clear input
                world.DelEntity( entity );
            }
        }

        void ClearTap()
        {
            foreach(int entity in tap_filter)
            {
                world.DelEntity( entity );
            }
        }

        void ClearDrag()
        {
            foreach(int entity in drag_filter)
            {
                world.DelEntity( entity );
            }
        }
    }
}
