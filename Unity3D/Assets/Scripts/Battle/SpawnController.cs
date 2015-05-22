﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using System;

public class SpawnController : MonoBehaviour
{
    private PoolManager poolManager;        // 物件池
    private GameObject clone;               // CopyCat
    private HoleState holeState;            // 地洞狀態

    private string holeRoot;                // 地洞的路徑值
    private int holeIndex;                  // 地洞Index

    private int[] arrLine;                  // 直線陣列
    private int[] arrOppositeLine;          // 反向直線陣列
    private int[] arrHorizontal;            // 水平陣列
    private int[] arrCircle;                // 圓圈陣列
    private int[] arrOppositeCircle;        // 反向圓圈陣列

    public GameObject[] Hole;               // 存放場景每個地洞
    public GameObject myPanel;              // Battle Panel

    public int holeLimit;                   // 地洞上限
    public int spawnCount;                  // 預設產生數量
    public float speed;                     // 速度
    public int NGUIDepth;                   // 物件深度

    public bool flag;                      // 是否產生 基本鼠的Flag
    public bool testFlag;                   // grandmonther know it

    void Start()
    {
        poolManager = GetComponent<PoolManager>();
        Global.photonService.ApplyDamageEvent += OnApplyDamageEvent;
        flag = true;

        holeIndex = 0;
        Global.MiceCount = 0;

        holeRoot = "GameUI/Camera/Battle(Panel)/Hole";

        arrLine = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        arrOppositeLine = new int[] { 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        arrCircle = new int[] { 1, 2, 3, 6, 8, 10, 13, 11, 12, 9, 7, 4, 5 };
        arrOppositeCircle = new int[] { 1, 4, 7, 9, 12, 11, 13, 10, 8, 6, 3, 2, 5 };
        arrHorizontal = new int[] { 1, 4, 7, 9, 11, 12 };
    }

    void Update()
    {
        if (poolManager.mergeFlag && poolManager.poolingFlag && flag)       // 如果 物件池初始化完成 且 可以產生
        {
            Spawn((byte)SpawnStatus.Circle, 1, speed, spawnCount);          // 開始產生老鼠
            // Spawn((byte)SpawnStatus.OppositeCircle, 2, 0.5f, 20);
            //            Debug.Log("Start");
            flag = false;
        }


    }

    /// <summary>
    /// 基本老鼠產生器 (產生方式,老鼠ID,速度,數量)
    /// </summary>
    public void Spawn(byte status, int miceID, float speed, int amount)
    {
        switch (status)
        {
            case (byte)SpawnStatus.Circle:
                {
                    StartCoroutine(OneByOne(arrCircle, miceID, speed, amount));
                    break;
                }
            case (byte)SpawnStatus.OppositeCircle:
                {
                    StartCoroutine(OneByOne(arrOppositeCircle, miceID, speed, amount));
                    break;
                }
            case (byte)SpawnStatus.Line:
                {
                    Debug.Log("Status");
                    StartCoroutine(OneByOne(arrLine, miceID, speed, amount));
                    break;
                }
            case (byte)SpawnStatus.OppositeLine:
                {
                    StartCoroutine(OneByOne(arrOppositeLine, miceID, speed, amount));
                    break;
                }
            case (byte)SpawnStatus.Horizontal:
                {
                    StartCoroutine(OneByOne(arrHorizontal, miceID, speed, amount));
                    break;
                }
        }
    }


    /// <summary>
    /// 一個接一個產生
    /// </summary>
    /// <param name="arrHoleNum"></param>
    /// <param name="miceID"></param>
    /// <param name="speed"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    IEnumerator OneByOne(int[] arrHoleNum, int miceID, float speed, int amount)
    {
        for (int spawnCount = 0; spawnCount < amount; spawnCount++)
        {
            if (holeIndex < arrHoleNum.Length) // 這怪怪
            {
                //Debug.Log("holeIndex : " + holeIndex);
                // 如果 目前老鼠數量 < 地洞總數 且 地洞Index值沒有超過13    Hole.Length=14(含BOSS地洞) (holeIndex+1) 是因為起始值是0 mod 13 = 0!!
                if (Global.MiceCount < Hole.Length && (float)holeIndex / (float)holeLimit != 1)
                {
                    // 目前這一個地洞的狀態
                    holeState = GameObject.Find(holeRoot + arrHoleNum[holeIndex].ToString()).GetComponent<HoleState>();
                    try
                    {
                        // 如果地洞狀態是開啟的 才能產生老鼠
                        if (holeState.holeState == HoleState.State.Open)
                        {
                            GameObject mice;                                // 暫存老鼠物件
                            //Debug.Log("miceID" + miceID);
                            mice = poolManager.ActiveObject(miceID);        // 存入Active的老鼠

                            // 如果有Active老鼠
                            if (mice != null)
                            {
                                //  Debug.Log("mice != null");
                                holeState.holeState = HoleState.State.Closed;
                                mice.gameObject.transform.parent = Hole[arrHoleNum[holeIndex] - 1].transform;       // 我的上層 = 地洞 (因為如果直接給position amins不會跟著移動，必須把物件放入EmptyGameObject下)
                                mice.GetComponent<UISprite>().depth = NGUIDepth;
                                mice.GetComponent<EggMice>().Play();

                                Global.MiceCount++;
                                holeIndex++;
                            }
                            else
                            {
                                Debug.Log("Object Pool hasn't Mice");
                            }
                        }
                        else
                        {
                            holeIndex++; //防止地洞關閉了確不會尋找沒開的地洞
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                yield return new WaitForSeconds(speed); // 等待多久才會產生
            }
            else
            {
                holeIndex = 0;  // 防止意外發生
            }
        }
    }

    void OnApplyDamageEvent(int miceID)     // 收到技能攻擊 (目前是測試數值 扣分)
    {
       Spawn((byte)SpawnStatus.Horizontal, miceID, speed, 10);

    }
}