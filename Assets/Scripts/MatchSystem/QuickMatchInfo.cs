using System.Collections.Generic;
using Pool;


namespace BadCode.Main
{
        public class QuickMatchInfo
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
            
            sPool<MatchCellInfo> vmatch;
            sPool<MatchCellInfo> hmatch;
            sPool<MatchCellInfo> result;
            IComparer<MatchCellInfo> cell_xy_sort;

            MatchCellInfo init_cell;
            JemType init_jem;

            public QuickMatchInfo()
            {
                vmatch = new sPool<MatchCellInfo>();
                hmatch = new sPool<MatchCellInfo>();
                result = new sPool<MatchCellInfo>();
                cell_xy_sort = new SortCellXYComparer();
            }

            public void Init(MatchCellInfo cell, JemType jem)
            {
                init_cell = cell;
                init_jem = jem;
                //clear pool's
                vmatch.Clear();
                hmatch.Clear();
                //add init cell
                vmatch.Add( cell );
                hmatch.Add( cell );
            }

            public ref MatchCellInfo GetInitCellRef()
            {
                return ref init_cell;
            }

            public JemType GetInitJemType()
            {
                return init_jem;
            }

            public bool Add(MatchCellInfo cell)
            {
                bool is_match = (cell.x == init_cell.x  || cell.y == init_cell.y) && 
                    !(cell.x == init_cell.x && cell.y == init_cell.y);
                if( !is_match ) return false;
                if( Exists( hmatch, in cell) ) return false;
                if( Exists( vmatch, in cell) ) return false;

                if(cell.x == init_cell.x)//vert
                {
                    vmatch.Add( cell );
                }
                else if(cell.y == init_cell.y)//hor
                {
                    hmatch.Add( cell );
                }

                return true;
            }

            public bool IsAnyMatch()
            {
                return vmatch.Count > 2 || hmatch.Count > 2;
            }

            public int GetMatchCount()
            {
                int vlength = vmatch.Count;
                int hlength = hmatch.Count;
                if(vlength >= 3 && hlength >= 3)
                {
                    return vlength + hlength;
                }
                else if(vlength >= 3)
                {
                    return vlength;
                }
                else if(hlength >= 3)
                {
                    return hlength;
                }
                else
                {
                    return 0;
                }
            }

            public bool GetBonusType( out BonusType bonus )
            {
                bonus = BonusType.vertical;
                int vlength = vmatch.Count;
                int hlength = hmatch.Count;
                if(vlength >= 3 && hlength >= 3)
                {
                    vmatch.Sort( cell_xy_sort );
                    hmatch.Sort( cell_xy_sort );
                    int vindex = Index(vmatch, init_cell);
                    int hindex = Index(hmatch, init_cell);
                    if( (vindex == 0 || vindex == vlength - 1) && 
                        (hindex == 0 || hindex == hlength - 1) )
                    {
                        bonus = BonusType.corner;
                    }
                    else
                    {
                        bonus = BonusType.cross;
                    }
                     return true;
                }
                else if(vlength > 3)
                {
                    bonus = BonusType.horizontal;
                    return true;
                }
                else if(hlength > 3)
                {
                    bonus = BonusType.vertical;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
            public sPool<MatchCellInfo> GetMatchArr()
            {
                result.Clear();
                int vlength = vmatch.Count;
                int hlength = hmatch.Count;
                if(vlength >= 3 && hlength >= 3)
                {
                    for(int i = 0; i < vmatch.Count; i++)
                    {
                        ref MatchCellInfo info = ref vmatch.GetByRef( i );
                        if( !Exists(result, info) )
                            result.Add( info );
                    }

                    for(int i = 0; i < hmatch.Count; i++)
                    {
                        ref MatchCellInfo info = ref hmatch.GetByRef( i );
                        if( !Exists(result, info) )
                            result.Add( info );
                    }
                    
                }
                else if(vlength >= 3)
                {
                    for(int i = 0; i < vmatch.Count; i++)
                    {
                        result.Add( vmatch[i] );
                    }
                }
                else if(hlength >= 3)
                {
                    for(int i = 0; i < hmatch.Count; i++)
                    {
                        result.Add( hmatch[i] );
                    }
                }

                return result;
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