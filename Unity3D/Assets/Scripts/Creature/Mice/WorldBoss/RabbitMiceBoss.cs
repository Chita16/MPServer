﻿using UnityEngine;
using System.Collections;
using MPProtocol;

public class RabbitMiceBoss  : MiceBossBase{

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        myHits = otherHits = 0;
        m_AIState = null;
        m_Arribute = null;
        m_Skill = null;
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;

        m_Skill.Display(gameObject, m_Arribute, m_AIState);
    }

    /// <summary>
    /// 受傷時
    /// </summary>
    /// <param name="damage">傷害</param>
    /// <param name="isMe">是否為自己攻擊</param>
    protected override void OnInjured(short damage, bool myAttack)
    {
        if (m_Arribute.GetShield() > 0)
        {
            m_Arribute.SetShield(m_Arribute.GetShield() - damage);
            Debug.Log("Hit Shield:" + m_Arribute.GetShield());
        }
        else
        {
            base.OnInjured(damage, myAttack);
        }

        if (m_Arribute.GetHP() != 0)
        {
            if (myAttack)
                myHits++;
            else
                otherHits++;
        }
        else
        {
            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.Die);
            if (Global.OtherData.RoomPlace != "Host")
            {
                short percent = (short)Mathf.Round((float)myHits / (float)(myHits + otherHits) * 100); // 整數百分比0~100% 目前是用打擊次數當百分比 如果傷害公式有變動需要修正
                Global.photonService.MissionCompleted((byte)Mission.WorldBoss, 1, percent, "");
            }
            transform.parent.GetComponentInChildren<Animator>().Play("HoleScale_R");
        }
    }

    /// <summary>
    /// 死亡時時
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected override void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            // 關閉血調顯示
            battleHUD.ShowBossHPBar(m_Arribute.GetHPPrecent(), true);

            Global.MiceCount--;
            Global.dictBattleMice.Remove(transform);
            OnDestory();
            Destroy(gameObject);
        }
    }
}