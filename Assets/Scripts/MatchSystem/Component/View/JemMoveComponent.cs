using UnityEngine;

namespace BadCode.Main
{
    public struct JemMoveComponent
    {
        public Transform tr;
        public int from_entity;
        public Vector2 from;
        public Vector2 to;
        public float tm_move;
        public float speed;
    }
}