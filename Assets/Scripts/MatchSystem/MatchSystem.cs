using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Leopotam.EcsLite;
using Pool;

namespace BadCode.Main
{
    public class MatchSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;

        //mgr
        InputMgr input_mgr;
        DtMgr dt_mgr;
        //pool's
        EcsPool<WaitComponent> wait_pool;
        EcsPool<ActComponent> act_pool;
        //match-pool
        EcsPool<MatchInitGameComponent> init_pool;
        EcsPool<MatchCellComponent> cell_pool;
        EcsPool<MatchJemComponent> jem_pool;
        EcsPool<MatchHoleComponent> hole_pool;
        EcsPool<MatchBornJemComponent> born_jem_pool;
        EcsPool<MatchBonusComponent> bonus_pool;
        EcsPool<MatchBlockComponent> block_pool;
        EcsPool<BlockCorrectMoveComponent> block_correct_pool;
        EcsPool<MatchCellMoveComponent> move_pool;
        EcsPool<MatchCellCorrectViewPos> correct_move_pool;
        EcsPool<MatchCellUserMoveComponent> move_user_pool;
        EcsPool<MatchWaitCellComponent> wait_cell_pool;
        EcsPool<MatchCheckCollapseComponent> check_collapse_pool;
        //view-pool
        EcsPool<InitJemViewComponent> init_jem_view_pool;
        EcsPool<InitBonusViewComponent> init_bonus_view_pool;
        EcsPool<JemViewComponent> jem_view_pool;
        EcsPool<BonusViewComponent> bonus_view_pool;
        EcsPool<InitBlockViewComponent> init_block_view_pool;
        EcsPool<BlockViewComponent> block_view_pool;
        EcsPool<JemUserMoveComponent> jem_move_user_pool;
        EcsPool<InitJemCollapseComponent> init_jem_collapse_pool;
        EcsPool<JemCollapseComponent> jem_collapse_pool;
        EcsPool<InitBonusCreateViewComponent> init_bonus_create_pool;
        EcsPool<BonusCreateViewComponent> bonus_create_pool;
        EcsPool<InitJemToBonusCollapseComponent> init_jem_to_bonus_collapse_pool;
        EcsPool<JemToBonusCollapseComponent> jem_to_bonus_collapse_pool;

        //drag-pool
        EcsPool<DragComponent> drag_pool;
        //filters
        EcsFilter wait_init_filter;
        EcsFilter act_init_filter;
        EcsFilter drag_filter;
        EcsFilter user_move_filter;
        EcsFilter move_filter;
        EcsFilter wait_cell_filter;
        EcsFilter born_jem_filter;
        EcsFilter check_collapse_filter;

        GameConfig config;
        int[,] cells_arr;
        JemType[] jem_types;
        QuickMatchInfo match_info;
        //sPoolContainer<CellInfo> cell_info_pool;
        uint last_touch_id;

        float dt_time;
        
        public void Init(EcsSystems systems) 
        {
            world = systems.GetWorld();

            //manager's
            input_mgr = systems.GetShared<WorldMgr>().GetMgr<InputMgr>();
            dt_mgr = systems.GetShared<WorldMgr>().GetMgr<DtMgr>();
            //pool
            wait_pool = world.GetPool<WaitComponent>();
            act_pool = world.GetPool<ActComponent>();
            init_pool = world.GetPool<MatchInitGameComponent>();
            cell_pool = world.GetPool<MatchCellComponent>();
            jem_pool = world.GetPool<MatchJemComponent>();
            hole_pool = world.GetPool<MatchHoleComponent>();
            born_jem_pool = world.GetPool<MatchBornJemComponent>();
            bonus_pool = world.GetPool<MatchBonusComponent>();
            block_pool = world.GetPool<MatchBlockComponent>();
            block_correct_pool = world.GetPool<BlockCorrectMoveComponent>();
            move_pool = world.GetPool<MatchCellMoveComponent>();
            correct_move_pool = world.GetPool<MatchCellCorrectViewPos>();
            move_user_pool = world.GetPool<MatchCellUserMoveComponent>();
            jem_move_user_pool = world.GetPool<JemUserMoveComponent>();
            wait_cell_pool = world.GetPool<MatchWaitCellComponent>();
            check_collapse_pool = world.GetPool<MatchCheckCollapseComponent>();
            //view-pool
            init_jem_view_pool = world.GetPool<InitJemViewComponent>();
            init_bonus_view_pool = world.GetPool<InitBonusViewComponent>();
            jem_view_pool = world.GetPool<JemViewComponent>();
            bonus_view_pool = world.GetPool<BonusViewComponent>();
            block_view_pool = world.GetPool<BlockViewComponent>();
            init_block_view_pool = world.GetPool<InitBlockViewComponent>();
            init_jem_collapse_pool = world.GetPool<InitJemCollapseComponent>();
            jem_collapse_pool = world.GetPool<JemCollapseComponent>();
            init_bonus_create_pool = world.GetPool<InitBonusCreateViewComponent>();
            bonus_create_pool = world.GetPool<BonusCreateViewComponent>();
            init_jem_to_bonus_collapse_pool = world.GetPool<InitJemToBonusCollapseComponent>();
            jem_to_bonus_collapse_pool = world.GetPool<JemToBonusCollapseComponent>();
            //input
            drag_pool = world.GetPool<DragComponent>(); 
            //filter
            wait_init_filter = world.Filter<MatchInitGameComponent>().Inc<WaitComponent>().End();
            act_init_filter = world.Filter<MatchInitGameComponent>().Inc<ActComponent>().End();
            drag_filter = world.Filter<DragComponent>().End();
            user_move_filter = world.Filter<MatchCellUserMoveComponent>().End();
            move_filter = world.Filter<MatchCellMoveComponent>().End();
            wait_cell_filter = world.Filter<MatchWaitCellComponent>().End();
            born_jem_filter = world.Filter<MatchBornJemComponent>().Inc<MatchCellComponent>().End();
            check_collapse_filter =  world.Filter<MatchCheckCollapseComponent>().End();
            //match
            jem_types = new JemType[]{
                JemType.jem_blue,
                JemType.jem_green,
                JemType.jem_pink,
                JemType.jem_red
            };
            //cell_info_pool = new sPoolContainer<CellInfo>();
            match_info = new QuickMatchInfo();
        }

        public void Run(EcsSystems systems) 
        {
            dt_time = dt_mgr.GetDt();

            RunInit();
            
            if(act_init_filter.GetEntitiesCount() == 0) return;

            RunWaitCell();
            RunInput();
            
            RunUserMove();
            RunFieldStartLoop();
            RunBornJem();
            RunFieldMoveLoop();
            RunCheckCollapse();
        }

        void RunInit()
        {
            if(act_init_filter.GetEntitiesCount() == 0)
            {
                //init game
                foreach(int entity in wait_init_filter)
                {
                    ref MatchInitGameComponent init = ref init_pool.Get(entity);
                    config = init.config;
                    OnInit();
                    
                    wait_pool.Del(entity);
                    act_pool.Add(entity);
                    break;
                }
            }
            //clear
            foreach(int entity in wait_init_filter)
            {
                world.DelEntity(entity);
            }
        }

        void RunInput()
        {
            foreach(int entity in drag_filter)
            {
                ref DragComponent drag_cmp = ref drag_pool.Get(entity);
                if(last_touch_id == drag_cmp.touch_id) continue;

                float dist = Vector2.Distance(drag_cmp.start_pos, drag_cmp.last_pos);
                //UnityEngine.Debug.LogFormat("drag problem: drag dist {0}, cmp touch={1}, last touch={2}", dist, drag_cmp.touch_id, last_touch_id);
                if(dist < 5) continue;

                last_touch_id = drag_cmp.touch_id;
                
                Vector2 world_pos = input_mgr.GetWorlPos(drag_cmp.start_pos);
                int cell_x;
                int cell_y;
                int cell_entity = GetCellEntity( world_pos, out cell_x, out cell_y);
                //UnityEngine.Debug.LogFormat("drag problem: from {0}", cell_entity);
                if(cell_entity < 0) continue;
                if( !IsInputCell( cell_entity) ) continue;

                Vector2 dir = drag_cmp.delta_pos.normalized;
                //UnityEngine.Debug.LogFormat("drag problem: dir {0}", dir);
                int next_cell_entity = -1;
                if( Mathf.Abs( dir.x ) > Mathf.Abs( dir.y ) )
                {
                    if(dir.x > 0)
                    {
                        next_cell_entity = GetCellEntity(cell_x + 1, cell_y);
                    }
                    else
                    {
                        next_cell_entity = GetCellEntity(cell_x - 1, cell_y);       
                    }
                }
                else
                {
                    if(dir.y > 0)
                    {
                        next_cell_entity = GetCellEntity(cell_x, cell_y - 1);       
                    }
                    else
                    {
                        next_cell_entity = GetCellEntity(cell_x, cell_y + 1);       
                    }
                }
                //UnityEngine.Debug.LogFormat("drag problem: from {0} to {1}", cell_entity, next_cell_entity);
                if(next_cell_entity < 0) continue;
                if( !IsInputCell( next_cell_entity) ) continue;

                ref MatchCellComponent cell_cmp = ref cell_pool.Get( cell_entity );
                ref MatchCellComponent next_cmp = ref cell_pool.Get( next_cell_entity );
                //change jem's
                ref MatchJemComponent cell_jem_cmp = ref jem_pool.Get( cell_entity );
                ref MatchJemComponent next_cell_jem_cmp = ref jem_pool.Get( next_cell_entity );
                JemType cell_jem = cell_jem_cmp.jem;
                JemType next_cell_jem = next_cell_jem_cmp.jem;
                cell_jem_cmp.jem = next_cell_jem;
                next_cell_jem_cmp.jem = cell_jem;
                //change view
                ref JemViewComponent cell_jem_view_cmp = ref jem_view_pool.Get( cell_entity );
                ref JemViewComponent next_jem_view_cmp = ref jem_view_pool.Get( next_cell_entity );
                Transform cell_tr = cell_jem_view_cmp.tr;
                Transform next_tr = next_jem_view_cmp.tr;
                cell_jem_view_cmp.tr = next_tr;
                next_jem_view_cmp.tr = cell_tr;
                //data move
                ref MatchCellUserMoveComponent user_move_cmp = ref move_user_pool.Add( cell_entity );
                user_move_cmp.home_x = cell_x;
                user_move_cmp.home_y = cell_y;
                // user_move_cmp.from = new Vector2(cell_cmp.pos_x, cell_cmp.pos_y);
                // user_move_cmp.cur_pos = user_move_cmp.from;
                // user_move_cmp.to = new Vector2(next_cmp.pos_x, next_cmp.pos_y);

                user_move_cmp.from = new Vector2(next_cmp.pos_x, next_cmp.pos_y);
                user_move_cmp.cur_pos = user_move_cmp.from;
                user_move_cmp.to = new Vector2(cell_cmp.pos_x, cell_cmp.pos_y);

                user_move_cmp.is_user_active = true;
                user_move_cmp.to_entity = next_cell_entity;

                ref MatchCellUserMoveComponent next_move_cmp = ref move_user_pool.Add( next_cell_entity );
                next_move_cmp.home_x = next_cmp.x;
                next_move_cmp.home_y = next_cmp.y;
                // next_move_cmp.from = new Vector2(next_cmp.pos_x, next_cmp.pos_y);
                // next_move_cmp.cur_pos = next_move_cmp.from;
                // next_move_cmp.to = new Vector2(cell_cmp.pos_x, cell_cmp.pos_y);

                next_move_cmp.from = new Vector2(cell_cmp.pos_x, cell_cmp.pos_y);
                next_move_cmp.cur_pos = next_move_cmp.from;
                next_move_cmp.to = new Vector2(next_cmp.pos_x, next_cmp.pos_y);

                next_move_cmp.is_user_active = false;
                next_move_cmp.to_entity = cell_entity;
                
                // //view move
                // if( jem_move_user_pool.Has( cell_entity ) )
                // {
                //     ref JemUserMoveComponent jem_move_user_cmp = ref jem_move_user_pool.Get( cell_entity );
                //     jem_move_user_cmp.user_move_speed = config.user_move_speed;
                //     jem_move_user_cmp.from = user_move_cmp.from;
                //     jem_move_user_cmp.to = user_move_cmp.to;
                //     jem_move_user_cmp.is_user_active = true;
                //     jem_move_user_cmp.tm_move = 0f;
                //     jem_move_user_cmp.to_entity = next_cell_entity;
                //     if( !init_move_user_pool.Has(cell_entity) )
                //         init_move_user_pool.Add( cell_entity );
                // }
                // else
                // {
                //     ref JemUserMoveComponent jem_move_user_cmp = ref jem_move_user_pool.Add( cell_entity );
                //     jem_move_user_cmp.user_move_speed = config.user_move_speed;
                //     jem_move_user_cmp.from = user_move_cmp.from;
                //     jem_move_user_cmp.to = user_move_cmp.to;
                //     jem_move_user_cmp.is_user_active = true;
                //     jem_move_user_cmp.tm_move = 0f;
                //     jem_move_user_cmp.to_entity = next_cell_entity;
                //     init_move_user_pool.Add( cell_entity );
                // }

                // if( jem_move_user_pool.Has( next_cell_entity ) )
                // {
                //     ref JemUserMoveComponent jem_next_move_user_cmp = ref jem_move_user_pool.Get( next_cell_entity );
                //     jem_next_move_user_cmp.user_move_speed = config.user_move_speed;
                //     jem_next_move_user_cmp.from = next_move_cmp.from;
                //     jem_next_move_user_cmp.to = next_move_cmp.to;
                //     jem_next_move_user_cmp.is_user_active = false;
                //     jem_next_move_user_cmp.tm_move = 0f;
                //     jem_next_move_user_cmp.to_entity = cell_entity;
                //     if( !init_move_user_pool.Has(next_cell_entity) )
                //         init_move_user_pool.Add( next_cell_entity );
                // }
                // else
                // {
                //     ref JemUserMoveComponent jem_next_move_user_cmp = ref jem_move_user_pool.Add( next_cell_entity );
                //     jem_next_move_user_cmp.user_move_speed = config.user_move_speed;
                //     jem_next_move_user_cmp.from = next_move_cmp.from;
                //     jem_next_move_user_cmp.to = next_move_cmp.to;
                //     jem_next_move_user_cmp.is_user_active = false;
                //     jem_next_move_user_cmp.tm_move = 0f;
                //     jem_next_move_user_cmp.to_entity = cell_entity;
                //     init_move_user_pool.Add( next_cell_entity );
                // }

            }
        }

        void RunUserMove()
        {
            bool is_match_exists = false;
            bool is_distance = false;
            //move
            foreach( int entity in user_move_filter)
            {
                ref MatchCellUserMoveComponent cmp = ref move_user_pool.Get( entity );
                cmp.tm_move += dt_time;
                cmp.cur_pos = Vector2.MoveTowards(cmp.from, cmp.to, config.user_move_speed * cmp.tm_move );
                if( Vector2.Distance(cmp.cur_pos, cmp.to) < config.min_dist)
                {
                    is_distance = true;
                    //check match
                    JemType jem;
                    if( !GetEntityJem(cmp.to_entity, out jem) ) //if( !GetEntityJem(entity, out jem) ) 
                    {
                        Debug.LogFormat("callapse problem: can't find jem");
                        continue;
                    }

                    GetMatch( cmp.to_entity, jem );
                    if( match_info.IsAnyMatch() )
                    {
                        is_match_exists = true;
                        //collapse
                        StartCollapse( match_info );
                        // //TODO: add collapse
                        // is_match_exists = true;
                        // sPool<MatchCellInfo> cell_info_arr = match_info.GetMatchArr();
                        // ref MatchCellInfo init_cell = ref match_info.GetInitCellRef();
                        // BonusType bonus;
                        // if( match_info.GetBonusType(out bonus) )
                        // {
                        //     //create bonus component
                        //     ref MatchBonusComponent bonus_cmp = ref bonus_pool.Add( cmp.to_entity );
                        //     bonus_cmp.bonus = bonus;
                        //     bonus_cmp.jem = jem;
                        //     //block init cell
                        //     AddWaitCellComponent( cmp.to_entity, config.tm_bonus_create );
                        //     //create bonus view
                        //     ref BonusViewComponent view_cmp = ref bonus_view_pool.Add( cmp.to_entity );
                        //     init_bonus_view_pool.Add( cmp.to_entity );
                        //     ref InitBonusCreateViewComponent init_bonus_create_cmp = ref init_bonus_create_pool.Add( cmp.to_entity );
                        //     ref BonusCreateViewComponent bonus_create_cmp = ref bonus_create_pool.Add( cmp.to_entity );
                        //     bonus_create_cmp.tm_move = config.tm_bonus_create * 0.3f / 2f;
                        //     bonus_create_cmp.is_jem_scale = true;
                        //     bonus_create_cmp.tm_wait = config.tm_bonus_create * 0.7f;
                        //     Vector2 init_pos = GetCellPos( cmp.to_entity );
                        //     //remove init jem
                        //     jem_pool.Del( cmp.to_entity );

                        //     //collapse jem's array
                        //     for(int i=0; i < cell_info_arr.Count; i++)
                        //     {
                        //         ref MatchCellInfo info = ref cell_info_arr.GetByRef( i );
                                
                        //         if(info.x == init_cell.x && info.y == init_cell.y) continue;

                        //         int collapse_entity = GetCellEntity( info.x, info.y );
                        //         //create jem move
                        //         init_jem_to_bonus_collapse_pool.Add( collapse_entity );
                        //         ref JemToBonusCollapseComponent jem_to_bonus_cmp = ref  jem_to_bonus_collapse_pool.Add( collapse_entity );
                        //         jem_to_bonus_cmp.bonus = bonus;
                        //         jem_to_bonus_cmp.jem = jem;
                        //         jem_to_bonus_cmp.tm_move = bonus_create_cmp.tm_wait;
                        //         jem_to_bonus_cmp.from = GetCellPos( collapse_entity );
                        //         jem_to_bonus_cmp.to = init_pos;
                        //         //wait jem
                        //         AddWaitCellComponent( collapse_entity, bonus_create_cmp.tm_wait );
                        //         //remove jem
                        //         jem_pool.Del( collapse_entity );
                        //     }
                        // }
                        // else
                        // {
                        //     for(int i=0; i < cell_info_arr.Count; i++)
                        //     {
                        //         ref MatchCellInfo info = ref cell_info_arr.GetByRef( i );
                        //         int collapse_entity = GetCellEntity( info.x, info.y );
                        //         AddWaitCellComponent( collapse_entity, config.tm_wait_collapse );
                        //         //add view collapse
                        //         if( !jem_collapse_pool.Has( collapse_entity ) )
                        //         {
                        //             ref JemCollapseComponent collapse_cmp = ref jem_collapse_pool.Add( collapse_entity );
                        //             collapse_cmp.tm_wait_change = 0.08f;
                        //             collapse_cmp.jem = jem;
                        //         }

                        //         if( !init_jem_collapse_pool.Has( collapse_entity ) )
                        //         {
                        //             init_jem_collapse_pool.Add( collapse_entity );
                        //             Debug.LogFormat("collapse problem: add collapse to x={0}, y={1}", info.x, info.y);
                        //         }
                        //         //remove jem
                        //         jem_pool.Del( collapse_entity );
                        //     }
                        // }
                    }
                }
            }

            //end move
            if(is_match_exists || is_distance)
            {
                foreach( int entity in user_move_filter)
                {
                    ref MatchCellUserMoveComponent cmp = ref move_user_pool.Get( entity );
                    if(is_match_exists)
                    {
                        // //change view
                        // if( cmp.is_user_active )
                        // {
                        //     ref JemViewComponent cell_jem_view_cmp = ref jem_view_pool.Get( entity );
                        //     ref JemViewComponent next_jem_view_cmp = ref jem_view_pool.Get( cmp.to_entity );
                        //     Transform cell_tr = cell_jem_view_cmp.tr;
                        //     Transform next_tr = next_jem_view_cmp.tr;
                        //     cell_jem_view_cmp.tr = next_tr;
                        //     next_jem_view_cmp.tr = cell_tr;
                        // }
                        //clear pool
                        move_user_pool.Del( entity );
                        jem_move_user_pool.Del( entity );
                        //init_move_user_pool.Del( entity );
                    }
                    else
                    {
                        //clear
                        if( cmp.is_back_move )
                        {
                            move_user_pool.Del( entity );
                        }
                        else
                        {
                            // Vector2 old_to = cmp.to;
                            // Vector2 old_from = cmp.from;
                            // cmp.to = old_from;
                            // cmp.from = old_to;
                            cmp.cur_pos = cmp.from;
                            cmp.tm_move = 0f;
                            cmp.is_back_move = true;
                            //view move
                            // if( jem_move_user_pool.Has( cmp.to_entity ) )
                            // {
                            //     ref JemUserMoveComponent jem_next_move_user_cmp = ref jem_move_user_pool.Get( cmp.to_entity );
                            //     jem_next_move_user_cmp.from = cmp.from;
                            //     jem_next_move_user_cmp.to = cmp.to;
                            //     jem_next_move_user_cmp.is_user_active = cmp.is_user_active;
                            //     jem_next_move_user_cmp.tm_move = 0f;
                            //     jem_next_move_user_cmp.to_entity = entity;
                            //     // if( !init_move_user_pool.Has(entity) )
                            //     //     init_move_user_pool.Add( entity );
                            // }
                            // else
                            // {
                            //     ref JemUserMoveComponent jem_next_move_user_cmp = ref jem_move_user_pool.Add( cmp.to_entity );
                            //     jem_next_move_user_cmp.from = cmp.from;
                            //     jem_next_move_user_cmp.to = cmp.to;
                            //     jem_next_move_user_cmp.is_user_active = cmp.is_user_active;
                            //     jem_next_move_user_cmp.tm_move = 0f;
                            //     jem_next_move_user_cmp.to_entity = entity;
                            //     // if( !init_move_user_pool.Has(entity) )
                            //     //     init_move_user_pool.Add( entity );
                            // }
                            //change jem's back
                            if( cmp.is_user_active )
                            {
                                //change gem
                                ref MatchJemComponent cell_jem_cmp = ref jem_pool.Get( entity );
                                ref MatchJemComponent next_cell_jem_cmp = ref jem_pool.Get( cmp.to_entity );
                                JemType cell_jem = cell_jem_cmp.jem;
                                JemType next_cell_jem = next_cell_jem_cmp.jem;
                                cell_jem_cmp.jem = next_cell_jem;
                                next_cell_jem_cmp.jem = cell_jem;
                                //change view
                                ref JemViewComponent cell_jem_view_cmp = ref jem_view_pool.Get( entity  );
                                ref JemViewComponent next_jem_view_cmp = ref jem_view_pool.Get( cmp.to_entity );
                                Transform cell_tr = cell_jem_view_cmp.tr;
                                Transform next_tr = next_jem_view_cmp.tr;
                                cell_jem_view_cmp.tr = next_tr;
                                next_jem_view_cmp.tr = cell_tr;
                            }
                        }
                    }
                }
            }
        }

        void RunFieldMoveLoop()
        {
            for( int y=config.size_y - 1; y >= 0; y-- )
            {
                for( int x=0; x<config.size_x; x++ )
                {
                    int entity = cells_arr[x, y];
                    RunCellLoopMove( x, y, entity );
                }
            }
        }

        void RunFieldStartLoop()
        {
            for(int y=config.size_y - 1; y >= 0; y--)
            {
                for(int x=0; x<config.size_x; x++)
                {
                    int entity = cells_arr[x, y];
                    
                    bool start_jem = RunJemMoveDown( x, y, entity );
                    if( !start_jem )
                    {
                        RunBlockCorrectMove( x, y, entity );
                    }
                }
            }
        }

        void RunCellLoopMove( int x, int y, int entity )
        {
            if( !move_pool.Has( entity ) ) return;

            ref MatchCellMoveComponent cmp = ref move_pool.Get( entity );
            cmp.tm_move += dt_time;
            cmp.cur_pos = Vector2.MoveTowards(cmp.from, cmp.to, cmp.speed * cmp.tm_move );
            if( Vector2.Distance(cmp.cur_pos, cmp.to) < config.min_dist)
            {
                //добавить компонет для окончательной позиции view
                int down_entity = GetCellEntity( x, y + 1 );
                if( down_entity > -1 && IsReadyForMoveDown( down_entity ) && IsReadyForView( down_entity) )
                {
                    CompleteMoveJem( in cmp, entity, down_entity );
                    CompleteMoveBonus( in cmp, entity, down_entity );
                }
                else
                {
                    if( !correct_move_pool.Has( entity ) )
                    {
                        ref MatchCellCorrectViewPos correct_cmp = ref correct_move_pool.Add( entity );
                        correct_cmp.cur_pos = cmp.to;
                    }
                    else
                    {
                        ref MatchCellCorrectViewPos correct_cmp = ref correct_move_pool.Get( entity );
                        correct_cmp.cur_pos = cmp.to;
                    }
                    //add check collapse
                    if( jem_pool.Has( entity ) )
                    {
                        ref MatchJemComponent jem_cmp = ref jem_pool.Get( entity );
                        GetMatchIncMove( entity, jem_cmp.jem ); //GetMatch( entity, jem_cmp.jem );
                        if( match_info.IsAnyMatch() )
                        {
                            if( !check_collapse_pool.Has( entity ) )
                            {
                                check_collapse_pool.Add( entity );
                            }
                        }
                    }
                    
                }
                move_pool.Del( entity );
            }
        }

        void RunCheckCollapse()
        {
            foreach( int entity in check_collapse_filter )
            {
                if( !jem_pool.Has( entity ) ) continue;

                ref MatchJemComponent jem_cmp = ref jem_pool.Get( entity );
                GetMatch( entity, jem_cmp.jem );
                if( match_info.IsAnyMatch() )
                {
                    StartCollapse( match_info );
                }
                check_collapse_pool.Del( entity );
            }
        }

        void RunBlockCorrectMove( int x, int y, int entity )
        {
            if( !block_correct_pool.Has( entity ) ) return;
            if( !IsReadyForMoveDown( entity ) ) return;
            if( !IsReadyForView( entity) ) return;
            int down_entity = GetCellEntity( x, y + 1);
            if( down_entity < 0 ) return;
            if( !IsReadyForMoveDown( down_entity ) ) return;
            if( !IsReadyForView( down_entity) ) return;

            ref BlockCorrectMoveComponent correct_cmp = ref block_correct_pool.Get( entity );
            int r_x = x + 1;
            int l_x = x - 1;
            //right fist
            if( correct_cmp.last_x != r_x || correct_cmp.last_x == 0)
            {
                bool is_start_r = StrtBlockCorrectMove( r_x, y, down_entity);
                bool is_start_l = false;
                if( !is_start_r )
                {
                    is_start_l = StrtBlockCorrectMove( l_x, y, down_entity);//StrtBlockCorrectMove( r_x, y, down_entity);
                }

                if( is_start_r || is_start_l )
                {
                    correct_cmp.last_x = r_x;
                }
            }
            else
            {
                bool is_start_l = StrtBlockCorrectMove( l_x, y, down_entity);
                bool is_start_r = false;
                if( !is_start_l )
                {
                    is_start_r = StrtBlockCorrectMove( r_x, y, down_entity);
                }

                if( is_start_r || is_start_l )
                {
                    correct_cmp.last_x = l_x;
                }
            }
        }

        bool StrtBlockCorrectMove( int x, int y, int down_entity)
        {
            int entity = GetCellEntity(x , y);
            if( entity < 0 ) return false;
            //check self correct
            //if( block_correct_pool.Has( entity ) ) return false;
            if( !IsReadyForStartDown( entity ) ) return false;
            if( !IsReadyForStartDown( down_entity ) ) return false;

            bool is_jem = StartMoveJem( entity, down_entity ); //StartMoveBlockCorrectJem( entity, down_entity);
            bool is_bonus = false;
            if( !is_jem )
            {
                is_bonus = StartMoveBonus( entity, down_entity );
            }

            return is_jem || is_bonus;
        }

        void CompleteMoveJem( in MatchCellMoveComponent from_cmp, int from_entity, int to_entity )
        {
            if( !jem_pool.Has( from_entity ) ) return; //is jem?

            ref MatchJemComponent from_jem_cmp = ref jem_pool.Get( from_entity );
            ref MatchJemComponent to_jem_cmp = ref jem_pool.Add( to_entity );
            to_jem_cmp.jem = from_jem_cmp.jem;
            ref MatchCellMoveComponent move_cmp = ref move_pool.Add( to_entity );
            //move_cmp.from = from_cmp.from;//GetCellPos( entity );
            move_cmp.to = GetCellPos( to_entity );
            move_cmp.from_entity = from_entity;
            move_cmp.cur_pos = GetCellPos( from_entity );
            move_cmp.from = new Vector2( move_cmp.cur_pos.x, from_cmp.from.y );
            if( Mathf.Approximately(from_cmp.from.x, move_cmp.cur_pos.x) )
            {
                move_cmp.tm_move = from_cmp.tm_move;
                move_cmp.from = from_cmp.from;
            }
            else
            {
                move_cmp.from = move_cmp.cur_pos;
                move_cmp.tm_move = 0;//from_cmp.tm_move / 2f;
            }
            move_cmp.speed = config.move_speed; //cmp.speed; //cmp.speed * cmp.tm_move;
            //Debug.LogFormat("complete move from={0}, to={1}", move_cmp.from, move_cmp.to);
            jem_pool.Del( from_entity );
            //move view
            ref JemViewComponent from_jem_view_cmp = ref jem_view_pool.Get( from_entity );
            if( jem_view_pool.Has( to_entity ) )
            {
                ref JemViewComponent to_jem_view_cmp = ref jem_view_pool.Get( to_entity );
                if( to_jem_view_cmp.tr != null )
                {
                    int p_x;
                    int p_y;
                    GetEntityXY(to_entity, out p_x, out p_y );
                    Debug.LogErrorFormat("jem_view problem: x={0}, y={1}", p_x, p_y);
                }

                // if( init_jem_view_pool.Has( from_entity ) )
                // {
                //     init_jem_view_pool.Add( to_entity );
                //     init_jem_view_pool.Del( from_entity );
                // }
                // else
                // {
                //     to_jem_view_cmp.tr = from_jem_view_cmp.tr;
                // }
            }   
            else
            {
                ref JemViewComponent to_jem_view_cmp = ref jem_view_pool.Add( to_entity );
                if( init_jem_view_pool.Has( from_entity ) )
                {
                    init_jem_view_pool.Add( to_entity );
                    init_jem_view_pool.Del( from_entity );
                }
                else
                {
                    to_jem_view_cmp.tr = from_jem_view_cmp.tr;
                }
            }
            jem_view_pool.Del( from_entity );
        }

        void CompleteMoveBonus( in MatchCellMoveComponent from_cmp, int from_entity, int to_entity )
        {
            if( !bonus_pool.Has( from_entity ) ) return;

            ref MatchBonusComponent from_bonus_cmp = ref bonus_pool.Get( from_entity );
            ref MatchBonusComponent to_bonus_cmp = ref bonus_pool.Add( to_entity );
            to_bonus_cmp.bonus = from_bonus_cmp.bonus;
            ref MatchCellMoveComponent move_cmp = ref move_pool.Add( to_entity );
            move_cmp.tm_move = from_cmp.tm_move;
            //move_cmp.from = cmp.from;//GetCellPos( entity );
            move_cmp.to = GetCellPos( to_entity );
            move_cmp.from_entity = from_entity;
            move_cmp.cur_pos = GetCellPos( from_entity );
            if( Mathf.Approximately(from_cmp.from.x, move_cmp.cur_pos.x) )
            {
                move_cmp.tm_move = from_cmp.tm_move;
                move_cmp.from = from_cmp.from;
            }
            else
            {
                move_cmp.from = move_cmp.cur_pos;
                move_cmp.tm_move = 0;//from_cmp.tm_move / 2f;
            }
            move_cmp.speed = config.move_speed;//cmp.speed; //cmp.speed * cmp.tm_move;
            bonus_pool.Del( from_entity );
            Debug.LogFormat("complete move: bonus from={0}, to={1}", move_cmp.from, move_cmp.to);
            //move view
            ref BonusViewComponent from_bonus_view_cmp = ref bonus_view_pool.Get( from_entity );
            ref BonusViewComponent to_bonus_view_cmp = ref bonus_view_pool.Add( to_entity );
            if( init_bonus_view_pool.Has( from_entity ) )
            {
                init_bonus_view_pool.Add( to_entity );
                init_bonus_view_pool.Del( from_entity );
            }
            else
            {
                to_bonus_view_cmp.tr = from_bonus_view_cmp.tr;
            }
            bonus_view_pool.Del( from_entity );
        }

        bool RunJemMoveDown( int x, int y, int entity )
        {
            if( !IsReadyForStartDown( entity ) ) return false;
            int down_entity = GetCellEntity( x, y + 1 );
            if( !(down_entity > -1 && IsReadyForMoveDown( down_entity ) && IsReadyForView( down_entity)) ) return false;
            
            bool start_jem = StartMoveJem( entity, down_entity );
            bool start_bonus = false;
            if( !start_jem )
            {
                start_bonus = StartMoveBonus( entity, down_entity );
            }
            return start_bonus || start_jem;
        }

        bool StartMoveJem( int from_entity, int to_entity )
        {
            if( !jem_pool.Has( from_entity ) ) return false; //is jem?

            ref MatchJemComponent from_jem_cmp = ref jem_pool.Get( from_entity );
            ref MatchJemComponent to_jem_cmp = ref jem_pool.Add( to_entity );
            to_jem_cmp.jem = from_jem_cmp.jem;
            jem_pool.Del( from_entity );
            ref MatchCellMoveComponent move_cmp = ref move_pool.Add( to_entity );
            move_cmp.from = GetCellPos( from_entity );
            move_cmp.to = GetCellPos( to_entity );
            move_cmp.cur_pos = GetCellPos( from_entity );
            move_cmp.speed = config.move_speed;
            move_cmp.from_entity = from_entity;
            //Debug.LogFormat("create move from={0}, to={1}", move_cmp.from, move_cmp.to);
            //move view
            ref JemViewComponent from_jem_view_cmp = ref jem_view_pool.Get( from_entity );
            ref JemViewComponent to_jem_view_cmp = ref jem_view_pool.Add( to_entity );
            if( init_jem_view_pool.Has( from_entity ) )
            {
                init_jem_view_pool.Add( to_entity );
                init_jem_view_pool.Del( from_entity );
            }
            else
            {
                to_jem_view_cmp.tr = from_jem_view_cmp.tr;
            }
            jem_view_pool.Del( from_entity );
            return true;
        }

        bool StartMoveBonus( int from_entity, int to_entity )
        {
            if( !bonus_pool.Has( from_entity ) ) return false; //is bonus?

            //copy bonus math cmp
            ref MatchBonusComponent from_bonus_cmp = ref bonus_pool.Get( from_entity );
            ref MatchBonusComponent to_bonus_cmp = ref bonus_pool.Add( to_entity );
            to_bonus_cmp.bonus = from_bonus_cmp.bonus;
            bonus_pool.Del( from_entity );
            //add move
            ref MatchCellMoveComponent move_cmp = ref move_pool.Add( to_entity );
            move_cmp.from = GetCellPos( from_entity );
            move_cmp.to = GetCellPos( to_entity );
            move_cmp.cur_pos = GetCellPos( from_entity );
            move_cmp.speed = config.move_speed;
            move_cmp.from_entity = from_entity;
            Debug.LogFormat("create move: bonus from={0}, to={1}", move_cmp.from, move_cmp.to);
            //move view
            ref BonusViewComponent from_bonus_view_cmp = ref bonus_view_pool.Get( from_entity );
            ref BonusViewComponent to_bonus_view_cmp = ref bonus_view_pool.Add( to_entity );
            if( init_bonus_view_pool.Has( from_entity ) )
            {
                init_bonus_view_pool.Add( to_entity );
                init_bonus_view_pool.Del( from_entity );
            }
            else
            {
                to_bonus_view_cmp.tr = from_bonus_view_cmp.tr;
            }
            bonus_view_pool.Del( from_entity );
            return true;
        }

        // bool StartMoveBlockCorrectJem( int from_entity, int to_entity )
        // {
        //     if( !jem_pool.Has( from_entity ) ) return false; //is jem?

        //     ref MatchJemComponent from_jem_cmp = ref jem_pool.Get( from_entity );
        //     ref MatchJemComponent to_jem_cmp = ref jem_pool.Add( to_entity );
        //     to_jem_cmp.jem = from_jem_cmp.jem;
        //     jem_pool.Del( from_entity );

        //     return true;
        // }

        void RunWaitCell()
        {
            foreach( int entity in wait_cell_filter)
            {
                ref MatchWaitCellComponent cmp = ref wait_cell_pool.Get( entity );
                cmp.tm_wait -= dt_time;
                if(cmp.tm_wait < 0f)
                {
                    wait_cell_pool.Del( entity );
                }
            }
        }

        void RunBornJem()
        {
            foreach( int entity in born_jem_filter )
            {
                if( IsReadyForBornCell( entity ) && IsReadyForView( entity ) )
                {
                    CreateBornJem( entity );
                }
            }
        }

        void CreateBornJem( int entity )
        {
            ref MatchCellComponent cell_cmp = ref cell_pool.Get( entity );
            ref MatchBornJemComponent born_cmp = ref born_jem_pool.Get( entity );
            ref MatchJemComponent jem_cmp = ref jem_pool.Add(entity);
            jem_cmp.jem = GetRandomJemType();
            ref MatchCellMoveComponent move = ref move_pool.Add( entity );
            move.from = new Vector2( born_cmp.born_x, born_cmp.born_y );
            move.to = new Vector2( cell_cmp.pos_x, cell_cmp.pos_y );
            move.speed = config.move_speed;
            move.cur_pos = move.from;
            move.from_entity = -1;
            //Debug.LogFormat("create born move from={0}, to={1}", move.from, move.to);
            //view - jem
            AddJemView( entity );
            //view - move
            //ref JemMoveComponent view_move_cmp = ref jem_move_pool.Add( entity );
        }

        void OnInit()
        {
            last_touch_id = 0;
            cells_arr = new int[config.size_x, config.size_y];
            for(int y = 0; y < config.size_y; y++)
            {
                for(int x = 0; x < config.size_x; x++)
                {
                    Vector2 pos = GetInitCellPos(x, y);
                    int entity = world.NewEntity();
                    ref MatchCellComponent cell_cmp = ref cell_pool.Add(entity);
                    cell_cmp.x = x;
                    cell_cmp.y = y;
                    cell_cmp.pos_x = pos.x;
                    cell_cmp.pos_y = pos.y;
                    cell_cmp.left_cell = -1;
                    cell_cmp.right_cell = -1;
                    cell_cmp.top_cell = -1;
                    cell_cmp.bottom_cell = -1;
                    cell_cmp.top_left_cell = -1;
                    cell_cmp.top_right_cell = -1;
                    cell_cmp.bottom_left_cell = -1;
                    cell_cmp.bottom_right_cell = -1;
                    cells_arr[x, y] = entity;
                }
            }
            //get broad cells
            int br_x;
            int br_y;
            for(int y = 0; y < config.size_y; y++)
            {
                for(int x = 0; x < config.size_x; x++)
                {
                    int entity = cells_arr[x, y];
                    ref MatchCellComponent cell_cmp = ref cell_pool.Get(entity);
                    //---left--
                    br_x = x - 1;
                    br_y = y;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.top_cell = br_entity;
                    }
                    //---rigth--
                    br_x = x + 1;
                    br_y = y;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.top_cell = br_entity;
                    }
                    //---top--
                    br_x = x;
                    br_y = y - 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.top_cell = br_entity;
                    }
                    //---bottom---
                    br_x = x;
                    br_y = y + 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.bottom_cell = br_entity;
                    }
                    //---left_top---
                    br_x = x - 1;
                    br_y = y - 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.bottom_cell = br_entity;
                    }
                    //---rigth_top---
                    br_x = x + 1;
                    br_y = y - 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.bottom_cell = br_entity;
                    }
                    //---rigth_top---
                    br_x = x + 1;
                    br_y = y - 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.bottom_cell = br_entity;
                    }
                    //---left_bottom---
                    br_x = x - 1;
                    br_y = y + 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.bottom_cell = br_entity;
                    }
                    //---rigth_bottom---
                    br_x = x + 1;
                    br_y = y + 1;
                    if( IsXY(br_x, br_y) )
                    {
                        int br_entity = cells_arr[br_x, br_y];
                        cell_cmp.bottom_cell = br_entity;
                    }
                }
            }

            switch( config.level_id )
            {
                case 0:
                    InitLevel_0();
                break;
                case 1:
                    InitLevel_1();
                break;
            }
            
            //create born jem's
            CreateBornJemCells();
            //create block move correct
            CreateBlockCorrectCells();
        }

        void InitLevel_0()
        {
            //create view
            for(int y = 0; y < config.size_y; y++)
            {
                for(int x = 0; x < config.size_x; x++)
                {
                    int entity = cells_arr[x, y];
                    CreateInitJem( entity, x, y );
                }
            }
        }

        void InitLevel_1()
        {
            int entity = cells_arr[8, 4];
            ref MatchBlockComponent block_cmp = ref block_pool.Add( entity );
            block_cmp.block = BlockType.jelly;
            AddBlockView( entity );
            int entity_2 = cells_arr[6, 4];
            ref MatchBlockComponent block_cmp_2 = ref block_pool.Add( entity_2 );
            block_cmp_2.block = BlockType.jelly;
            AddBlockView( entity_2 );
            int entity_3 = cells_arr[5, 4];
            ref MatchBlockComponent block_cmp_3 = ref block_pool.Add( entity_3 );
            block_cmp_3.block = BlockType.jelly;
            AddBlockView( entity_3 );
            int entity_5 = cells_arr[4, 4];
            ref MatchBlockComponent block_cmp_5 = ref block_pool.Add( entity_5 );
            block_cmp_5.block = BlockType.jelly;
            AddBlockView( entity_5 );

            int entity_4 = cells_arr[7, 2];
            ref MatchBlockComponent block_cmp_4 = ref block_pool.Add( entity_4 );
            block_cmp_4.block = BlockType.jelly;
            AddBlockView( entity_4 );
            
        }

        void CreateBornJemCells()
        {
            for(int x = 0; x < config.size_x; x++)
            {
                int entity = GetFistNotHoleEntity( x );//cells_arr[x, 0];
                if( entity > -1 )
                {
                    int top_entity = cells_arr[x, 0];
                    ref MatchCellComponent cell_top_cmp = ref cell_pool.Get( top_entity );
                    ref MatchBornJemComponent cmp = ref born_jem_pool.Add( entity );
                    cmp.born_x = cell_top_cmp.pos_x;
                    cmp.born_y = cell_top_cmp.pos_y + config.bound_y;
                }
                else
                {
                    Debug.LogErrorFormat("can't get not hole cell with column={0}", x);
                }
            }
        }

        void CreateBlockCorrectCells()
        {
            for(int x = 0; x < config.size_x; x++ )
            {
                CreateBlockCorrectCells( x );
            }
        }

        void CreateBlockCorrectCells( int x )
        {

            int start_block_y = -1;
            for( int y = 0; y < config.size_y; y++ )
            {
                int entity = cells_arr[ x, y ];
                if( start_block_y < 0 && block_pool.Has( entity ) )
                {
                    start_block_y = y;
                }

                if( start_block_y < 0 )
                {
                    block_correct_pool.Del( entity );
                }
                else
                {
                    if( !block_correct_pool.Has( entity )  && !IsBlockCell( entity ) )
                    {
                        block_correct_pool.Add( entity );
                    }
                }
            }
        }

        void CreateInitJem(int entity, int x, int y)
        {
            if( jem_pool.Has(entity) )
            {
                UnityEngine.Debug.LogErrorFormat("entity {0} already has component {1}", 
                                                    entity,  jem_pool.GetComponentType().ToString());
            }
            else
            {
                ref MatchJemComponent jem_cmp = ref jem_pool.Add(entity);
                if(x == 0 && y == 0)
                {
                    jem_cmp.jem = JemType.jem_red;
                }
                else if(x == 0 && y == 2)
                {
                    jem_cmp.jem = JemType.jem_red;
                }
                else if(x == 1 && y == 1)
                {
                    jem_cmp.jem = JemType.jem_red;
                }
                else if(x == 0 && y == 1)
                {
                    jem_cmp.jem = JemType.jem_pink;
                }
                else if(x == 1 && y == 0)
                {
                    jem_cmp.jem = JemType.jem_blue;
                }
                else if(x == 2 && y == 1)
                {
                    jem_cmp.jem = JemType.jem_green;
                }
                else if(x == 0 && y == 3)
                {
                    jem_cmp.jem = JemType.jem_red;
                }
                else
                {
                    jem_cmp.jem = GetRandomJemType();
                }
            }
            AddJemView( entity );
        }

        void AddJemView( int entity )
        {
            //view
            if( jem_view_pool.Has( entity ) )
            {
                int x;
                int y;
                GetEntityXY(entity, out x, out y);
                UnityEngine.Debug.LogErrorFormat("entity {0} already has component {1} x={2}, y={3}", 
                                                    entity,  jem_view_pool.GetComponentType().ToString(), x, y);
            }
            else
            {
                ref JemViewComponent view_cmp = ref jem_view_pool.Add( entity );
                if( !init_jem_view_pool.Has( entity ) )
                {
                    init_jem_view_pool.Add( entity );
                }
            }
        }

        void AddBlockView( int entity )
        {
            if(block_view_pool.Has( entity ))
            {
                UnityEngine.Debug.LogErrorFormat("entity {0} already has component {1}", 
                                                    entity,  block_view_pool.GetComponentType().ToString());
            }
            else
            {
                ref BlockViewComponent block_cmp = ref block_view_pool.Add( entity );
                if( !init_block_view_pool.Has( entity ) )
                {
                    init_block_view_pool.Add( entity );
                }
            }
        }

        void AddBonusView( int entity )
        {
            if(bonus_view_pool.Has( entity ))
            {
                UnityEngine.Debug.LogErrorFormat("entity {0} already has component {1}", 
                                                    entity,  bonus_view_pool.GetComponentType().ToString());
            }
            else
            {
                ref BonusViewComponent bonus_cmp = ref bonus_view_pool.Add( entity );
                if( !init_bonus_view_pool.Has( entity ) )
                {
                    init_bonus_view_pool.Add( entity );
                }
            }
        }

        JemType GetRandomJemType()
        {
            int r_index = UnityEngine.Random.Range(0, jem_types.Length);
            return jem_types[r_index];
        }

        bool IsXY(int x, int y)
        {
            return  (x >= 0 && x < config.size_x) && (y >= 0 && y <config.size_y);
        }

        Vector2 GetInitCellPos(int x, int y)
        {
            float xmin = 0;
            float ymin = 0;
            float half_bound_x = config.bound_x / 2f;
            float half_bound_y = config.bound_y / 2f;

            xmin = -( (float)config.size_x / 2f ) * config.bound_x + half_bound_x;
            ymin = ( (float)config.size_y / 2f ) * config.bound_y - half_bound_y;

            float pos_x = xmin + config.bound_x * x;
            float pos_y = ymin - config.bound_y * y;
            return new Vector2(pos_x, pos_y);
        }

        ref MatchCellComponent GetCellComponent( int entity )
        {
            ref MatchCellComponent cmp = ref cell_pool.Get( entity );
            return ref cmp;
        }

        Vector2 GetCellPos( int entity )
        {
            ref MatchCellComponent cmp = ref cell_pool.Get( entity );
            return new Vector2( cmp.pos_x, cmp.pos_y );
        }

        int GetCellEntity( Vector2 pos, out int cell_x,  out int cell_y)
        {
            cell_x = 0;
            cell_y = 0;
            float half_bound_x = config.bound_x / 2f;
            float half_bound_y = config.bound_y / 2f;
            for(int x=0; x<config.size_x; x++)
            {
                for(int y=0; y<config.size_y; y++)
                {
                    int entity = cells_arr[x, y];
                    ref MatchCellComponent cmp = ref cell_pool.Get( entity );
                    if(pos.x >= cmp.pos_x - half_bound_x &&
                        pos.x <= cmp.pos_x + half_bound_x &&
                        pos.y >= cmp.pos_y - half_bound_y &&
                        pos.y <= cmp.pos_y + half_bound_y)
                        {
                            cell_x = x;
                            cell_y = y;
                            return entity;
                        }
                }
            }
            return -1;
        }

        int GetFistNotHoleEntity( int x )
        {
            //create view
            for(int y = 0; y < config.size_y; y++)
            {
                int entity = cells_arr[x, y];
                if ( !hole_pool.Has( entity ) )
                {
                    return entity;
                }
            }
            return -1;
        }

        int GetCellEntity( int cell_x, int cell_y)
        {
            if( (cell_x >= 0 && cell_x < config.size_x) &&
                (cell_y >= 0 && cell_y < config.size_y) )
            {
                return cells_arr[cell_x, cell_y];
            }
            return -1;
        }

        bool GetEntityXY(int entity, out int result_x, out int result_y)
        {
            result_x = -1;
            result_y = -1;
            for(int x=0; x < config.size_x; x++)
            {
                for(int y=0; y < config.size_y; y++)
                {
                    if( cells_arr[x, y] == entity )
                    {
                        result_x = x;
                        result_y = y;
                        return true;
                    }
                }
            }
            return false;
        }

        bool GetEntityJem( int entity, out JemType jem )
        {
            jem = JemType.jem_blue;
            if( !jem_pool.Has( entity ) ) return false;

            MatchJemComponent jem_cmp = jem_pool.Get( entity );
            jem = jem_cmp.jem;
            return true;
        }

        bool IsInputCell( int entity )
        {
            if( !jem_pool.Has( entity ) ) return false;
            if( move_pool.Has( entity ) ) return false;
            if( move_user_pool.Has( entity ) ) return false;
            if( block_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsMatchCell( int entity )
        {
            if( !jem_pool.Has( entity ) ) return false;
            if( move_pool.Has( entity ) ) return false;
            if( move_user_pool.Has( entity ) ) return false;
            if( block_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsMatchCellIncMove( int entity )
        {
            if( !jem_pool.Has( entity ) ) return false;
            if( move_user_pool.Has( entity ) ) return false;
            if( block_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsReadyForBornCell( int entity )
        {
            if( jem_pool.Has( entity ) ) return false;
            if( bonus_pool.Has( entity ) ) return false;
            if( block_pool.Has( entity ) ) return false;
            if( move_pool.Has( entity ) ) return false;
            if( move_user_pool.Has( entity ) ) return false;
            if( wait_cell_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsReadyForMoveDown( int entity )
        {
            if( jem_pool.Has( entity ) ) return false;
            if( bonus_pool.Has( entity ) ) return false;
            if( block_pool.Has( entity ) ) return false;
            if( move_pool.Has( entity ) ) return false;
            if( move_user_pool.Has( entity ) ) return false;
            if( wait_cell_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsReadyForView( int entity )
        {
            if( jem_collapse_pool.Has( entity ) ) return false;
            if( bonus_create_pool.Has( entity ) ) return false;
            if( jem_to_bonus_collapse_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsReadyForStartDown( int entity )
        {
            if( move_pool.Has( entity ) ) return false;
            if( move_user_pool.Has( entity ) ) return false;
            if( wait_cell_pool.Has( entity ) ) return false;

            return true;
        }

        bool IsBlockCell( int entity )
        {
            if( block_pool.Has( entity ) ) return true;

            return false;
        }

        void GetMatch(int entity, JemType jem)
        {
            int start_x;
            int start_y;
            if( !GetEntityXY(entity, out start_x, out start_y) ) return;

            match_info.Init( new MatchCellInfo{ x = start_x, y = start_y,
                                             entity = entity }, jem );

            AddBroadJemsRecursive( match_info, jem, start_x - 1, start_y );
            AddBroadJemsRecursive( match_info, jem, start_x + 1, start_y );
            AddBroadJemsRecursive( match_info, jem, start_x, start_y - 1 );
            AddBroadJemsRecursive( match_info, jem, start_x, start_y + 1 );
        }

        void AddBroadJemsRecursive(QuickMatchInfo match_info, JemType jem, int x, int y)
        {
            int entity = GetCellEntity(x, y);
            if(entity < 0) return;

            JemType check_jem;
            if( !GetEntityJem(entity, out check_jem) ) return;
            if(check_jem != jem) return;
            if( !IsMatchCell( entity ) ) return;

            if( match_info.Add( new MatchCellInfo{ x = x, y = y,
                                       entity = entity } ) )
            {
                AddBroadJemsRecursive( match_info, jem, x - 1, y );
                AddBroadJemsRecursive( match_info, jem, x + 1, y );
                AddBroadJemsRecursive( match_info, jem, x, y - 1 );
                AddBroadJemsRecursive( match_info, jem, x, y + 1 );
            }
        }

        void GetMatchIncMove(int entity, JemType jem)
        {
            int start_x;
            int start_y;
            if( !GetEntityXY(entity, out start_x, out start_y) ) return;

            match_info.Init( new MatchCellInfo{ x = start_x, y = start_y,
                                             entity = entity }, jem );

            AddBroadJemsRecursiveIncMove( match_info, jem, start_x - 1, start_y );
            AddBroadJemsRecursiveIncMove( match_info, jem, start_x + 1, start_y );
            AddBroadJemsRecursiveIncMove( match_info, jem, start_x, start_y - 1 );
            AddBroadJemsRecursiveIncMove( match_info, jem, start_x, start_y + 1 );
        }

        void StartCollapse( QuickMatchInfo match_info )
        {
            sPool<MatchCellInfo> cell_info_arr = match_info.GetMatchArr();
            ref MatchCellInfo init_cell = ref match_info.GetInitCellRef();
            int init_entity = match_info.GetInitCellRef().entity;
            JemType init_jem = match_info.GetInitJemType();
            BonusType bonus;

            if( match_info.GetBonusType(out bonus) )
            {
                //create bonus component
                ref MatchBonusComponent bonus_cmp = ref bonus_pool.Add( init_entity );
                bonus_cmp.bonus = bonus;
                bonus_cmp.jem = init_jem;
                //block init cell
                AddWaitCellComponent( init_entity, config.tm_bonus_create * 1.2f );
                //create bonus view
                ref BonusViewComponent view_cmp = ref bonus_view_pool.Add( init_entity );
                init_bonus_view_pool.Add( init_entity );
                ref InitBonusCreateViewComponent init_bonus_create_cmp = ref init_bonus_create_pool.Add( init_entity);
                ref BonusCreateViewComponent bonus_create_cmp = ref bonus_create_pool.Add( init_entity );
                bonus_create_cmp.tm_move = config.tm_bonus_create * 0.3f / 2f;
                bonus_create_cmp.is_jem_scale = true;
                bonus_create_cmp.tm_wait = config.tm_bonus_create * 0.7f;
                if( jem_view_pool.Has( init_entity) )
                {
                    ref JemViewComponent jem_view = ref jem_view_pool.Get( init_entity );
                    bonus_create_cmp.jem_tr = jem_view.tr;
                }
                Vector2 init_pos = GetCellPos( init_entity );
                //remove init jem
                jem_pool.Del( init_entity );
                jem_view_pool.Del( init_entity );

                //collapse jem's array
                for(int i=0; i < cell_info_arr.Count; i++)
                {
                    ref MatchCellInfo info = ref cell_info_arr.GetByRef( i );
                    
                    if(info.x == init_cell.x && info.y == init_cell.y) continue;

                    int collapse_entity = GetCellEntity( info.x, info.y );
                    //create jem move
                    init_jem_to_bonus_collapse_pool.Add( collapse_entity );
                    ref JemToBonusCollapseComponent jem_to_bonus_cmp = ref  jem_to_bonus_collapse_pool.Add( collapse_entity );
                    jem_to_bonus_cmp.bonus = bonus;
                    jem_to_bonus_cmp.jem = init_jem;
                    jem_to_bonus_cmp.tm_move = bonus_create_cmp.tm_wait;
                    jem_to_bonus_cmp.from = GetCellPos( collapse_entity );
                    jem_to_bonus_cmp.to = init_pos;
                    //wait jem
                    AddWaitCellComponent( collapse_entity, bonus_create_cmp.tm_wait );
                    if( jem_view_pool.Has(collapse_entity) )
                    {
                        ref JemViewComponent view_jem_cmp = ref jem_view_pool.Get( collapse_entity );
                        jem_to_bonus_cmp.tr = view_jem_cmp.tr;
                    }
                    //remove jem
                    jem_pool.Del( collapse_entity );
                    jem_view_pool.Del( collapse_entity );
                }
            }
            else
            {
                for(int i=0; i < cell_info_arr.Count; i++)
                {
                    ref MatchCellInfo info = ref cell_info_arr.GetByRef( i );
                    int collapse_entity = GetCellEntity( info.x, info.y );
                    AddWaitCellComponent( collapse_entity, config.tm_wait_collapse );
                    //add view collapse
                    if( !jem_collapse_pool.Has( collapse_entity ) )
                    {
                        ref JemCollapseComponent collapse_cmp = ref jem_collapse_pool.Add( collapse_entity );
                        collapse_cmp.tm_wait_change = 0.08f;
                        collapse_cmp.jem = init_jem;
                        //start init
                        if( !init_jem_collapse_pool.Has( collapse_entity ) )
                        {
                            init_jem_collapse_pool.Add( collapse_entity );
                            //Debug.LogFormat("collapse problem: add collapse to x={0}, y={1}", info.x, info.y);
                        }
                        //copy jem view
                        if( jem_view_pool.Has( collapse_entity ) )
                        {
                            ref JemViewComponent jem_view = ref jem_view_pool.Get( collapse_entity );
                            collapse_cmp.jem_tr = jem_view.tr;
                        }
                    }
                    //remove jem
                    jem_pool.Del( collapse_entity );
                    jem_view_pool.Del( collapse_entity );
                }
            }
        }

        bool IsMatchMoveExists( QuickMatchInfo match_info )
        {
            sPool<MatchCellInfo> pool = match_info.GetMatchArr();
            for( int i=0; i<pool.Count; i++ )
            {
                ref MatchCellInfo cell = ref pool.GetByRef( i );
                int entity = GetCellEntity( cell.x, cell.y );
                if( entity < 0 ) continue;

                if( move_pool.Has( entity ) ) return true;
            }

            return false;
        }

        void AddBroadJemsRecursiveIncMove(QuickMatchInfo match_info, JemType jem, int x, int y)
        {
            int entity = GetCellEntity(x, y);
            if(entity < 0) return;

            JemType check_jem;
            if( !GetEntityJem(entity, out check_jem) ) return;
            if(check_jem != jem) return;
            if( !IsMatchCellIncMove( entity ) ) return;

            if( match_info.Add( new MatchCellInfo{ x = x, y = y,
                                       entity = entity } ) )
            {
                AddBroadJemsRecursiveIncMove( match_info, jem, x - 1, y );
                AddBroadJemsRecursiveIncMove( match_info, jem, x + 1, y );
                AddBroadJemsRecursiveIncMove( match_info, jem, x, y - 1 );
                AddBroadJemsRecursiveIncMove( match_info, jem, x, y + 1 );
            }
        }

        void AddWaitCellComponent(int entity, float tm_wait)
        {
            if(wait_cell_pool.Has( entity ))
            {
                ref MatchWaitCellComponent cmp = ref wait_cell_pool.Get( entity );
                if(cmp.tm_wait < tm_wait)
                {
                    cmp.tm_wait = tm_wait;
                }
            }
            else
            {
                ref MatchWaitCellComponent cmp = ref wait_cell_pool.Add( entity );
                cmp.tm_wait = tm_wait;
            }
        }
    }
}