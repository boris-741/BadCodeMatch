using UnityEngine;
using UnityEngine.UI;
using Leopotam.EcsLite;

namespace BadCode.Main
{
    public class UIStartSystem : UIBaseSystem<UIStartComponent>
    {
        protected override void RunUIWait()
        {
            if(ui_wait_filter.GetEntitiesCount() == 0) return;
            
            change_list.Clear();
            
            foreach(int entity in ui_wait_filter)
            {
                ref UIStartComponent ui_cmp = ref ui_pool.Get(entity);   
                ui_cmp.loading_text = ui_cmp.main_go.transform.Find("Text").GetComponent<Text>();
                ready_pool.Add(entity);
                wait_pool.Del(entity);
                ref UIObjComponent obj_cmp = ref obj_pool.Add(entity);
                obj_cmp.body_go = ui_cmp.main_go;
                obj_cmp.body_tr = ui_cmp.main_go.transform;
                obj_cmp.body_cg = ui_cmp.main_go.AddComponent<CanvasGroup>();
            }

            for(int i=0; i< change_list.Count; i++)
            {
                OnInit( change_list[i] );
            }

            change_list.Clear();
        }

        protected override void RunUIAct() {}

        protected override void RunUIReady()
        {
            foreach(int entity in ui_ready_filter)
            {
                ref UIStartComponent ui_cmp = ref ui_pool.Get(entity);    
                ui_cmp.tmchange += dt_tm; 
                OnText(ref ui_cmp);
            }
        }

        void OnText( ref UIStartComponent ui_cmp )
        {
            if(ui_cmp.tmchange > 0.25f)
            {
                SetState(ui_cmp.loading_text, ui_cmp.view_state);
                ui_cmp.view_state += 1;
                ui_cmp.tmchange = 0f;
                if(ui_cmp.view_state > 3)
                {
                    ui_cmp.view_state = 0;
                }
            }
        }

        void SetState(Text text, int state)
        {
            switch(state)
            {
                case 0:
                    text.text = string.Format("Loading");
                break;
                case 1:
                    text.text = string.Format("Loading.");
                break;
                case 2:
                    text.text = string.Format("Loading..");
                break;
                case 3:
                    text.text = string.Format("Loading...");
                break;
                default:
                    text.text = string.Format("Loading...");
                break;
            }
        }
    }
}