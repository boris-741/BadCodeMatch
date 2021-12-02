using UnityEngine;

namespace BadCode.Main
{
    public struct MatchCellUserMoveComponent
    {
        public int to_entity;
        public Vector2 cur_pos;
        public Vector2 from;
        public Vector2 to;
        public float tm_move;
        public int home_x;
        public int home_y;
        public bool is_user_active;
        public bool is_back_move;
    }
}