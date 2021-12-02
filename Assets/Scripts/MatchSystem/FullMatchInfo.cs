using UnityEngine;
using System.Collections.Generic;
using Pool;


namespace BadCode.Main
{
        public class FullMatchInfo
        {
            class SortCellXYComparer : IComparer<MatchCellInfo>
            {
                public int Compare(MatchCellInfo p1, MatchCellInfo p2) 
                {
                    int a = p1.x.CompareTo(p2.x);
                    if(a == 0)
                        a = p1.y.CompareTo(p2.y);
                    return a;
                }
            }
            class SortCellYXComparer : IComparer<MatchCellInfo>
            {
                public int Compare(MatchCellInfo p1, MatchCellInfo p2) 
                {
                    int a = p1.y.CompareTo(p2.y);
                    if(a == 0)
                        a = p1.x.CompareTo(p2.x);
                    return a;
                }
            }
            
            sPool<MatchCellInfo> bmatch;
            sPool<MatchCellInfo> result;
            sPoolContainer<MatchCellInfo> vmatch;
            sPoolContainer<MatchCellInfo> hmatch;
            IComparer<MatchCellInfo> cell_xy_sort;
            IComparer<MatchCellInfo> cell_yx_sort;
            MatchCellInfo init_cell;
            bool is_matched;

            public FullMatchInfo( MatchCellInfo init_cell )
            {
                bmatch = new sPool<MatchCellInfo>();
                result = new sPool<MatchCellInfo>();
                vmatch = new sPoolContainer<MatchCellInfo>();
                hmatch = new sPoolContainer<MatchCellInfo>();
                cell_xy_sort = new SortCellXYComparer();
                cell_yx_sort = new SortCellYXComparer();
                this.init_cell = init_cell;
            }

            public void Init()
            {
                bmatch.Clear();
                result.Clear();
                vmatch.Recycle();
                hmatch.Recycle();
                is_matched = false;
            }

            public bool Add(MatchCellInfo cell)
            {
                if( Exists( bmatch, in cell ) )
                {
                    return false;
                }
                else
                {
                    bmatch.Add( cell );
                    return true;
                }
            }

            public sPool<MatchCellInfo> GetMatchArr()
            {
                return result;
            }

            void PrepareVertical()
            {
                bmatch.Sort( cell_xy_sort );
                sPool<MatchCellInfo> tmp = vmatch.Get();
                int last_x = -1;
                int last_y = -1;
                for(int i=0; i<bmatch.Count; i++)
                {
                    ref MatchCellInfo info = ref bmatch.GetByRef( i );
                    if( tmp.Count == 0 )
                    {
                        tmp.Add( info );
                        last_x = info.x;
                        last_y = info.y;
                    }
                    else if( last_x == info.x && Mathf.Abs( last_y - info.y) == 1)
                    {
                        tmp.Add( info );
                    }
                    else
                    {
                        //add
                        if(tmp.Count < 3)
                        {
                            vmatch.Recycle( tmp );
                        }
                        //create new
                        tmp = vmatch.Get();
                        //add 
                        tmp.Add( info );
                        last_x = info.x;
                        last_y = info.y;
                    }
                }

                sPool<sPool<MatchCellInfo>> used_pool =  vmatch.GetUsed();
                for(int i=used_pool.Count-1; i >=0; i--)
                {
                    sPool<MatchCellInfo> used_item = used_pool[ i ];
                    if( used_item.Count < 2)
                    {
                        vmatch.Recycle( used_item );
                    }
                }
            }

            void PrepareHorizontal()
            {
                bmatch.Sort( cell_yx_sort );
                sPool<MatchCellInfo> tmp = hmatch.Get();
                int last_x = -1;
                int last_y = -1;
                for(int i=0; i<bmatch.Count; i++)
                {
                    ref MatchCellInfo info = ref bmatch.GetByRef( i );
                    if( tmp.Count == 0 )
                    {
                        tmp.Add( info );
                        last_x = info.x;
                        last_y = info.y;
                    }
                    else if( last_y == info.y && Mathf.Abs( last_x - info.x) == 1)
                    {
                        tmp.Add( info );
                    }
                    else
                    {
                        //add
                        if(tmp.Count < 3)
                        {
                            hmatch.Recycle( tmp );
                        }
                        //create new poll
                        tmp = hmatch.Get();
                        //add 
                        tmp.Add( info );
                        last_x = info.x;
                        last_y = info.y;
                    }
                }

                sPool<sPool<MatchCellInfo>> used_pool =  hmatch.GetUsed();
                for(int i=used_pool.Count-1; i >=0; i--)
                {
                    sPool<MatchCellInfo> used_item = used_pool[ i ];
                    if( used_item.Count < 2)
                    {
                        hmatch.Recycle( used_item );
                    }
                }
            }

            void GetResult()
            {
                //get corner
                bool is_cornenr = false;
                sPool<sPool<MatchCellInfo>> h_used_pools =  hmatch.GetUsed();
                sPool<sPool<MatchCellInfo>> v_used_pools =  vmatch.GetUsed();
                for( int h_p=0; h_p<h_used_pools.Count; h_p++)
                {
                    sPool<MatchCellInfo> h_pool = h_used_pools[ h_p ];
                    for( int h=0; h<h_pool.Count; h++)
                    {
                        ref MatchCellInfo h_info = ref h_pool.GetByRef( h );
                        for( int v_p=0; v_p<v_used_pools.Count; v_p++)
                        {
                            sPool<MatchCellInfo> v_pool = v_used_pools[ v_p ];
                            for( int v=0; v<h_pool.Count; v++)
                            {
                                ref MatchCellInfo v_info = ref v_pool.GetByRef( v );
                                if( (v_info.x == h_info.x && v_info.y == h_info.y) && !Exists( result, v_info ) )
                                {
                                    //add corner
                                    result.Add( v_info );
                                    //add vertical
                                    for(int a_v=0; a_v<v_pool.Count; a_v++)
                                    {
                                        if( a_v != v )
                                        {
                                            ref MatchCellInfo a_v_info = ref v_pool.GetByRef( a_v );
                                            result.Add( a_v_info );
                                        }
                                    }
                                    //add horizontal
                                    for(int a_h=0; a_h<h_pool.Count; a_h++)
                                    {
                                        if( a_h != h )
                                        {
                                            ref MatchCellInfo a_h_info = ref h_pool.GetByRef( a_h );
                                            result.Add( a_h_info );
                                        }
                                    }
                                    is_cornenr = true;
                                }
                            }
                        }
                    }
                }

                if( is_cornenr )
                {
                    
                }
            }

            bool Exists(sPool<MatchCellInfo> arr, in MatchCellInfo check_cell)
            {
                return Index( arr, in check_cell) >= 0;
            }

            int Index(sPool<MatchCellInfo> arr, in MatchCellInfo check_cell)
            {
                for(int i=0; i<arr.Count; i++)
                {
                    ref MatchCellInfo cell = ref arr.GetByRef( i );
                    if(cell.x == check_cell.x && cell.y == check_cell.y)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
}