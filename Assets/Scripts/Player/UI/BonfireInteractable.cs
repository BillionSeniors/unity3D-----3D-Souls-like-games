using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QHZ
{
    public class BonfireInteractable : Interactable
    {
        public WorldSaveBonfire saveBonfire; // 篝火保存管理器
        public WorldSaveGameManager gameManager; // 游戏保存管理器
        _BonfireTeleportUI _bonfireTeleportUI; // 篝火传送UI
        public CharacterBonfireSave characterBonfireSave; // 角色篝火保存

        // 篝火传送位置
        [Header("篝火传送位置")]
        public Transform bonfireTeleportTransform;

        // 篝火激活状态
        [Header("激活状态")]
        public bool hasBeenActivated; // 是否已激活
        public int bonfireID; // 篝火唯一ID

        // 篝火唯一ID（用于保存已激活的篝火）
        [Header("篝火状态")]
        public bool bonfire1; // 篝火1
        public bool bonfire2; // 篝火2
        public bool bonfire3; // 篝火3
        public bool bonfire4; // 篝火4
        public bool bonfire5; // 篝火5

        // 篝火特效
        [Header("篝火特效")]
        public ParticleSystem activationFX; // 激活特效
        public ParticleSystem fireFX; // 火焰特效
        public AudioClip bonfireActivationSoundFX; // 激活音效

        public AudioSource audioSource; // 音效播放器

        private bool canTeleport = false; // 新增变量，控制是否可以传送

        protected override void Awake()
        {
            // 如果当前角色保存数据中没有该篝火的记录，则添加
            if (!WorldSaveGameManager.instance.currentCharacterSaveData.bonfireInWorld.ContainsKey(bonfireID))
            {
                WorldSaveGameManager.instance.currentCharacterSaveData.bonfireInWorld.Add(bonfireID, false);
            }

            // 从保存数据中获取篝火激活状态
            hasBeenActivated = WorldSaveGameManager.instance.currentCharacterSaveData.bonfireInWorld[bonfireID];

            // 如果篝火已被激活，加载时播放火焰特效
            if (hasBeenActivated)
            {
                // 如果篝火已被激活，加载时播放火焰特效
                fireFX.gameObject.SetActive(true);
                fireFX.Play();
                interactableText = "按[F]传送";
                canTeleport = true; // 已激活的篝火可以直接传送
            }
            else
            {
                interactableText = "按[F]点燃篝火";
                canTeleport = false; // 未激活的篝火不能传送
            }

            // 初始化组件
            audioSource = GetComponent<AudioSource>(); // 获取音效播放器
            _bonfireTeleportUI = FindObjectOfType<_BonfireTeleportUI>(); // 查找篝火传送UI
            saveBonfire = FindObjectOfType<WorldSaveBonfire>(); // 查找篝火保存管理器
            gameManager = FindObjectOfType<WorldSaveGameManager>(); // 查找游戏保存管理器
            characterBonfireSave = FindObjectOfType<CharacterBonfireSave>(); // 查找角色篝火保存
        }

        protected override void Start()
        {
            // 初始化篝火传送位置和交互对象
            characterBonfireSave.bonfireTeleportTransform = FindObjectOfType<_BonfireTeleportTransform>();
            characterBonfireSave.interactableBonfire = FindObjectOfType<BonfireInteractable>();
        }

        public override void Interact(PlayerManager playerManager)
        {
            if (hasBeenActivated && canTeleport) // 添加canTeleport条件
            {
                // 如果篝火已激活且可以传送，打开传送菜单
                _bonfireTeleportUI.bonfireMenu_UI.SetActive(true);
                _bonfireTeleportUI.hudWindow.SetActive(false);
            }
            else if (!hasBeenActivated)
            {
                // 激活篝火
                playerManager.playerAnimatorManager.PlayTargetAnimation(playerManager.playerAnimatorManager.animation_bonfire_activate, true);
                playerManager.uiManager.ActivateBonfirePopUp();

                if (WorldSaveGameManager.instance.currentCharacterSaveData.bonfireInWorld.ContainsKey(bonfireID))
                {
                    WorldSaveGameManager.instance.currentCharacterSaveData.bonfireInWorld.Remove(bonfireID);
                }

                WorldSaveGameManager.instance.currentCharacterSaveData.bonfireInWorld.Add(bonfireID, true);

                hasBeenActivated = true;
                // 移除直接设置interactableText的代码
                activationFX.gameObject.SetActive(true);
                activationFX.Play();
                gameManager.SaveGame();
                fireFX.gameObject.SetActive(true);
                fireFX.Play();
                audioSource.PlayOneShot(bonfireActivationSoundFX);

                // 添加协程延迟设置传送文本
                StartCoroutine(DelayTeleportText());
            }
        }

        private IEnumerator DelayTeleportText()
        {
            yield return new WaitForSeconds(3f);
            interactableText = "按[F]传送";
            canTeleport = true; // 3秒后允许传送
        }
    }
}