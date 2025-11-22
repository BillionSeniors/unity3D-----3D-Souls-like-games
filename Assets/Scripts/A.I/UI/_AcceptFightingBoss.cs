using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace QHZ
{
    /// <summary>
    /// BOSS战交互系统 - 处理玩家触发BOSS战的完整流程
    /// 包含UI立即关闭、防重复触发、特效管理等功能
    /// </summary>
    public class _AcceptFightingBoss : Interactable
    {
        [Header("事件管理")]
        public WorldEventManager worldEventManager;

        [Header("战斗状态")]
        [SerializeField] public bool _acceptFightingBool;
        [SerializeField] private bool isInteracting = false;

        [Header("交互设置")]
        [Tooltip("交互冷却时间（秒）")]
        public float interactionCooldown = 2f;
        [Tooltip("UI淡出时间（秒）")]
        public float uiFadeOutDuration = 0.3f;

        [Header("调试")]
        public bool enableDebugLogs = true;

        #region 初始化
        protected override void Awake()
        {
            base.Awake();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (worldEventManager == null)
            {
                worldEventManager = FindObjectOfType<WorldEventManager>();
                if (worldEventManager == null && enableDebugLogs)
                {
                    Debug.LogError("世界事件管理器未找到!", this);
                }
            }
        }
        #endregion

        #region 主要交互逻辑
        public override void Interact(PlayerManager playerManager)
        {
            if (!CanInteract()) return;

            base.Interact(playerManager);
            StartInteraction(playerManager);
        }

        private bool CanInteract()
        {
            if (isInteracting)
            {
                if (enableDebugLogs) Debug.Log("交互正在冷却中", this);
                return false;
            }
            return true;
        }

        private void StartInteraction(PlayerManager playerManager)
        {
            isInteracting = true;
            if (enableDebugLogs) Debug.Log("开始BOSS战交互", this);

            // 立即隐藏UI
            playerManager.StartCoroutine(HideAllInteractionUI(playerManager));

            // 启动BOSS战流程
            StartCoroutine(BossFightSequence(playerManager));
        }
        #endregion

        #region UI控制
        private IEnumerator HideAllInteractionUI(PlayerManager playerManager)
        {
            if (enableDebugLogs) Debug.Log("正在隐藏交互UI", this);

            // 同时淡出两个UI元素
            Coroutine fadeInteractable = playerManager.StartCoroutine(
                FadeOutUI(playerManager.interactableUIGameObject));
            Coroutine fadeItem = playerManager.StartCoroutine(
                FadeOutUI(playerManager.itemInteractableGameObject));

            yield return fadeInteractable;
            yield return fadeItem;
        }

        private IEnumerator FadeOutUI(GameObject uiElement)
        {
            if (uiElement == null || !uiElement.activeSelf) yield break;

            CanvasGroup group = uiElement.GetComponent<CanvasGroup>();
            if (group == null) group = uiElement.AddComponent<CanvasGroup>();

            float startAlpha = group.alpha;
            float timer = 0f;

            while (timer < uiFadeOutDuration)
            {
                group.alpha = Mathf.Lerp(startAlpha, 0f, timer / uiFadeOutDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            uiElement.SetActive(false);
            group.alpha = startAlpha; // 重置alpha值以便下次使用
        }
        #endregion

        #region BOSS战流程
        private IEnumerator BossFightSequence(PlayerManager playerManager)
        {
            // 阶段1：激活战斗状态
            ActivateBossFight();

            // 阶段2：短暂延迟确保事件顺序
            yield return new WaitForSeconds(0.35f);

            // 阶段3：启动音乐和血条
            StartBossMusicAndHealthBar();

            // 重置交互状态（带冷却时间）
            yield return new WaitForSeconds(interactionCooldown);
            isInteracting = false;

            if (enableDebugLogs) Debug.Log("BOSS战交互流程完成", this);
        }

        private void ActivateBossFight()
        {
            _acceptFightingBool = true;
            worldEventManager.bossHasBeenAwakened = true;

            if (enableDebugLogs) Debug.Log("BOSS战已激活", this);
        }

        private void StartBossMusicAndHealthBar()
        {
            worldEventManager.StartBossMusic();

            if (worldEventManager.bossHasBeenAwakened &&
                worldEventManager.bossHealthBar != null)
            {
                worldEventManager.bossHealthBar.SetUIHealthBarToActive();
            }
            else if (enableDebugLogs)
            {
                Debug.LogWarning("无法显示BOSS血条: " +
                    (worldEventManager.bossHasBeenAwakened ? "血条引用为空" : "BOSS未觉醒"),
                    this);
            }
        }
        #endregion

        #region 辅助功能
        // 外部调用的强制重置方法
        public void ResetInteractionState()
        {
            isInteracting = false;
            StopAllCoroutines();

            if (enableDebugLogs) Debug.Log("交互状态已强制重置", this);
        }

        // 在禁用时自动清理
        private void OnDisable()
        {
            ResetInteractionState();
        }
        #endregion
    }
}