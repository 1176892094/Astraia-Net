using System;
using JFramework.Core;
using JFramework.Interface;
using UnityEngine;

namespace JFramework.Net
{
    /// <summary>
    /// 实体的抽象类
    /// </summary>
    [Serializable]
    public abstract class NetworkEntity : NetworkBehaviour, IEntity
    {
        /// <summary>
        /// 实体Id
        /// </summary>
        public int entityId { get; private set; }

        /// <summary>
        /// 实体更新
        /// </summary>`
        protected virtual void OnUpdate() { }

        /// <summary>
        /// 实体启用
        /// </summary>
        protected virtual void OnEnable() => GlobalManager.Listen(this);

        /// <summary>
        /// 实体禁用
        /// </summary>
        protected virtual void OnDisable() => GlobalManager.Remove(this);

        /// <summary>
        /// 实体销毁 (如果能获取到角色接口 则销毁角色的控制器)
        /// </summary>
        protected virtual void OnDestroy() => GetComponent<ICharacter>()?.Destroy();

        /// <summary>
        /// 实体Id
        /// </summary>
        int IEntity.entityId
        {
            get => entityId;
            set => entityId = value;
        }

        /// <summary>
        /// 实体Transform
        /// </summary>
        Transform IEntity.transform => transform;

        /// <summary>
        /// 实体GameObject
        /// </summary>
        GameObject IEntity.gameObject => gameObject;

        /// <summary>
        /// 实体接口调用实体更新方法
        /// </summary>
        void IEntity.Update() => OnUpdate();
    }
}