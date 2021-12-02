using UnityEngine;

namespace BadCode.Main
{
    public struct DragComponent
    {
        public uint touch_id;
        public Vector2 start_pos;
        public Vector2 last_pos;
        public Vector2 delta_pos;
    }
}