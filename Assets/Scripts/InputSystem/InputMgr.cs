using System;
using UnityEngine;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class InputMgr : BaseMgr
    {
        Camera camera; 
        
        public InputMgr(EcsWorld world) : base(world)
        {
            this.world = world;
        }

        public void SetCamera(Camera camera)
        {
            this.camera = camera;
        }

        public Vector2 GetWorlPos(Vector2 screen_pos)
        {
            Vector2 input_world_pos = camera.ScreenToWorldPoint( screen_pos );
            return input_world_pos;
        }
        
        
    }
}