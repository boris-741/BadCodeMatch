using UnityEngine;

namespace BadCode.Main
{
    public struct JemCollapseComponent
    {
        public Transform jem_tr;
        public Transform tr;
        public SpriteRenderer sr;
        public JemType jem;
        public int collapse_index;
        public float tm_wait_change;
        public float tm_cur;
        public float tm_wait_collapse;
    }
}