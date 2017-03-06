﻿using UnityEngine;
using System.Collections;

public abstract class MiceBossBase : Creature
{
    public static UICamera cam;
    public static BattleHUD battleHUD = null;
    protected int _shield = 0;
    protected int myHits, otherHits;              // 打擊紀錄
    protected float m_LastTime, m_StartTime;

    protected virtual void Awake()
    {
        if (battleHUD == null) battleHUD = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleHUD>();
        if (cam == null) cam = Camera.main.GetComponent<UICamera>();

        m_StartTime = m_LastTime = Time.time;

        Global.photonService.BossInjuredEvent += OnInjured;
        Global.photonService.LoadSceneEvent += OnDestory;   // 離開房間時


    }

    protected virtual void Update()
    {
        // 遊戲開始時
        if (Global.isGameStart)
        {
            battleHUD.ShowBossHPBar(m_Arribute.GetHPPrecent(), false);    // 顯示血調
            m_AnimState.UpdateAnimation();
            if (Time.time < m_StartTime + m_Skill.GetSkillTime())
                m_Skill.UpdateEffect();
            if (m_Arribute.GetHP() == 0) OnDead(0);
        }
        else
            gameObject.SetActive(false);
    }

    public abstract void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime);

    /// <summary>
    /// On Touch / On Click
    /// </summary>
    protected virtual void OnHit()
    {
        if (Global.isGameStart && ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled && m_Arribute.GetHP() != 0)
        {
            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.OnHit);

            if (m_Arribute.GetHP() - 1 == 0) GetComponent<BoxCollider2D>().enabled = false;

            if (m_Arribute.GetShield() == 0)
                Global.photonService.BossDamage(1);  // 傷害1是錯誤的 需要由Server判定、技能等級
            else
                m_Arribute.SetShield(m_Arribute.GetShield() - 1);
        }
    }


    /// <summary>
    /// 死亡
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected abstract void OnDead(float lifeTime);

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage"></param>
    protected override void OnInjured(short damage, bool myAttack)
    {
        m_Arribute.SetHP(Mathf.Max(0, m_Arribute.GetHP() - damage));
    }

    /// <summary>
    /// 銷毀時，移除事件
    /// </summary>
    protected virtual void OnDestory()
    {
        Global.photonService.BossInjuredEvent -= OnInjured;
        Global.photonService.LoadSceneEvent -= OnDestory;   // 離開房間時
    }

    public override void SetSkill(SkillBase skill)
    {
        if (this.m_Skill != null)
            this.m_Skill.Release();
        this.m_Skill = skill;
    }

    public override void SetState(AIState state)
    {
        if (this.m_AIState != null)
            this.m_AIState = null;
        this.m_AIState = state;
    }

    public override void SetArribute(CreatureAttr arribute)
    {
        if (this.m_Arribute != null)
            this.m_Arribute = null;
        this.m_Arribute = arribute;
    }

    public override void SetAnimState(AnimatorState state)
    {
        if (this.m_AnimState != null)
            this.m_AnimState = null;
        this.m_AnimState = state;
    }
}