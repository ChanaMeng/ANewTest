using System;
using SDClub.Core;

namespace SDClub.UIFrameWork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UILifeAttribute : Attribute
    {
        public Type LogicType { get; }
        public UILifeAttribute(Type logicType)
        {
            LogicType = logicType;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class UIBindAttribute : Attribute
    {
        public Type BindType { get; }
        public UIBindAttribute(Type bindType)
        {
            BindType = bindType;
        }
    }
    
    // UI 生命周期接口
    public interface IUIOnLoad { void OnLoad(); }
    public interface IUIOnShow { void OnShow(object args); }
    public interface IUIOnHide { void OnHide(); }
    public interface IUIOnDestroy { void OnDestroy(); }
    
    // UI 生命周期系统基类
    public abstract class UILogicSystem<T> : IUIOnLoad, IUIOnShow, IUIOnHide, IUIOnDestroy 
        where T : Entity
    {
        public Entity Entity { get; set; }
        
        public virtual void OnLoad() { }
        public virtual void OnShow(object args) { }
        public virtual void OnHide() { }
        public virtual void OnDestroy() { }
    }
    
    // UI 绑定系统基类 - 负责绑定 Unity 组件引用
    public abstract class UIAutoSystem<T> where T : Entity
    {
        public abstract void BindUI(T self);
    }
}
