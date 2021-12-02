using UnityEngine;
using UnityEngine.UI;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    //public class UIMainConsole<T> : UIConsoleSystem<T> where T: struct
    public class UIGameConsole : UIConsoleSystem<UIGameConsoleComponent>
    {
        MatchMgr match_mgr;

        protected override void OnCreateSystem( EcsSystems systems )
        {
            res_path = "Prefabs/GameConsole";
            match_mgr = systems.GetShared<WorldMgr>().GetMgr<MatchMgr>();
        }

        protected override void OnInit(int entity)
        {
            ref UIObjComponent obj_cmp = ref obj_pool.Get( entity );
            Button btn = obj_cmp.body_tr.Find("BottomLeftPanel/BtnPlay").GetComponent<Button>();
            btn.onClick.AddListener( ()=>OnStartPlayClick(entity) );
        }

        void OnStartPlayClick(int entity)
        {
            bool is_game = match_mgr.IsGame();
            Debug.LogFormat("game: {0}, OnStartPlayClick, entity={1} is game={2}", 
                                this.GetType().Name, entity, is_game);
            if( !is_game )
            {
                GameConfig cfg = new GameConfig{
                    size_x = 9,
                    size_y = 9,
                    bound_x = 72,
                    bound_y = 72,
                    min_dist = 0.0001f,
                    user_move_speed = 200f,
                    move_speed = 400f,
                    tm_wait_collapse = 0.2f,
                    tm_bonus_create = 1f,
                    level_id = 1
                };
                match_mgr.StartGame( cfg );
            }
            else
            {
                match_mgr.EndGame();
            }
        }
    }
}
