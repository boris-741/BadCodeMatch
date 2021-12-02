using UnityEngine;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class MatchMgr : BaseMgr
    {
        //pool's
        EcsPool<WaitComponent> wait_pool;
        EcsPool<MatchInitGameComponent> init_game_pool;
        EcsPool<MatchClearGameComponent> clear_game_pool;
        //filter's
        EcsFilter wait_init_filter;
        EcsFilter act_init_filter;
        
        Transform match_tr;
        
        public MatchMgr(EcsWorld world) : base(world)
        {
            this.world = world;
            //pool
            wait_pool = world.GetPool<WaitComponent>();
            init_game_pool = world.GetPool<MatchInitGameComponent>();
            clear_game_pool = world.GetPool<MatchClearGameComponent>();

            //filter
            wait_init_filter = world.Filter<MatchInitGameComponent>().Inc<WaitComponent>().End();
            act_init_filter = world.Filter<MatchInitGameComponent>().Inc<ActComponent>().End();
        }

        public void SetMatchTr(Transform tr)
        {
            this.match_tr = tr;
        }

        public Transform GetMatchTr()
        {
            return match_tr;
        }

        public bool IsGame()
        {
            if( wait_init_filter.GetEntitiesCount() > 0 ) return true; // game init
            if( act_init_filter.GetEntitiesCount() > 0 ) return true; // game start

            return false;
        }

        public void StartGame( GameConfig cfg )
        {
            if( IsGame() )
            {
                Debug.LogErrorFormat("game is started!");
            }
            else
            {
                int entity = world.NewEntity();
                ref MatchInitGameComponent init_cmp = ref init_game_pool.Add( entity );
                init_cmp.config = cfg;
                wait_pool.Add( entity );
            }
        }

        public void EndGame()
        {
            if( !IsGame() )
            {
                Debug.LogErrorFormat("game not start!");
            }
            else
            {
                int entity = world.NewEntity();
                clear_game_pool.Add( entity );
            }
        }
    }
}