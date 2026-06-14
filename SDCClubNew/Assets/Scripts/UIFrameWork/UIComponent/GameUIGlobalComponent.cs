using System;
using System.Collections.Generic;
using SDClub.Core;
using UnityEngine;

namespace SDClub.UIFrameWork
{
    // Singleton 管理全局 UI 状态
    public class GameUIGlobalComponent
    {
        public static GameUIGlobalComponent Instance { get; } = new();
        
        /// UI 层级管理: 每个层级对应一个 Canvas
        public Dictionary<UILayerType, Transform> Layers { get; } = new();
        
        /// UI 生命周期注册表: entityType → UILogicSystem 实例
        public Dictionary<Type, object> UILifeHandlers { get; } = new();
        
        /// UI 绑定注册表: entityType → UIAutoSystem 实例
        public Dictionary<Type, object> UIBindHandlers { get; } = new();
        
        /// Canvas 根节点
        public Transform UIRoot { get; set; }
        public Camera UICamera { get; set; }
        
        // 注册 UI 层级
        public void RegisterLayer(UILayerType layer, Transform canvasTransform)
        {
            Layers[layer] = canvasTransform;
        }
        
        public Transform GetLayer(UILayerType layer)
        {
            Layers.TryGetValue(layer, out var t);
            return t;
        }
        
        // 注册生命周期处理器
        public void RegisterLifeHandler(Type entityType, object handler)
        {
            UILifeHandlers[entityType] = handler;
        }
        
        public T GetLifeHandler<T>(Type entityType) where T : class
        {
            UILifeHandlers.TryGetValue(entityType, out var h);
            return h as T;
        }
        
        // 注册绑定处理器
        public void RegisterBindHandler(Type entityType, object handler)
        {
            UIBindHandlers[entityType] = handler;
        }
        
        public T GetBindHandler<T>(Type entityType) where T : class
        {
            UIBindHandlers.TryGetValue(entityType, out var h);
            return h as T;
        }
    }
}
