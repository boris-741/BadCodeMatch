using UnityEngine;
using UnityEngine.U2D;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class MatchViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        //mng
        MatchMgr match_mgr;
        PoolMgr pool_mgr;
        DtMgr dt_mgr;
        //match-pool
        EcsPool<MatchInitGameComponent> init_game_pool;
        EcsPool<MatchCellUserMoveComponent> cell_user_move_pool;
        EcsPool<MatchCellComponent> cell_pool;
        EcsPool<MatchJemComponent> jem_pool;
        EcsPool<MatchBonusComponent> bonus_pool;
        EcsPool<MatchBlockComponent> block_pool;
        //view-pool
        EcsPool<InitJemViewComponent> init_jem_view_pool;
        EcsPool<InitBonusViewComponent> init_bonus_view_pool;
        EcsPool<InitBlockViewComponent> init_block_view_pool;
        EcsPool<JemViewComponent> jem_view_pool;
        EcsPool<BonusViewComponent> bonus_view_pool;
        EcsPool<BlockViewComponent> block_view_pool;
        //EcsPool<JemUserMoveComponent> jem_move_user_pool;
        //EcsPool<InitUserMoveViewComponent> init_move_user_pool;
        EcsPool<InitJemCollapseComponent> init_jem_collapse_pool;
        EcsPool<JemCollapseComponent> jem_collapse_pool;
        EcsPool<InitJemToBonusCollapseComponent> init_jem_to_bonus_collapse_pool;
        EcsPool<JemToBonusCollapseComponent> jem_to_bonus_collapse_pool;
        EcsPool<InitBonusCreateViewComponent> init_bonus_create_pool;
        EcsPool<BonusCreateViewComponent> bonus_create_pool;
        EcsPool<MatchCellMoveComponent> match_cell_move_pool;
        EcsPool<MatchCellCorrectViewPos> match_correct_move_pool;
        EcsPool<GameObjectComponent> go_pool;
        //filter's
        EcsFilter act_init_filter;
        EcsFilter init_jem_filter;
        EcsFilter init_bonus_filter;
        EcsFilter init_block_filter;
        //EcsFilter init_jem_user_move_filter;
        //EcsFilter act_jem_user_move_filter;
        EcsFilter jem_user_move_filter;
        EcsFilter bonus_user_move_filter;
        EcsFilter init_jem_collapse_filter;
        EcsFilter act_jem_collapse_filter;
        EcsFilter init_jem_to_bonus_collapse_filter;
        EcsFilter act_jem_to_bonus_collapse_filter;
        EcsFilter init_bonus_create_filter;
        EcsFilter act_bonus_create_filter;
        EcsFilter jem_move_filter;
        EcsFilter bonus_move_filter;
        EcsFilter jem_correct_filter;
        EcsFilter bonus_correct_filter;
        EcsFilter correct_filter;
        EcsFilter go_filter;
        //match
        Transform match_tr;
        GameConfig config;
        bool is_config;
        float dt_time;

        public void Init(EcsSystems systems) 
        {
            world = systems.GetWorld();
            match_mgr = systems.GetShared<WorldMgr>().GetMgr<MatchMgr>();
            pool_mgr = systems.GetShared<WorldMgr>().GetMgr<PoolMgr>();
            dt_mgr = systems.GetShared<WorldMgr>().GetMgr<DtMgr>();
            //pool
            init_game_pool = world.GetPool<MatchInitGameComponent>();
            cell_user_move_pool = world.GetPool<MatchCellUserMoveComponent>();
            cell_pool = world.GetPool<MatchCellComponent>();
            jem_pool = world.GetPool<MatchJemComponent>();
            init_jem_view_pool = world.GetPool<InitJemViewComponent>();
            init_bonus_view_pool = world.GetPool<InitBonusViewComponent>();
            init_block_view_pool = world.GetPool<InitBlockViewComponent>();
            jem_view_pool = world.GetPool<JemViewComponent>();
            bonus_view_pool = world.GetPool<BonusViewComponent>();
            block_view_pool = world.GetPool<BlockViewComponent>();
            bonus_pool = world.GetPool<MatchBonusComponent>();
            block_pool = world.GetPool<MatchBlockComponent>();
            //jem_move_user_pool = world.GetPool<JemUserMoveComponent>();
            //init_move_user_pool = world.GetPool<InitUserMoveViewComponent>();
            init_jem_collapse_pool = world.GetPool<InitJemCollapseComponent>();
            jem_collapse_pool = world.GetPool<JemCollapseComponent>();
            init_jem_to_bonus_collapse_pool = world.GetPool<InitJemToBonusCollapseComponent>();
            jem_to_bonus_collapse_pool = world.GetPool<JemToBonusCollapseComponent>();
            init_bonus_create_pool = world.GetPool<InitBonusCreateViewComponent>();
            bonus_create_pool = world.GetPool<BonusCreateViewComponent>();
            match_cell_move_pool = world.GetPool<MatchCellMoveComponent>();
            match_correct_move_pool = world.GetPool<MatchCellCorrectViewPos>();
            go_pool = world.GetPool<GameObjectComponent>();
            //filter's
            act_init_filter = world.Filter<MatchInitGameComponent>().Inc<ActComponent>().End();
            init_jem_filter = world.Filter<JemViewComponent>().Inc<InitJemViewComponent>().Inc<MatchJemComponent>().End();
            init_bonus_filter = world.Filter<BonusViewComponent>().Inc<InitBonusViewComponent>().End();
            init_block_filter = world.Filter<BlockViewComponent>().Inc<InitBlockViewComponent>().End();
            //init_jem_user_move_filter = world.Filter<JemUserMoveComponent>().Inc<JemViewComponent>().Inc<InitUserMoveViewComponent>().End();
            //act_jem_user_move_filter = world.Filter<JemUserMoveComponent>().Inc<JemViewComponent>().Exc<InitUserMoveViewComponent>().End();
            init_jem_collapse_filter = world.Filter<JemCollapseComponent>().Inc<InitJemCollapseComponent>().End();
            act_jem_collapse_filter = world.Filter<JemCollapseComponent>().Exc<InitJemCollapseComponent>().End();
            init_jem_to_bonus_collapse_filter = world.Filter<JemToBonusCollapseComponent>().Inc<InitJemToBonusCollapseComponent>().End();
            act_jem_to_bonus_collapse_filter = world.Filter<JemToBonusCollapseComponent>().Exc<InitJemToBonusCollapseComponent>().End();
            init_bonus_create_filter = world.Filter<BonusCreateViewComponent>().Inc<InitBonusCreateViewComponent>().End();
            act_bonus_create_filter = world.Filter<BonusCreateViewComponent>().Exc<InitBonusCreateViewComponent>().End();
            jem_user_move_filter = world.Filter<MatchCellUserMoveComponent>().Inc<JemViewComponent>().End();
            bonus_user_move_filter = world.Filter<MatchCellUserMoveComponent>().Inc<BonusViewComponent>().End();
            jem_move_filter = world.Filter<MatchCellMoveComponent>().Inc<JemViewComponent>().End();
            bonus_move_filter = world.Filter<MatchCellMoveComponent>().Inc<BonusViewComponent>().End();
            jem_correct_filter = world.Filter<MatchCellCorrectViewPos>().Inc<JemViewComponent>().End();
            bonus_correct_filter = world.Filter<MatchCellCorrectViewPos>().Inc<BonusViewComponent>().End();
            correct_filter = world.Filter<MatchCellCorrectViewPos>().End();
            go_filter = world.Filter<GameObjectComponent>().End();
            //match
            match_tr = match_mgr.GetMatchTr();
        }

        public void Run(EcsSystems systems) 
        {
            if( act_init_filter.GetEntitiesCount() == 0) 
            {
                return;
            }
            GetInit();
            dt_time = dt_mgr.GetDt();

            RunViewJem();
            RunViewBonus();
            RunViewBlock();
            //RunMoveUserJem();
            RunUserJemMove();
            RunUserBonusMove();
            RunMatchJemCorrect();
            RonMatchBonusCorrect();
            RunMatchJemMove();
            RunMatchBonusMove();

            RunJemCollapse();
            RunJemToBonusCollapse();
            RunBonusCreate();

            RunClearCorrect();
        }

        void GetInit()
        {
            if( is_config ) return;

            foreach(int entity in act_init_filter)
            {
                ref MatchInitGameComponent init = ref init_game_pool.Get( entity );
                config = init.config;
            }
            is_config = true;
        }

        void RunViewJem()
        {
            foreach(int entity in init_jem_filter)
            {
                
                ref MatchCellComponent cell_cmp = ref cell_pool.Get( entity ); 
                ref MatchJemComponent jem_cmp = ref jem_pool.Get( entity );
                ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                CoreObjectType obj_type = GetJemObject( jem_cmp.jem );
                GameObject go = GetPoolGo( obj_type ); //pool_mgr.GetGO( core_type );
                jem_view_cmp.tr = go.transform;
                SetParent(jem_view_cmp.tr, cell_cmp.pos_x, cell_cmp.pos_y);
                init_jem_view_pool.Del( entity );
            }
        }

        void RunViewBonus()
        {
            foreach( int entity in init_bonus_filter )
            {
                ref MatchCellComponent cell_cmp = ref cell_pool.Get( entity ); 
                ref MatchBonusComponent bonus_cmp = ref bonus_pool.Get( entity );
                ref BonusViewComponent bonus_view_cmp = ref bonus_view_pool.Get( entity );
                CoreObjectType obj_type = GetBonusObject(bonus_cmp.bonus, bonus_cmp.jem);
                GameObject go = GetPoolGo( obj_type ); //pool_mgr.GetGO( obj_type );
                bonus_view_cmp.tr = go.transform;
                SetParent(bonus_view_cmp.tr, cell_cmp.pos_x, cell_cmp.pos_y);
                bonus_view_cmp.tr.localEulerAngles = GetBonusAngle( bonus_cmp.bonus );
                init_bonus_view_pool.Del( entity );
            }
        }

        void RunViewBlock()
        {
            foreach( int entity in init_block_filter )
            {
                ref MatchCellComponent cell_cmp = ref cell_pool.Get( entity ); 
                ref MatchBlockComponent match_block_cmp = ref block_pool.Get( entity );
                ref BlockViewComponent block_view_cmp = ref block_view_pool.Get( entity );
                CoreObjectType obj_type = GetBlockObject( match_block_cmp.block );
                GameObject go = GetPoolGo( obj_type ); //pool_mgr.GetGO( obj_type );
                block_view_cmp.tr = go.transform;
                SetParent(block_view_cmp.tr, cell_cmp.pos_x, cell_cmp.pos_y);

                init_block_view_pool.Del( entity );
            }
        }

        // void RunMoveUserJem()
        // {
        //     //init
        //     foreach(int entity in init_jem_user_move_filter)
        //     {
        //         ref JemUserMoveComponent cmp = ref jem_move_user_pool.Get( entity );
        //         ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
        //         cmp.tr = jem_view_cmp.tr;
                
        //     }
        //     //change and clear
        //     foreach(int entity in init_jem_user_move_filter)
        //     {
                
        //         ref JemUserMoveComponent cmp = ref jem_move_user_pool.Get( entity );
        //         //change entity
        //         if( cmp.is_user_active ) 
        //         {
        //             ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
        //             ref JemViewComponent next_jem_view_cmp = ref jem_view_pool.Get( cmp.to_entity );
        //             Transform jem_tr = jem_view_cmp.tr;
        //             Transform next_jem_tr = next_jem_view_cmp.tr;
        //             jem_view_cmp.tr = next_jem_tr;
        //             next_jem_view_cmp.tr = jem_tr;
        //             Debug.LogFormat("change jems entity={0}, to_entity={1}", entity, cmp.to_entity);
        //         }
        //         //clear init
        //         init_move_user_pool.Del( entity );
        //     }
        //     //move
        //     foreach(int entity in act_jem_user_move_filter)
        //     {
        //         ref JemUserMoveComponent cmp = ref jem_move_user_pool.Get( entity );
        //         cmp.tm_move += dt_time;
        //         Vector2 pos = Vector2.MoveTowards(cmp.from, cmp.to, cmp.user_move_speed * cmp.tm_move );
        //         cmp.tr.position = pos;
        //         if(Vector2.Distance(pos, cmp.to) < config.min_dist)
        //         {
        //             jem_move_user_pool.Del( entity );
        //         }
        //     }
        // }

        void RunJemCollapse()
        {
            //init
            foreach(int entity in init_jem_collapse_filter)
            {
                ref JemCollapseComponent collapse_cmp = ref jem_collapse_pool.Get( entity );
                //wait collapse
                collapse_cmp.tm_wait_collapse -= dt_time;
                if( collapse_cmp.tm_wait_collapse > 0f ) continue;

                ref MatchCellComponent cell_cmp = ref cell_pool.Get( entity ); 
                //ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                //init collapse
                SpriteRenderer sr;
                Sprite sprite;
                GameObject go = GetEmtyGo( out sr ); //pool_mgr.GetEmptyGO(out sr);
                CoreObjectType collapse_obj_type = GetCollapseObject( collapse_cmp.jem, collapse_cmp.collapse_index ) ;
                pool_mgr.GetOrCreateSprite( collapse_obj_type, out sprite );
                sr.sprite = sprite;
                collapse_cmp.tr = go.transform;
                collapse_cmp.sr = sr;
                SetParent(collapse_cmp.tr, cell_cmp.pos_x, cell_cmp.pos_y);
                ReleaseGo( collapse_cmp.jem_tr.gameObject ); //pool_mgr.ReleaseGO( collapse_cmp.jem_tr.gameObject ); //pool_mgr.ReleaseGO( jem_view_cmp.tr.gameObject );
                //claer pool's
                //jem_view_pool.Del( entity );
                init_jem_collapse_pool.Del( entity );
            }
            //collapse
            foreach(int entity in act_jem_collapse_filter)
            {
                ref JemCollapseComponent collapse_cmp = ref jem_collapse_pool.Get( entity );
                
                collapse_cmp.tm_wait_change -= dt_time;
                if(collapse_cmp.tm_wait_change >= 0) continue;

                collapse_cmp.collapse_index += 1;
                if(collapse_cmp.collapse_index < 5)
                {
                    CoreObjectType collapse_obj_type = GetCollapseObject( collapse_cmp.jem, collapse_cmp.collapse_index );
                    Sprite sprite;
                    pool_mgr.GetOrCreateSprite( collapse_obj_type, out sprite );
                    collapse_cmp.sr.sprite = sprite;  
                    collapse_cmp.tm_wait_change = 0.08f;  
                }
                else
                {
                    ReleaseGo( collapse_cmp.tr.gameObject ); //pool_mgr.ReleaseGO( collapse_cmp.tr.gameObject );
                    //clear pool
                    jem_collapse_pool.Del( entity );
                }
            }
        }

        void RunBonusCreate()
        {
            //init
            foreach( int entity in init_bonus_create_filter)
            {
                ref BonusCreateViewComponent bonus_cmp = ref bonus_create_pool.Get( entity );
                if( !bonus_cmp.init )
                {
                    //ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                    ref BonusViewComponent bonus_view_cmp = ref bonus_view_pool.Get( entity );
                    ref MatchBonusComponent bonus_match_cmp = ref bonus_pool.Get( entity );
                    //bonus_cmp.jem_tr = jem_view_cmp.tr;
                    bonus_cmp.bonus_tr = bonus_view_cmp.tr;
                    bonus_cmp.bonus_tr.localScale = Vector3.forward;
                    bonus_cmp.bonus_tr.gameObject.SetActive( false );
                    //jem_view_cmp.tr = null;
                    bonus_cmp.init = true;
                }

                bonus_cmp.tm_wait -=dt_time;
                if( bonus_cmp.tm_wait < 0 )
                {
                    bonus_cmp.bonus_tr.gameObject.SetActive( true );
                    //clear init
                    //jem_view_pool.Del( entity );
                    init_bonus_create_pool.Del( entity );
                }
            }
            //create view
            foreach( int entity in act_bonus_create_filter )
            {
                ref BonusCreateViewComponent bonus_cmp = ref bonus_create_pool.Get( entity );
                bonus_cmp.cur_tm_move += dt_time;
                float tm_k = bonus_cmp.cur_tm_move / bonus_cmp.tm_move;
                if(bonus_cmp.is_jem_scale)
                {
                    bonus_cmp.jem_tr.localScale = Vector3.LerpUnclamped( Vector3.one, new Vector3(0f, 0f, 1f), Easing.EaseInBack(tm_k) );
                    if(bonus_cmp.cur_tm_move > bonus_cmp.tm_move)
                    {
                        bonus_cmp.cur_tm_move = 0;
                        bonus_cmp.is_jem_scale = false;
                    }
                }
                else
                {
                    bonus_cmp.bonus_tr.localScale = Vector3.LerpUnclamped( new Vector3(0f, 0f, 1f), Vector3.one, Easing.EaseOutBack(tm_k) );
                    
                    if(bonus_cmp.cur_tm_move > bonus_cmp.tm_move)
                    {
                        bonus_cmp.bonus_tr.localScale = Vector3.one;
                        ReleaseGo( bonus_cmp.jem_tr.gameObject ); //pool_mgr.ReleaseGO( bonus_cmp.jem_tr.gameObject );    
                        //clear entity
                        bonus_create_pool.Del( entity );
                    }
                }
            }

        }

        void RunJemToBonusCollapse()
        {
            //init
            foreach(int entity in init_jem_to_bonus_collapse_filter)
            {
                ref JemToBonusCollapseComponent cmp = ref jem_to_bonus_collapse_pool.Get( entity );
                //ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                //cmp.tr = jem_view_cmp.tr;
                //jem_view_cmp.tr = null;
                //TODO: JemViewComponent оставляем - добавить проверку в RunViewJem --??
                //clear init
                //jem_view_pool.Del( entity );
                init_jem_to_bonus_collapse_pool.Del( entity );
            }
            //move
            foreach(int entity in act_jem_to_bonus_collapse_filter)
            {
                ref JemToBonusCollapseComponent cmp = ref jem_to_bonus_collapse_pool.Get( entity );
                cmp.tm_wait -= dt_time;
                if( cmp.tm_wait > 0f ) continue;

                cmp.cur_tm_move += dt_time;
                cmp.tr.localPosition = Vector3.LerpUnclamped(cmp.from, cmp.to, Easing.EaseInBack( cmp.cur_tm_move/cmp.tm_move) );
                if( cmp.cur_tm_move >= cmp.tm_move )
                {
                    //release go
                    ReleaseGo( cmp.tr.gameObject ); //pool_mgr.ReleaseGO( cmp.tr.gameObject );
                    //clear entity
                    jem_to_bonus_collapse_pool.Del( entity );
                }
            }
        }

        void RunUserJemMove()
        {
            foreach( int entity in jem_user_move_filter )
            {
                ref MatchCellUserMoveComponent move_cmp = ref cell_user_move_pool.Get( entity );
                ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                jem_view_cmp.tr.localPosition = move_cmp.cur_pos;
            }
        }

        void RunUserBonusMove()
        {
            foreach( int entity in bonus_user_move_filter )
            {
                ref MatchCellUserMoveComponent move_cmp = ref cell_user_move_pool.Get( entity );
                ref BonusViewComponent bonus_view_cmp = ref bonus_view_pool.Get( entity );
                bonus_view_cmp.tr.localPosition = move_cmp.cur_pos;
            }
        }

        void RunMatchJemMove()
        {
            foreach( int entity in jem_move_filter )
            {
                ref MatchCellMoveComponent cell_move_cmp = ref match_cell_move_pool.Get( entity );
                ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                jem_view_cmp.tr.localPosition = cell_move_cmp.cur_pos;
            }
        }

        void RunMatchJemCorrect()
        {
            foreach( int entity in jem_correct_filter )
            {
                ref MatchCellCorrectViewPos cell_correct_cmp = ref match_correct_move_pool.Get( entity );
                ref JemViewComponent jem_view_cmp = ref jem_view_pool.Get( entity );
                jem_view_cmp.tr.localPosition = cell_correct_cmp.cur_pos;
            }
        }

        void RunMatchBonusMove()
        {
            foreach( int entity in bonus_move_filter )
            {
                ref MatchCellMoveComponent cell_move_cmp = ref match_cell_move_pool.Get( entity );
                ref BonusViewComponent bonus_view_cmp = ref bonus_view_pool.Get( entity );
                bonus_view_cmp.tr.localPosition = cell_move_cmp.cur_pos;
            }
        }

        void RonMatchBonusCorrect()
        {
            foreach( int entity in bonus_correct_filter )
            {
                ref MatchCellCorrectViewPos cell_correct_cmp = ref match_correct_move_pool.Get( entity );
                ref BonusViewComponent bonus_view_cmp = ref bonus_view_pool.Get( entity );
                bonus_view_cmp.tr.localPosition = cell_correct_cmp.cur_pos;
            }
        }

        void RunClearCorrect()
        {
            foreach( int entity in correct_filter )
            {
                match_correct_move_pool.Del( entity );
            }
        }

        void SetParent(Transform tr, float pos_x, float pos_y)
        {
            tr.SetParent(match_tr, false);
            tr.localPosition = new Vector3( pos_x, pos_y, 0f );
        }

        GameObject GetPoolGo( CoreObjectType obj_type )
        {
            //create go
            GameObject go = pool_mgr.GetGO( obj_type );
            //create entity
            int entity = world.NewEntity();
            ref GameObjectComponent cmp = ref go_pool.Add( entity );
            cmp.go = go;
            return go;
        }

        GameObject GetEmtyGo( out SpriteRenderer sr )
        {
            //create go
            GameObject go = pool_mgr.GetEmptyGO(out sr);
            //create entity
            int entity = world.NewEntity();
            ref GameObjectComponent cmp = ref go_pool.Add( entity );
            cmp.go = go;
            return go;
        }

        void ReleaseGo( GameObject go )
        {
            //clear go entity
            foreach( int entity in go_filter )
            {
                ref GameObjectComponent cmp = ref go_pool.Get( entity );
                if( cmp.go == go )
                {
                    world.DelEntity( entity );
                }
            }
            //release go
            pool_mgr.ReleaseGO( go );
            
        }

        CoreObjectType GetJemObject( JemType jem )
        {
            switch(jem)
            {
                case JemType.jem_blue:
                    return CoreObjectType.jelly_blue;
                case JemType.jem_green:
                    return CoreObjectType.jelly_green;
                case JemType.jem_pink:
                    return CoreObjectType.jelly_pink;
                case JemType.jem_red:
                    return CoreObjectType.jelly_red;
                default:
                    return CoreObjectType.none;
            }
        }

        CoreObjectType GetBlockObject( BlockType block )
        {
            switch(block)
            {
                case BlockType.jelly:
                    return CoreObjectType.mm_brown;
                default:
                    return CoreObjectType.none;
            }
        }

        CoreObjectType GetCollapseObject( JemType jem, int index )
        {
            switch(jem)
            {
                case JemType.jem_blue:
                    switch(index)
                    {
                        case 0:
                            return CoreObjectType.explosionblue01;
                        case 1:
                            return CoreObjectType.explosionblue02;
                        case 2:
                            return CoreObjectType.explosionblue03;
                        case 4:
                            return CoreObjectType.explosionblue05;
                        default:
                            return CoreObjectType.explosionblue05;
                    }
                case JemType.jem_green:
                    switch(index)
                    {
                        case 0:
                            return CoreObjectType.explosiongreen01;
                        case 1:
                            return CoreObjectType.explosiongreen02;
                        case 2:
                            return CoreObjectType.explosiongreen03;
                        case 4:
                            return CoreObjectType.explosiongreen04;
                        default:
                            return CoreObjectType.explosiongreen05;
                    }
                case JemType.jem_pink:
                    switch(index)
                    {
                        case 0:
                            return CoreObjectType.explosionpink01;
                        case 1:
                            return CoreObjectType.explosionpink02;
                        case 2:
                            return CoreObjectType.explosionpink03;
                        case 4:
                            return CoreObjectType.explosionpink04;
                        default:
                            return CoreObjectType.explosionpink05;
                    }
                case JemType.jem_red:
                    switch(index)
                    {
                        case 0:
                            return CoreObjectType.explosionred01;
                        case 1:
                            return CoreObjectType.explosionred02;
                        case 2:
                            return CoreObjectType.explosionred03;
                        case 4:
                            return CoreObjectType.explosionred04;
                        default:
                            return CoreObjectType.explosionred05;
                    }
                default:
                    return CoreObjectType.none;
            }
        }

        CoreObjectType GetBonusObject(BonusType bonus, JemType jem)
        {
            switch( bonus )
            {
                case BonusType.vertical:
                case BonusType.horizontal:
                    switch( jem )
                    {
                        case JemType.jem_blue:
                            return CoreObjectType.wrappedsolid_blue;
                        case JemType.jem_green:
                            return CoreObjectType.wrappedsolid_green;
                        case JemType.jem_pink:
                            return CoreObjectType.wrappedsolid_pink;
                        case JemType.jem_red:
                            return CoreObjectType.wrappedsolid_red;
                        default:
                            return CoreObjectType.wrappedsolid_red;
                    }
                case BonusType.cross:
                case BonusType.corner:
                    switch( jem )
                    {
                        case JemType.jem_blue:
                            return CoreObjectType.swirl_blue;
                        case JemType.jem_green:
                            return CoreObjectType.swirl_green;
                        case JemType.jem_pink:
                            return CoreObjectType.swirl_pink;
                        case JemType.jem_red:
                            return CoreObjectType.swirl_red;
                        default:
                            return CoreObjectType.swirl_red;
                    }
                default:
                    return CoreObjectType.wrappedsolid_blue;
            }
        }

        Vector3 GetBonusAngle(BonusType bonus)
        {
            switch( bonus )
            {
                case BonusType.vertical:
                    return new Vector3(0f, 0f, 90f);
                default:
                    return Vector3.zero;
            }
        }
    }
}