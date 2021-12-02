using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class UIBaseSystem<T> : IEcsInitSystem, IEcsRunSystem where T: struct
    {
        protected EcsWorld world;
        //pool
        protected EcsPool<WaitComponent> wait_pool;
        protected EcsPool<ActComponent> act_pool;
        protected EcsPool<ReadyComponent> ready_pool;
        protected EcsPool<UIObjComponent> obj_pool; 
        protected EcsPool<UICloseComponent> close_pool; 
        protected EcsPool<UIWaitCloseComponent> wait_close_pool; 
        protected EcsPool<UIActCloseComponent> act_close_pool; 
        protected EcsPool<T> ui_pool;
        //filter
        protected EcsFilter ui_filter;
        protected EcsFilter ui_wait_filter;
        protected EcsFilter ui_act_filter;
        protected EcsFilter ui_ready_filter;
        protected EcsFilter ui_wait_close_filter;
        protected EcsFilter ui_act_close_filter;
        //shared
        protected LoadMgr load_mgr;
        protected UIMgr ui_mgr;
        protected DtMgr dt_mgr;
        protected string res_path;
        protected float dt_tm;
        protected List<int> change_list;
        
        public void Init (EcsSystems systems) 
        {
            world = systems.GetWorld();
            load_mgr = systems.GetShared<WorldMgr>().GetMgr<LoadMgr>();
            ui_mgr = systems.GetShared<WorldMgr>().GetMgr<UIMgr>();
            dt_mgr = systems.GetShared<WorldMgr>().GetMgr<DtMgr>();
            //pool
            wait_pool = world.GetPool<WaitComponent>(); 
            act_pool = world.GetPool<ActComponent>(); 
            ready_pool = world.GetPool<ReadyComponent>(); 
            obj_pool = world.GetPool<UIObjComponent>(); 
            close_pool = world.GetPool<UICloseComponent>(); 
            wait_close_pool = world.GetPool<UIWaitCloseComponent>(); 
            act_close_pool = world.GetPool<UIActCloseComponent>(); 
            ui_pool = world.GetPool<T>(); 

            //filter's
            ui_wait_filter = world.Filter<T>().Inc<WaitComponent>().End();
            ui_act_filter = world.Filter<T>().Inc<ActComponent>().End();
            ui_ready_filter = world.Filter<T>().Inc<ReadyComponent>().End();
            ui_wait_close_filter = world.Filter<T>().Inc<UICloseComponent>().Inc<UIWaitCloseComponent>().End();
            ui_act_close_filter = world.Filter<T>().Inc<UICloseComponent>().Inc<UIActCloseComponent>().End();
            ui_filter = world.Filter<T>().End();
            change_list = new List<int>();
            OnCreateSystem( systems );
        }

        public void Run (EcsSystems systems) 
        {
            if(ui_filter.GetEntitiesCount() == 0) return;

            dt_tm = dt_mgr.GetDt();
            RunUIWait();
            RunUIAct();
            RunUIReady();

            RunCloseWait();
            RunCloseAct();
        }

        protected virtual void OnCreateSystem( EcsSystems systems ){}
        protected virtual void OnInit(int entity){}

        protected virtual void RunUIWait()
        {
            if(string.IsNullOrEmpty(res_path))
            {
                UnityEngine.Debug.LogErrorFormat("res path is empty for ui {0}", typeof(T).ToString());
                Clear();
                return;
            }

            foreach(int entity in ui_wait_filter)
            {
                load_mgr.Load(res_path);
                act_pool.Add(entity);
                wait_pool.Del(entity);
            }
        }

        protected virtual void RunUIAct()
        {
            if(ui_act_filter.GetEntitiesCount() == 0) return;
            
            change_list.Clear();
            foreach(int entity in ui_act_filter)
            {
                Object obj;
                if(load_mgr.GetObject(res_path, out obj))
                {
                    ready_pool.Add(entity);
                    act_pool.Del(entity);
                    ref UIObjComponent obj_cmp = ref obj_pool.Add(entity);
                    obj_cmp.body_go = CreateInstance(obj);
                    obj_cmp.body_tr = obj_cmp.body_go.transform;
                    obj_cmp.body_cg = obj_cmp.body_go.AddComponent<CanvasGroup>();
                    change_list.Add( entity );
                    Debug.LogFormat("craete instance: {0}", this.GetType().Name);
                }
            } 

            for(int i=0; i< change_list.Count; i++)
            {
                OnInit( change_list[i] );
            }
        }

        protected virtual void RunUIReady(){}

        protected virtual void  RunCloseWait()
        {
            if(ui_wait_close_filter.GetEntitiesCount() == 0) return;
            
            change_list.Clear();
            foreach( int entity in ui_wait_close_filter)
            {
                change_list.Add( entity );
                wait_close_pool.Del( entity );
                act_close_pool.Add( entity );
            }

            for(int i=0; i < change_list.Count; i++)
            {
                OnCloseStart( change_list[i] );
            }
        }

        protected virtual void  RunCloseAct()
        {
            foreach( int entity in ui_act_close_filter)
            {
                ref UICloseComponent close_cmp = ref close_pool.Get( entity );
                close_cmp.tm_cur += dt_tm;
                OnCloseAct( entity, close_cmp.tm_total, close_cmp.tm_cur );
                if(close_cmp.tm_cur >= close_cmp.tm_total)
                {
                    change_list.Add( entity );
                    ref UIObjComponent obj_cmp = ref obj_pool.Get( entity );
                    OnCloseComplete( obj_cmp.body_go );
                    world.DelEntity( entity );
                }
            }
        }

        protected virtual void OnCloseStart( int entity ){}
        protected virtual void OnCloseAct( int entity, float tm_total, float tm_cur )
        {
            ref UIObjComponent obj_cmp = ref obj_pool.Get( entity );
            obj_cmp.body_cg.alpha = Mathf.Lerp(1f, 0f, tm_cur / tm_total);
        }
        protected virtual void OnCloseComplete( UnityEngine.GameObject go )
        {
            UnityEngine.GameObject.Destroy( go );
        }

        protected virtual void Clear()
        {
            foreach(int entity in ui_filter)
            {
                world.DelEntity(entity);
            }
        }

        protected GameObject CreateInstance(Object obj)
        {
            GameObject go = GameObject.Instantiate(obj as GameObject);
            Transform tr = go.transform;
            tr.SetParent(ui_mgr.GetCanvasTransfrom(), false);
            tr.SetAsFirstSibling();
            return go;
        }

        protected bool IsUIExists()
        {
            if( ui_act_filter.GetEntitiesCount() > 0) return true;
            if( ui_ready_filter.GetEntitiesCount() > 0) return true;

            return false;
        }
    }
}