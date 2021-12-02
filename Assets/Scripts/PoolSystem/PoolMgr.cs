using UnityEngine;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class PoolMgr : BaseMgr
    {
        //ecs pool
        EcsPool<PoolGOComponent> go_pool;
        EcsPool<BisyComponent> bisy_pool;
        EcsPool<FreeComponent> free_pool;
        EcsPool<AtlasComponent> atlas_pool;
        EcsPool<PoolSpriteComponent> sprite_pool;
        EcsPool<InitPoolComponent> init_pool;
        EcsPool<WaitComponent> wait_pool;

        //ecs filter
        EcsFilter atlas_ready_filter;
        EcsFilter go_bisy_filter;
        EcsFilter go_free_filter;
        EcsFilter sprite_filter;
        EcsFilter pool_ready_filter;

        public PoolMgr(EcsWorld world) : base(world)
        {
            this.world = world;
            //pool
            go_pool = world.GetPool<PoolGOComponent>();
            bisy_pool = world.GetPool<BisyComponent>();
            free_pool = world.GetPool<FreeComponent>();
            atlas_pool = world.GetPool<AtlasComponent>();
            sprite_pool = world.GetPool<PoolSpriteComponent>();
            init_pool = world.GetPool<InitPoolComponent>();
            wait_pool = world.GetPool<WaitComponent>();
            //filter
            atlas_ready_filter = world.Filter<AtlasComponent>().Inc<ReadyComponent>().End();
            go_bisy_filter = world.Filter<PoolGOComponent>().Inc<BisyComponent>().End();
            go_free_filter = world.Filter<PoolGOComponent>().Inc<FreeComponent>().End();
            sprite_filter = world.Filter<PoolSpriteComponent>().End();
            pool_ready_filter = world.Filter<InitPoolComponent>().Inc<ReadyComponent>().End();  
        }

        public void InitPool()
        {
            int entity = world.NewEntity();
            init_pool.Add(entity);
            wait_pool.Add(entity);
        }
        
        public bool IsInitPool()
        {
            return pool_ready_filter.GetEntitiesCount() > 0;
        }

        public void ClearInit()
        {
            foreach(int entity in pool_ready_filter)
            {
                world.DelEntity(entity);
            }
        }

        bool GetGOFromPool (out int result_entity, out GameObject go, out SpriteRenderer sr)
        {
            go = null;
            sr = null;
            result_entity = -1;
            foreach(int entity in go_free_filter)
            {
                ref PoolGOComponent go_cmp = ref go_pool.Get(entity);   
                go = go_cmp.go;
                sr = go_cmp.sr;
                result_entity = entity;
                return true;
            }
            return false;
        }

        public bool GetOrCreateSprite(CoreObjectType object_type, out Sprite spr)
        {
            spr = null;
            foreach(int entity in sprite_filter)
            {
                ref PoolSpriteComponent spr_cmp = ref sprite_pool.Get(entity);  
                if(spr_cmp.obj_type == object_type)
                {
                    spr = spr_cmp.sprite;
                    return true;
                }
            }

            foreach(int entity in atlas_ready_filter)
            {
                ref AtlasComponent atlas_cmp = ref atlas_pool.Get(entity);
                string sprite_name = GetSpriteName(object_type);
                if(string.IsNullOrEmpty(sprite_name))
                {
                    Debug.LogErrorFormat("can't find sprite for type={0}", object_type);
                    break;
                }
                spr = atlas_cmp.atlas.GetSprite(sprite_name);
                if(spr == null)
                {
                    Debug.LogErrorFormat("pool atlas: can't get sprite [{0}] from atlas", sprite_name);
                }
                int spr_entity = world.NewEntity();
                ref PoolSpriteComponent spr_cmp = ref sprite_pool.Add(spr_entity);
                spr_cmp.sprite = spr;
                spr_cmp.obj_type = object_type;
                return true;
            }
            return false;
        }

        public GameObject GetEmptyGO( out SpriteRenderer sr )
        {
            GameObject go;
            int result_entity;
            if(GetGOFromPool(out result_entity, out go, out sr))
            {
                bisy_pool.Add(result_entity);
                free_pool.Del(result_entity);
                return go;
            }
            else
            {
                result_entity= world.NewEntity();
                ref PoolGOComponent go_cmp = ref go_pool.Add(result_entity);
                bisy_pool.Add(result_entity);
                go = new GameObject();
                sr = go.AddComponent<SpriteRenderer>();
                go_cmp.go = go;
                go_cmp.tr = go.transform;
                go_cmp.sr = sr;
                return go;
            }
        }

        public GameObject GetGO( CoreObjectType object_type )
        {
            GameObject go;
            SpriteRenderer sr;
            Sprite sprite;
            int result_entity;
            if( !GetOrCreateSprite(object_type, out sprite) )
            {
                Debug.LogErrorFormat( "can't get sprite for {0}", object_type.ToString() );
            }

            if(sprite == null)
            {
                Debug.LogErrorFormat("pool atlas: sprite is null");
            }

            if(GetGOFromPool(out result_entity, out go, out sr))
            {
                bisy_pool.Add(result_entity);
                free_pool.Del(result_entity);
                sr.sprite = sprite;
                return go;
            }
            else
            {
                result_entity = world.NewEntity();
                ref PoolGOComponent go_cmp = ref go_pool.Add(result_entity);
                bisy_pool.Add(result_entity);
                go = new GameObject();
                sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                go_cmp.go = go;
                go_cmp.tr = go.transform;
                go_cmp.sr = sr;
                return go;
            }
        }

        public void ReleaseGO(GameObject go)
        {
            foreach(int entity in go_bisy_filter)
            {
                ref PoolGOComponent go_cmp = ref go_pool.Get(entity);   
                if(go_cmp.go == go)
                {
                    go_cmp.tr.SetParent( null );
                    go_cmp.tr.localPosition = new Vector3(0f, 10000f, 0f);
                    go_cmp.tr.localScale = Vector3.one;
                    go_cmp.tr.localEulerAngles = Vector3.zero;
                    bisy_pool.Del(entity);
                    free_pool.Add(entity);
                    break;
                }
            }
        }

        string GetSpriteName(CoreObjectType object_type)
        {
            switch(object_type)
            {
                case CoreObjectType.jem_blue:
                    return "bean_blue";
                case CoreObjectType.jem_green:
                    return "bean_green";
                case CoreObjectType.jem_pink:
                    return "bean_pink";
                case CoreObjectType.jem_red:
                    return "bean_red";
                case CoreObjectType.jelly_blue:
                    return "jelly_blue";
                case CoreObjectType.jelly_green:
                    return "jelly_green";
                case CoreObjectType.jelly_pink:
                    return "jelly_pink";
                case CoreObjectType.jelly_red:
                    return "jelly_red";
                case CoreObjectType.mm_brown:
                    return "mm_brown";
                case CoreObjectType.explosionblue01:
                    return "explosionblue01";
                case CoreObjectType.explosionblue02:
                    return "explosionblue02";
                case CoreObjectType.explosionblue03:
                    return "explosionblue03";
                case CoreObjectType.explosionblue04:
                    return "explosionblue04";
                case CoreObjectType.explosionblue05:
                    return "explosionblue05";
                case CoreObjectType.explosiongreen01:
                    return "explosiongreen01";
                case CoreObjectType.explosiongreen02:
                    return "explosiongreen02";
                case CoreObjectType.explosiongreen03:
                    return "explosiongreen03";
                case CoreObjectType.explosiongreen04:
                    return "explosiongreen04";
                case CoreObjectType.explosiongreen05:
                    return "explosiongreen05";
                case CoreObjectType.explosionpink01:
                    return "explosionpink01";
                case CoreObjectType.explosionpink02:
                    return "explosionpink02";
                case CoreObjectType.explosionpink03:
                    return "explosionpink03";
                case CoreObjectType.explosionpink04:
                    return "explosionpink04";
                case CoreObjectType.explosionpink05:
                    return "explosionpink05";
                case CoreObjectType.explosionred01:
                    return "explosionred01";
                case CoreObjectType.explosionred02:
                    return "explosionred02";
                case CoreObjectType.explosionred03:
                    return "explosionred03";
                case CoreObjectType.explosionred04:
                    return "explosionred04";
                case CoreObjectType.explosionred05:
                    return "explosionred05";
                case CoreObjectType.wrappedsolid_blue:
                    return "wrappedsolid_blue";
                case CoreObjectType.wrappedsolid_green:
                    return "wrappedsolid_green";
                case CoreObjectType.wrappedsolid_pink:
                    return "wrappedsolid_purple";
                case CoreObjectType.wrappedsolid_red:
                    return "wrappedsolid_red";
                case CoreObjectType.swirl_blue:
                    return "swirl_blue";
                case CoreObjectType.swirl_green:
                    return "swirl_green";
                case CoreObjectType.swirl_pink:
                    return "swirl_pink";
                case CoreObjectType.swirl_red:
                    return "swirl_red";
                default:
                    return "";
            }
        }

    }
}