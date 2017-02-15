﻿using UnityEngine;
using System.Collections;

public class Mice : MiceBase
{
    private BattleManager battleManager;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();

        // m_AIState = null;
        // m_Arribute = null;
        // m_AnimState = null;

        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void OnEnable()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        _lastTime = Time.fixedTime; // 出生時間
    }


    public void Update()
    {
        if (Global.isGameStart)
        {
            m_AnimState.UpdateAnimation();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 擊中時
    /// </summary>
    protected override void OnHit()
    {
        //        Debug.Log("Hit1");
        if (Global.isGameStart && enabled && m_Arribute.GetHP() > 0)
        {
            // Debug.Log("Hit2");
            OnInjured(1);
            Global.dictBattleMice.Remove(transform.parent);
            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.Die);
        }
        else
        {
            Debug.Log("enabled: " + enabled + "   Collider: " + GetComponent<BoxCollider2D>().enabled + "  m_Arribute.GetHP(): " + m_Arribute.GetHP());
        }
    }


    /// <summary>
    /// 死亡時
    /// </summary>
    /// <param name="lifeTime">存活時間上限</param>
    protected override void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            if (m_Arribute.GetHP() == 0)
                battleManager.UpadateScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數
            else
                battleManager.LostScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數
            Global.dictBattleMice.Remove(transform.parent);
            Global.MiceCount--;

            gameObject.SetActive(false);
            this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
        }
    }

    public override void OnEffect(string name, object value)
    {
        if (name == "Scorched")
            OnHit();
        if (name == "Snow")
            m_AnimState.SetMotion((bool)value);
    }
}
