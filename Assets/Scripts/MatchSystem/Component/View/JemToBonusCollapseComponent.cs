using UnityEngine;

namespace BadCode.Main
{
    public struct JemToBonusCollapseComponent
    {
        public BonusType bonus;
        public JemType jem;
        public Transform tr;
        public SpriteRenderer sr;
        public Vector2 from;
        public Vector2 to;
        public float tm_wait;
        public float tm_move;
        public float cur_tm_move;
    }
}