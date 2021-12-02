using System;
using System.Collections.Generic;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class DtMgr : BaseMgr
    {
        float dt;
        float smooth_dt;

        public DtMgr(EcsWorld world) : base(world)
        {
            this.world = world;
        }
        
        public void SetDt(float dt, float smooth_dt)
        {
            this.dt = dt;
            this.smooth_dt = smooth_dt;
        }

        public float GetDt()
        {
            return dt;
        }
    }
}
