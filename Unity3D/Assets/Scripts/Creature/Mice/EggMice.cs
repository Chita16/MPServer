﻿using UnityEngine;
using System.Collections;

public class EggMice : MiceBase
{

    public void GetSkill()
    {
        Debug.Log(m_Skill);
    }

    public void GetAttribute()
    {
        Debug.Log(m_Arribute.GetShield());
    }

    public void Test()
    {
        m_Skill.Display(gameObject, m_Arribute, m_AIState);
    }

    public override void SetState(AIState state)
    {
        throw new System.NotImplementedException();
    }

    public override void SetArribute(CreatureAttr arribute)
    {
        m_Arribute = arribute;
    }

    protected override void OnInjured(short damage)
    {
        throw new System.NotImplementedException();
    }

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnHit()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDead(float lifeTime)
    {
        throw new System.NotImplementedException();
    }
}
