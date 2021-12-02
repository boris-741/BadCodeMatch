using System.Collections.Generic;
using UnityEngine;
using Leopotam.EcsLite;
using Pool;

namespace BadCode.Main
{
    public class StartSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        //pool
        EcsPool<StartComponent> start_pool;
        EcsPool<WaitComponent> wait_pool;
        EcsPool<ActComponent> act_pool;
        //filter
        EcsFilter start_wait_filter;
        EcsFilter start_act_filter;
        //mgr
        DtMgr dt_mgr;
        UIMgr ui_mgr;
        PoolMgr pool_mgr;
        sPool<EcsPackedEntity> pack_pool;
        
        public void Init(EcsSystems systems) 
        {
            world = systems.GetWorld();

            pack_pool = new sPool<EcsPackedEntity>();

            //pool's
            start_pool = world.GetPool<StartComponent>(); 
            wait_pool = world.GetPool<WaitComponent>(); 
            act_pool = world.GetPool<ActComponent>(); 

            //filter's
            start_wait_filter = world.Filter<StartComponent>().Inc<WaitComponent>().End();
            start_act_filter = world.Filter<StartComponent>().Inc<ActComponent>().End();

            //mgr
            dt_mgr = systems.GetShared<WorldMgr>().GetMgr<DtMgr>();
            ui_mgr = systems.GetShared<WorldMgr>().GetMgr<UIMgr>();
            pool_mgr = systems.GetShared<WorldMgr>().GetMgr<PoolMgr>();
        }

        public void Run(EcsSystems systems) 
        {
            RunWaitStart();
            RunActStart();
        }

        void RunWaitStart()
        {
            foreach(int entity in start_wait_filter)
            {
                ref StartComponent start_cmp = ref start_pool.Get(entity);
                wait_pool.Del(entity);
                act_pool.Add(entity);
                //init start wnd
                start_cmp.process_dic = CreateProcessDic();
                start_cmp.process_dic[StartProcess.init_ui_start] = Time.realtimeSinceStartup;
                ref UIStartComponent start_ui  = ref ui_mgr.CreateWindow<UIStartComponent>();
                start_ui.main_go = start_cmp.loading_go;
            }
        }

        void RunActStart()
        {
            foreach(int entity in start_act_filter)
            {
                ref StartComponent start_cmp = ref start_pool.Get(entity);
                float time = Time.realtimeSinceStartup;
                if(start_cmp.process_dic[StartProcess.init_ui_start] < 0f) continue;
                if(time - start_cmp.process_dic[StartProcess.init_ui_start] < 1.5f) continue;
                
                if(start_cmp.process_dic[StartProcess.init_ui_main_console] < 0f)
                {
                    ref UIGameConsoleComponent game_console_ui  = ref ui_mgr.CreateWindow<UIGameConsoleComponent>();
                    start_cmp.process_dic[StartProcess.init_ui_main_console] = time;
                }

                if(start_cmp.process_dic[StartProcess.ready_main_console] < 0f)
                {
                    if(ui_mgr.IsWindowReady<UIGameConsoleComponent>())
                    {
                        start_cmp.process_dic[StartProcess.ready_main_console] = time;
                    }
                }

                if(start_cmp.process_dic[StartProcess.preapare_pool] < 0f)
                {
                    pool_mgr.InitPool();
                    start_cmp.process_dic[StartProcess.preapare_pool] = time;
                }

                if(start_cmp.process_dic[StartProcess.ready_pool] < 0f)
                {
                    if(pool_mgr.IsInitPool())
                    {
                        pool_mgr.ClearInit();
                        start_cmp.process_dic[StartProcess.ready_pool] = time;
                        Debug.LogFormat("init pool complete");
                    }
                }

                if( IsLoadComlete(start_cmp.process_dic) )
                {
                    start_cmp.process_dic = null;
                    ui_mgr.CloseWindowReady<UIStartComponent>();
                    start_pool.Del( entity );
                    Debug.LogFormat("finish start");
                }
            }
        }

        bool IsLoadComlete(Dictionary<StartProcess, float> process_dic)
        {
            foreach(KeyValuePair<StartProcess, float> pair in process_dic)
            {
                if(pair.Value < 0f) return false;
            }

            return true;
        }

        Dictionary<StartProcess, float>  CreateProcessDic()
        {
            Dictionary<StartProcess, float> process_dic = new Dictionary<StartProcess, float> ();
            process_dic[StartProcess.init_ui_start] = -1f;
            process_dic[StartProcess.init_ui_main_console] = -1f;
            process_dic[StartProcess.ready_main_console] = -1f;
            process_dic[StartProcess.preapare_pool] = -1f;
            process_dic[StartProcess.ready_pool] = -1f;
            return process_dic;
        }
    }
}