﻿using UnityEngine;
using System.Collections;

public abstract class SkillPlayer : SkillBase
{
    protected int useTimes = 0;

    public SkillPlayer(SkillAttr skill)
        : base(skill)
    {
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        throw new System.NotImplementedException();
    }

    public override void Display()
    {
        throw new System.NotImplementedException();
    }
}