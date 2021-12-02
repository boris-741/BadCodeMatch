using UnityEngine;
using UnityEngine.U2D;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class MatchClearSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        //manager's
        PoolMgr pool_mgr;
        //pool
        EcsPool<MatchClearGameComponent> clear_pool;
        //view pool
        // EcsPool<JemViewComponent> jem_view_pool;
        // EcsPool<BonusViewComponent> bonus_view_pool;
        // EcsPool<BlockViewComponent> block_view_pool;
        // EcsPool<JemCollapseComponent> jem_collapse_pool;
        EcsPool<GameObjectComponent> go_pool;
        //filter's
        EcsFilter clear_filter;
        EcsFilter init_match_filter;
        EcsFilter match_cell_filter;
        //view filter's
        // EcsFilter jem_filter;
        // EcsFilter bonus_filter;
        // EcsFilter block_filter;
        // EcsFilter jem_collapse_filter;
        EcsFilter go_filter;
        

        public void Init(EcsSystems systems) 
        {
            world = systems.GetWorld();
            //manager's
            pool_mgr = systems.GetShared<WorldMgr>().GetMgr<PoolMgr>();
            //pool
            clear_pool = world.GetPool<MatchClearGameComponent>();
            //view pool
            // jem_view_pool = world.GetPool<JemViewComponent>();
            // bonus_view_pool = world.GetPool<BonusViewComponent>();
            // block_view_pool = world.GetPool<BlockViewComponent>();
            // jem_collapse_pool = world.GetPool<JemCollapseComponent>();
            go_pool = world.GetPool<GameObjectComponent>();
            //filter's
            clear_filter = world.Filter<MatchClearGameComponent>().End();
            init_match_filter = world.Filter<MatchInitGameComponent>().End();
            match_cell_filter = world.Filter<MatchCellComponent>().End();
            //view filter's
            // jem_filter = world.Filter<JemViewComponent>().End();
            // bonus_filter = world.Filter<BonusViewComponent>().End();
            // block_filter = world.Filter<BlockViewComponent>().End();
            // jem_collapse_filter = world.Filter<JemCollapseComponent>().End();
            go_filter = world.Filter<GameObjectComponent>().End();
        }

        public void Run(EcsSystems systems) 
        {
            RunClear();
        }

        void RunClear()
        {
            foreach( int entity in clear_filter )
            {
                //clear view
                // ClearJemView();
                // ClearBonusView();
                // ClearJemCollapseView();
                // ClearBlockView();
                DelGO();
                //del entitie's
                DelCellMatch();
                DelInitMatch();
                //self del
                clear_pool.Del( entity );
            }
        }

        // void ClearJemView()
        // {
        //     foreach( int entity in jem_filter)
        //     {
        //         ref JemViewComponent cmp = ref jem_view_pool.Get( entity );
        //         if( cmp.tr != null )
        //         {
        //             pool_mgr.ReleaseGO( cmp.tr.gameObject );
        //         }
        //     }
        // }

        // void ClearBonusView()
        // {
        //     foreach( int entity in bonus_filter)
        //     {
        //         ref BonusViewComponent cmp = ref bonus_view_pool.Get( entity );
        //         if( cmp.tr != null )
        //         {
        //             pool_mgr.ReleaseGO( cmp.tr.gameObject );
        //         }
        //     }
        // }

        // void ClearBlockView()
        // {
        //     foreach( int entity in block_filter)
        //     {
        //         ref BlockViewComponent cmp = ref block_view_pool.Get( entity );
        //         if( cmp.tr != null )
        //         {
        //             pool_mgr.ReleaseGO( cmp.tr.gameObject );
        //         }
        //     }
        // }

        // void ClearJemCollapseView()
        // {
        //     foreach( int entity in jem_collapse_filter)
        //     {
        //         ref JemCollapseComponent cmp = ref jem_collapse_pool.Get( entity );
        //         cmp.sr = null;
        //         if( cmp.tr != null )
        //         {
        //             pool_mgr.ReleaseGO( cmp.tr.gameObject );
        //         }
        //     }
        // }

        void DelInitMatch()
        {
            foreach( int entity in init_match_filter )
            {
                world.DelEntity( entity );
            }
        }

        void DelCellMatch()
        {
            foreach( int entity in match_cell_filter )
            {
                world.DelEntity( entity );
            }
        }

        void DelGO()
        {
            foreach( int entity in go_filter )
            {
                ref GameObjectComponent cmp = ref go_pool.Get( entity );
                pool_mgr.ReleaseGO( cmp.go );
                world.DelEntity( entity );
            }
        }
    }
}