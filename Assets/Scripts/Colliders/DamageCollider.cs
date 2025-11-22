using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QHZ
{
    public class DamageCollider : MonoBehaviour
    {
        // 引用角色管理器，用于处理攻击者和受击者的交互
        public CharacterManager characterManager;
        // 伤害碰撞器组件
        protected Collider damageCollider;
        // 是否在启动时自动启用碰撞器
        public bool enableDamageColliderOnStartUp = false;

        [Header("队伍编号")]
        public int teamIDNumber = 0; // 队伍ID，用于区分敌我

        [Header("韧性相关")]
        public float poiseDamage; // 削韧值（影响角色的硬直）
        public float offensivePoiseBonus; // 攻击方的额外削韧加成

        [Header("伤害类型")]
        public int physicalDamage; // 物理伤害
        public int fireDamage; // 火焰伤害
        public int _magicDamage; // 魔法伤害
        public int _lightningDamage; // 闪电伤害
        public int _darkDamage; // 暗影伤害
        public int _bleedDamage; // 出血伤害

        [Header("破防系数")]
        public float guardBreakModifier = 1; // 破防系数（影响防御时的耐力消耗）

        // 状态标志
        protected bool shieldHasBeenHit; // 是否击中盾牌
        protected bool hasBeenParried; // 是否被格挡
        protected string currentDamageAnimation; // 当前播放的受击动画名称
        public bool _lifeSteal_enable; // 是否启用吸血效果

        // 碰撞信息
        protected Vector3 contactPoint; // 碰撞点坐标
        protected float angleHitFrom; // 攻击角度（用于受击方向判定）

        // 存储本次攻击计算中已伤害过的角色，避免重复计算
        private List<CharacterManager> charactersDamagedDuringThisCalculation = new List<CharacterManager>();

        // 初始化碰撞器
        protected virtual void Awake()
        {
            damageCollider = GetComponent<Collider>();
            damageCollider.gameObject.SetActive(true);
            damageCollider.isTrigger = true; // 设为触发器
            damageCollider.enabled = enableDamageColliderOnStartUp; // 按配置启用
        }

        // 启用碰撞器
        public void EnableDamageCollider()
        {
            damageCollider.enabled = true;
        }

        // 禁用碰撞器，并清空已伤害角色列表
        public void DisableDamageCollider()
        {
            if (charactersDamagedDuringThisCalculation.Count > 0)
            {
                charactersDamagedDuringThisCalculation.Clear();
            }
            damageCollider.enabled = false;
        }

        // 触发器进入事件
        protected virtual void OnTriggerEnter(Collider collision)
        {
            // 碰撞对象是可伤害角色（层检查）
            if (collision.gameObject.layer == LayerMask.NameToLayer("Damageable Character"))
            {
                shieldHasBeenHit = false;
                hasBeenParried = false;

                // 获取受击角色的管理器
                CharacterManager enemyManager = collision.GetComponentInParent<CharacterManager>();

                if (enemyManager != null)
                {
                    AICharacterManager aiCharacter = enemyManager as AICharacterManager;

                    // 避免重复伤害
                    if (charactersDamagedDuringThisCalculation.Contains(enemyManager))
                        return;

                    charactersDamagedDuringThisCalculation.Add(enemyManager);

                    // 同队豁免
                    if (enemyManager.characterStatsManager.teamIDNumber == teamIDNumber)
                        return;

                    // 检查格挡和防御
                    CheckForParry(enemyManager);
                    CheckForBlock(enemyManager);

                    // 如果被格挡或击中盾牌，终止伤害计算
                    if (hasBeenParried || shieldHasBeenHit)
                        return;

                    // 更新受击角色的硬直状态
                    enemyManager.characterStatsManager.poiseResetTimer = enemyManager.characterStatsManager.totalPoiseResetTime;
                    enemyManager.characterStatsManager.totalPoiseDefence -= poiseDamage;

                    // 记录碰撞点和攻击角度
                    contactPoint = collision.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                    angleHitFrom = Vector3.SignedAngle(
                        characterManager.transform.forward,
                        enemyManager.transform.forward,
                        Vector3.up
                    );

                    // 处理伤害和特效
                    DealDamage(enemyManager);
                    _CheckForWeaponDamageEffectType();

                    _lifeSteal_enable = true; // 触发吸血标记

                    // 如果是AI，将其目标设为攻击者
                    if (aiCharacter != null)
                    {
                        aiCharacter.currentTarget = characterManager;
                    }
                }
            }

            // 碰撞对象是幻影墙（隐藏墙壁）
            if (collision.tag == "Illusionary Wall")
            {
                IllusionaryWall illusionaryWall = collision.GetComponent<IllusionaryWall>();
                illusionaryWall.wallHasBeenHit = true; // 标记墙壁已被击中
            }
        }

        // 检查是否被格挡
        protected virtual void CheckForParry(CharacterManager enemyManager)
        {
            if (enemyManager.isParrying)
            {
                // 播放被格挡动画
                characterManager.GetComponentInChildren<CharacterAnimatorManager>()
                    .PlayTargetAnimation(enemyManager.characterCombatManager.weaponArt_Parried, true);
                hasBeenParried = true;
            }
        }

        // 检查是否击中盾牌
        protected virtual void CheckForBlock(CharacterManager enemyManager)
        {
            Vector3 directionFromPlayerToEnemy = (characterManager.transform.position - enemyManager.transform.position);
            float dotValueFromPlayerToEnemy = Vector3.Dot(directionFromPlayerToEnemy, enemyManager.transform.forward);

            // 如果对方正在防御且攻击方向在正面范围内
            if (enemyManager.isBlocking && dotValueFromPlayerToEnemy > 0.3f)
            {
                shieldHasBeenHit = true;

                // 生成防御受击特效
                TakeBlockedDamageEffect takeBlockedDamage = Instantiate(
                    WorldCharacterEffectsManager.instance.takeBlockedDamageEffect
                );
                // 传递伤害数据
                takeBlockedDamage.physicalDamage = physicalDamage;
                takeBlockedDamage.fireDamage = fireDamage;
                takeBlockedDamage._lightningDamage = _lightningDamage;
                takeBlockedDamage._darkDamage = _darkDamage;
                takeBlockedDamage._magicDamage = _magicDamage;
                takeBlockedDamage._bleedDamage = _bleedDamage;
                takeBlockedDamage.poiseDamage = poiseDamage;
                takeBlockedDamage.staminaDamage = poiseDamage;

                // 应用特效
                enemyManager.characterEffectsManager.ProcessEffectInstantly(takeBlockedDamage);
            }
        }

        // 处理实际伤害计算
        protected virtual void DealDamage(CharacterManager enemyManager)
        {
            // 初始化最终伤害值
            float finalPhysicalDamage = physicalDamage;
            float finalFireDamage = fireDamage;
            // ...其他伤害类型类似

            // 根据攻击类型（轻/重击）和使用的武器（左/右手）应用伤害系数
            if (characterManager.isUsingRightHand)
            {
                if (characterManager.characterCombatManager.currentAttackType == AttackType.light)
                {
                    finalPhysicalDamage *= characterManager.characterInventoryManager.rightWeapon.lightAttackDamageModifier;
                    // ...其他伤害类型类似
                }
                else if (characterManager.characterCombatManager.currentAttackType == AttackType.heavy)
                {
                    finalPhysicalDamage *= characterManager.characterInventoryManager.rightWeapon.heavyAttackDamageModifier;
                    // ...其他伤害类型类似
                }
            }
            // 左手武器逻辑类似...

            // 生成受击特效并传递伤害数据
            TakeDamageEffect takeDamageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
            takeDamageEffect.physicalDamage = finalPhysicalDamage;
            takeDamageEffect.fireDamage = finalFireDamage;
            // ...其他伤害类型类似
            takeDamageEffect.contactPoint = contactPoint;
            takeDamageEffect.angleHitFrom = angleHitFrom;

            // 应用特效
            enemyManager.characterEffectsManager.ProcessEffectInstantly(takeDamageEffect);
        }

        // 检查武器附加效果（如出血）
        private void _CheckForWeaponDamageEffectType()
        {
            // 获取武器效果类型组件
            var weaponEffect = characterManager.characterInventoryManager.GetComponentInChildren<_WeaponEffectDamageType>();

            // 出血类型武器
            if (weaponEffect._effectWeaponDamageType == _EffectWeaponDamageType._BleedWeaponType)
            {
                foreach (CharacterManager character in charactersDamagedDuringThisCalculation)
                {
                    // 如果目标已处于出血状态则跳过
                    if (character.characterStatsManager._isBleeded)
                        return;

                    // 生成出血积累效果
                    _BleedBuildUpEffect _bleedBuildUp = Instantiate(WorldCharacterEffectsManager.instance._bleedBuildUpEffect);

                    // 避免重复添加同一效果
                    foreach (var effect in character.characterEffectsManager.timedEffects)
                    {
                        if (effect.effectID == _bleedBuildUp.effectID)
                        {
                            return;
                        }
                    }

                    // 添加效果到目标
                    character.characterEffectsManager.timedEffects.Add(_bleedBuildUp);
                }
            }
        }
    }
}