using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QHZ
{
    /// <summary>
    /// BOSS角色管理器，负责处理BOSS特有的逻辑
    /// </summary>
    public class AICharacterBossManager : MonoBehaviour
    {
        [Header("基础引用")]
        public AICharacterManager enemy; // 关联的AI角色管理器
        public string bossName; // BOSS名称（用于UI显示）
        
        [Header("UI组件")]
        public UIBossHealthBar bossHealthBar; // BOSS血条UI
        public BossCombatStanceState bossCombatStanceState; // BOSS战斗状态控制器

        [Header("第二阶段特效")]
        public GameObject particleFX; // 通用粒子特效
        public GameObject _activated_second_phase_particle_skill_1; // 第二阶段技能1特效预制体
        public Transform _activated_second_phase_particle_skill_1_transform; // 技能1特效生成位置

        /// <summary>
        /// 初始化组件引用
        /// </summary>
        private void Awake()
        {
            // 获取当前游戏对象上的AI角色管理器
            enemy = GetComponent<AICharacterManager>();
            
            // 查找场景中的BOSS血条UI
            bossHealthBar = FindObjectOfType<UIBossHealthBar>();
            
            // 获取子物体中的BOSS战斗状态组件
            bossCombatStanceState = GetComponentInChildren<BossCombatStanceState>();
        }

        /// <summary>
        /// 游戏开始时初始化BOSS设置
        /// </summary>
        private void Start()
        {
            // 设置BOSS名称和最大生命值
            bossHealthBar.SetBossName(bossName);
            bossHealthBar.SetBossMaxHealth(enemy.aiCharacterStatsManager.maxHealth);

            // 如果是BOSS角色，设置特殊属性
            if (enemy.aiCharacterStatsManager.isBoss)
            {
                // 增加护甲韧性
                enemy.aiCharacterStatsManager.armorPoiseBonus = 750;
                
                // 设置初始Y轴速度
                enemy.aiCharacterLocomotionManager.yVelocity.y = enemy.aiCharacterLocomotionManager.groundYVelocity;
            }
        }

        /// <summary>
        /// 更新BOSS血条显示
        /// </summary>
        /// <param name="currentHealth">当前生命值</param>
        /// <param name="maxHealth">最大生命值</param>
        public void UpdateBossHealthBar(float currentHealth, float maxHealth)
        {
            // 更新血条UI显示
            bossHealthBar.SetBossCurrentHealth(currentHealth);

            // 检查是否进入第二阶段（生命值低于25%且未转换过阶段）
            if (bossCombatStanceState != null)
            {
                if (currentHealth <= maxHealth / 4 && !bossCombatStanceState.hasPhaseShifted)
                {
                    ShiftToSecondPhase_BOSS_AI();
                }
            }
        }

        /// <summary>
        /// 转换到BOSS第二阶段
        /// </summary>
        public void ShiftToSecondPhase_BOSS_AI()
        {
            // 恢复满血
            enemy.aiCharacterStatsManager.currentHealth = enemy.aiCharacterStatsManager.maxHealth;
            
            // 标记已转换阶段
            bossCombatStanceState.hasPhaseShifted = true;
            
            // 设置动画参数
            enemy.animator.SetBool("isInvulnerable", true); // 无敌状态
            enemy.animator.SetBool("isPhaseShifting", true); // 阶段转换状态
            
            // 播放第二阶段转换动画
            enemy.aiCharacterAnimatorManager.PlayTargetAnimation(
                enemy.aiCharacterCombatManager.boss_animation_second_phase, 
                true);

            // 生成第二阶段技能特效
            if (_activated_second_phase_particle_skill_1 != null && 
                _activated_second_phase_particle_skill_1_transform != null)
            {
                GameObject skill_01_effect = Instantiate(
                    _activated_second_phase_particle_skill_1, 
                    _activated_second_phase_particle_skill_1_transform);
            }
        }
    }
}