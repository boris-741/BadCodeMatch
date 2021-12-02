using UnityEngine;

namespace BadCode.Main
{
    public struct MatchCellMoveComponent
    {
        public int from_entity;
        public Vector2 from;
        public Vector2 to;
        public Vector2 cur_pos;
        public float tm_move;
        public float speed;
    }
}