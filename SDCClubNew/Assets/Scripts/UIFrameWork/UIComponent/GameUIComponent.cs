using System.Collections.Generic;
using SDClub.Core;

namespace SDClub.UIFrameWork
{
    public class GameUIComponent : Entity, IAwake, IDestroy
    {
        /// 所有打开的 UI (按名称索引)
        public Dictionary<string, Entity> AllUIs { get; } = new();
        
        /// 正在显示的 UI
        public Dictionary<string, Entity> InShow { get; } = new();
        
        /// 已隐藏的 UI
        public Dictionary<string, Entity> InHide { get; } = new();
        
        /// 多实例 UI
        public Dictionary<string, List<Entity>> MultiUI { get; } = new();
        
        /// 记录所有 UI 列表 (按打开顺序)
        public List<Entity> UIList { get; } = new();
        
        // 创建UI
        public T CreateUI<T>(string uiName, UILayerType layer = UILayerType.Center) where T : Entity, IAwake, new()
        {
            if (AllUIs.TryGetValue(uiName, out var exist))
            {
                HideUI(uiName);
            }
            
            var ui = AddChild<T>();
            ui.AddComponent<UIViewComponent, string>(uiName);
            AllUIs[uiName] = ui;
            InShow[uiName] = ui;
            UIList.Add(ui);
            return ui;
        }
        
        // 获取UI
        public T GetUI<T>(string uiName) where T : Entity
        {
            AllUIs.TryGetValue(uiName, out var ui);
            return ui as T;
        }
        
        // 显示UI
        public void ShowUI(string uiName, object args = null)
        {
            if (!InHide.TryGetValue(uiName, out var ui)) return;
            InHide.Remove(uiName);
            InShow[uiName] = ui;
            
            var handler = GameUIGlobalComponent.Instance.GetLifeHandler<IUIOnShow>(ui.GetType());
            handler?.OnShow(args);
        }
        
        // 隐藏UI
        public void HideUI(string uiName)
        {
            if (!InShow.TryGetValue(uiName, out var ui)) return;
            InShow.Remove(uiName);
            InHide[uiName] = ui;
            
            var handler = GameUIGlobalComponent.Instance.GetLifeHandler<IUIOnHide>(ui.GetType());
            handler?.OnHide();
        }
        
        // 销毁UI
        public void DestroyUI(string uiName)
        {
            if (AllUIs.TryGetValue(uiName, out var ui))
            {
                var handler = GameUIGlobalComponent.Instance.GetLifeHandler<IUIOnDestroy>(ui.GetType());
                handler?.OnDestroy();
                
                AllUIs.Remove(uiName);
                InShow.Remove(uiName);
                InHide.Remove(uiName);
                UIList.Remove(ui);
                ui.Dispose();
            }
        }
        
        public void DestroyAll()
        {
            foreach (var kv in new Dictionary<string, Entity>(AllUIs))
            {
                DestroyUI(kv.Key);
            }
        }
    }
}
