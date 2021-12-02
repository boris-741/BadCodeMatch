using UnityEngine;

namespace BadCode.Main
{
    public struct JemUserMoveComponent
    {
        public int to_entity;
        public Vector2 from;
        public Vector2 to;
        public float tm_move;
        public Transform tr;
        public float user_move_speed;
        public bool is_user_active;
    }
}