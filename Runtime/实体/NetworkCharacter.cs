using System;
using System.Collections.Generic;
using JFramework.Interface;
using JFramework.Net;
using Sirenix.OdinInspector;
using UnityEngine;

// ReSharper disable All
namespace JFramework.Net
{
    public abstract class NetworkCharacter : NetworkBehaviour, IEntity
    {
        /// <summary>
        /// 控制器容器
        /// </summary>
        [ShowInInspector, LabelText("控制器列表"), SerializeField]
        private readonly Dictionary<Type, ScriptableObject> controllers = new Dictionary<Type, ScriptableObject>();
        
        /// <summary>
        /// 实体销毁
        /// </summary>
        public virtual void Despawn() { }

        /// <summary>
        /// 实体更新
        /// </summary>`
        protected virtual void OnUpdate() { }

        /// <summary>
        /// 实体启用
        /// </summary>
        protected virtual void OnEnable() => ((IEntity)this).Enable();

        /// <summary>
        /// 实体禁用
        /// </summary>
        protected virtual void OnDisable() => ((IEntity)this).Disable();

        /// <summary>
        /// 实体销毁
        /// </summary>
        private void OnDestroy()
        {
            try
            {
                foreach (var scriptable in controllers.Values)
                {
                    Destroy(scriptable);
                }

                controllers.Clear();
                Despawn();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <typeparam name="T">可使用任何继承IController的对象</typeparam>
        /// <returns>返回控制器对象</returns>
        public T GetOrAddCtrl<T>() where T : ScriptableObject, IController
        {
            var key = typeof(T);
            if (!controllers.ContainsKey(key))
            {
                var controller = ScriptableObject.CreateInstance<T>();
                controllers.Add(key, controller);
                return (T)controller.Spawn(this);
            }

            return (T)controllers[key];
        }

        /// <summary>
        /// 实体接口调用实体更新方法
        /// </summary>
        void IEntity.Update() => OnUpdate();
    }
}