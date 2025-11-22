using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QHZ
{
    /// <summary>
    /// 用于触发Boss战的碰撞器事件脚本
    /// 当玩家进入触发器时激活Boss战斗事件
    /// </summary>
    public class EventColliderBeginBossFight : MonoBehaviour
    {
        // 引用世界事件管理器
        [SerializeField] private WorldEventManager worldEventManager;

        /// <summary>
        /// 初始化时获取WorldEventManager实例
        /// （注意：FindObjectOfType有性能开销，建议在编辑器拖拽赋值）
        /// </summary>
        private void Awake()
        {
            worldEventManager = FindObjectOfType<WorldEventManager>();

            // 安全校验
            if (worldEventManager == null)
            {
                Debug.LogError("未找到WorldEventManager实例！", this);
            }
        }

        /// <summary>
        /// 当其他碰撞体进入触发器时调用
        /// </summary>
        /// <param name="other">进入触发器的碰撞体</param>
        private void OnTriggerEnter(Collider other)
        {
            // 检查碰撞对象是否是玩家
            if (other.CompareTag("Player")) // 比直接使用tag更高效
            {
                // 验证事件管理器是否有效
                if (worldEventManager != null)
                {
                    worldEventManager.ActivateBossFight();
                }
                else
                {
                    Debug.LogWarning("尝试激活Boss战但WorldEventManager为空", this);
                }

                // 可选：禁用触发器避免重复触发
                // GetComponent<Collider>().enabled = false;
            }
        }
    }
}