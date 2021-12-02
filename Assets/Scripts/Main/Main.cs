using UnityEngine;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class Main : MonoBehaviour
    {

        public Canvas canvas;
        public GameObject loading;
        public Transform match_tr;
        public Camera main_camera;

        EcsWorld _world;
        EcsSystems _systems;
        DtMgr dt_mgr;

        void Start() 
        {
            //world
            _world = new EcsWorld();
            //manager
            WorldMgr worldMgr = new WorldMgr(_world);
            UIMgr ui_mgr = worldMgr.GetMgr<UIMgr>();
            dt_mgr = worldMgr.GetMgr<DtMgr>();
            ui_mgr.SetCanvas(canvas);
            MatchMgr match_mgr = worldMgr.GetMgr<MatchMgr>();
            match_mgr.SetMatchTr( match_tr );
            InputMgr input_mgr = worldMgr.GetMgr<InputMgr>();
            input_mgr.SetCamera( main_camera );
            //systems
            _systems = new EcsSystems(_world, worldMgr)
                //start
                .Add (new StartSystem())
                //input
                .Add (new ImputSystem())
                //load res
                .Add(new LoadSystem())
                .Add(new PoolSystem())
                //ui
                .Add (new UIStartSystem())
                .Add (new UIGameConsole())
                //game
                .Add( new MatchSystem())
                .Add( new MatchViewSystem())
                .Add(new MatchClearSystem());
            _systems.Init();

            //init start wnd
            InitStart();
        }
        
        void Update() 
        {
            dt_mgr.SetDt(dt: Time.deltaTime, smooth_dt: Time.smoothDeltaTime);
            _systems.Run();
        }

        void InitStart()
        {
            int entity = _world.NewEntity();
            EcsPool<StartComponent> start_pool = _world.GetPool<StartComponent>();
            EcsPool<WaitComponent> wait_pool = _world.GetPool<WaitComponent>();
            ref StartComponent start_cmp = ref start_pool.Add(entity);
            start_cmp.loading_go = loading;
            wait_pool.Add(entity);
        }
    }
}
