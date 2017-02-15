﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using System;
using System.Linq;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 物件池 提供一個高效率的物件生成、回收方式
 * 231行，EggMice和ID必須由程式控制 現在亂寫
 * 170~ lerpSpeed等 錯誤 應由伺服器
 * 173、技能使用量 錯誤 應由伺服器
 * ***************************************************************/

public class PoolManager : MonoBehaviour
{
    private AssetLoader assetLoader;
    private ObjectFactory objFactory;
    private AttrFactory attrFactory;
    //private Dictionary<string, object> _tmpDict;
    private Dictionary<int, string> _dictMiceObject;
    private Dictionary<int, string> _dictSkillObject;
    //private HashSet<int> _myMice;
    //private HashSet<int> _otherMice;

    private GameObject clone;

    private float _lastTime;
    private float _currentTime;
    static float lerpSpeed = 0.1f, upSpeed = 6, upDantance = 60;    // 錯誤 數值不應該在這裡 應該在實體化時輸入

    Dictionary<string, object> _dictSkillMice;

    public GameObject Panel;

    [Tooltip("物件池位置")]
    public GameObject ObjectPool;

    [Tooltip("技能位置")]
    public GameObject skillArea;

    [Tooltip("產生數量")]
    [Range(3, 5)]
    public int spawnCount = 5;

    [Tooltip("物件池上限(各種類分開)")]
    [Range(3, 10)]
    public int clearLimit = 5;

    [Tooltip("物件池上限(各種類分開)")]
    [Range(10, 20)]
    public int clearTime = 10;

    [Tooltip("物件池保留量(各種類分開)")]
    [Range(2, 5)]
    public int reserveCount = 2;

    private bool _mergeFlag = false;          // 合併老鼠完成
    private bool _poolingFlag = false;        // 初始化物件池

    public bool mergeFlag { get { return _mergeFlag; } }
    public bool poolingFlag { get { return _poolingFlag; } }


    private Vector3 bossScale = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 skillScale = new Vector3(0.9f, 0.9f, 0.9f);

    void Awake()
    {
        assetLoader = gameObject.AddComponent<AssetLoader>();
        attrFactory = new AttrFactory();
        objFactory = new ObjectFactory();
        _lastTime = 0;
        _currentTime = 0;
        clearTime = 10;
        _poolingFlag = false;      // 初始化物件池
        _dictMiceObject = new Dictionary<int, string>();
        _dictSkillObject = new Dictionary<int, string>();
        //_myMice = new HashSet<int>();
        //_otherMice = new HashSet<int>();
        // _tmpDict = new Dictionary<string, object>();
        _dictSkillMice = new Dictionary<string, object>();
        MergeMice();                                // 將雙方的老鼠合併 剔除相同的老鼠
        LoadItem();
    }

    void Start()
    {
        assetLoader.init();
        LoadMiceAsset();
        LoadItemAsset();
        LoadEffectAsset();
    }

    private void LoadMiceAsset()
    {
        // 載入 老鼠資產
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            assetLoader.LoadAsset(item.Value + "/", item.Value);
        }

        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            assetLoader.LoadPrefab(item.Value + "/", item.Value);
        }
    }

    private void LoadItemAsset()
    {
        // 載入 道具資產
        assetLoader.LoadAsset("ItemICON" + "/", "ItemICON");

        foreach (KeyValuePair<int, string> item in _dictSkillObject)
        {
            assetLoader.LoadPrefab("ItemICON" + "/", item.Value + "ICON");
        }
    }

    private void LoadEffectAsset()
    {
        // 載入 特效資產

        assetLoader.LoadAsset("Effects" + "/", "Effects");

        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            int itemID = Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
            string itemName = ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString() + "ICON";

            assetLoader.LoadPrefab("Effects" + "/", itemName.Remove(itemName.Length - 4) + "Effect");
        }
    }

    void Update()
    {
        //if (!_poolingFlag && !string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //    Debug.Log(assetLoader.ReturnMessage);




        if (assetLoader.loadedObj && !_poolingFlag)
        {
            _dictSkillMice = Global.dictTeam;
            InstantiateObject(_dictMiceObject);
            InstantiateSkillMice(_dictSkillMice);
            _poolingFlag = true;
            //            Debug.Log("pooling Mice Completed ! ");
        }


        _currentTime = Time.time;

        // 定時清除物件池
        if (_currentTime - _lastTime > clearTime)     // 達到清除時間時
        {
            for (int i = 0; i < ObjectPool.transform.childCount; i++)       // 跑遍動態池
            {
                if (ObjectPool.transform.GetChild(i).childCount > clearLimit)           // 如果動態池超過限制數量
                {
                    for (int j = 0; j < ObjectPool.transform.GetChild(i).childCount - reserveCount; j++)    // 銷毀物件
                    {
                        Destroy(ObjectPool.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }
            _lastTime = _currentTime;
        }
    }
    /*
        List<float> GetMiceProperty(string miceName)
        {
            List<float> data = new List<float>();

            foreach (KeyValuePair<string, object> item in Global.miceProperty)
            {

            }

            return data;
        }
        */


    void InstantiateObject(Dictionary<int, string> objectData)
    {
        foreach (KeyValuePair<int, string> item in objectData)
        {
            GameObject bundle = assetLoader.GetAsset(item.Value);
            if (bundle != null) Instantiate(item.Key.ToString(), bundle);
        }
    }

    void InstantiateSkillMice(Dictionary<string, object> objectData)
    {

        int i = 0;
        float lerpSpeed = 0.1f;
        float upDistance = 30f;
        float energyValue = 0.2f;

        foreach (KeyValuePair<string, object> item in objectData)
        {
            GameObject bundle = assetLoader.GetAsset(item.Value.ToString());
            if (bundle != null)
            {
                Vector3 scale = Vector3.zero;
                Transform parent = skillArea.transform.GetChild(i);
                clone = new GameObject();

                scale = (i == 4) ? bossScale : skillScale;

                // instantiate skill btn
                clone = objFactory.Instantiate(bundle, parent, item.Key, Vector3.zero, scale, Vector2.zero, -1);
                clone.AddComponent<SkillBtn>();
                clone.GetComponent<SkillBtn>().init(Convert.ToInt16(item.Key), lerpSpeed * (i + 1), upDistance, energyValue * (i + 1));
                clone.AddComponent<UIButton>();
                //EventDelegate.Set(clone.GetComponent<UIButton>().onClick, clone.GetComponent<SkillBtn>().OnClick);
                

                // instantiate Item btn
                int itemID = Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
                string itemName = ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString() + "ICON";
                short skillID = Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "SkillID", itemID.ToString()));
                short skillType = Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.dictSkills, "SkillType", skillID.ToString()));

                if (assetLoader.GetAsset(itemName) != null)
                {
                    bundle = assetLoader.GetAsset(itemName);
                    clone = objFactory.Instantiate(bundle, parent, itemName.ToString(), Vector3.zero, Vector3.one, new Vector2(300, 300), -1);
                    clone.AddComponent<ItemBtn>();
                    clone.GetComponent<ItemBtn>().init(itemID.ToString(), skillType, lerpSpeed * (i + 1), upDistance, energyValue * (i + 1));
                    clone.AddComponent<BoxCollider2D>();
                    clone.GetComponent<BoxCollider2D>().size = new Vector2(300, 250);
                    clone.AddComponent<UIButton>();
                    clone.SetActive(false);
                    //EventDelegate.Set(clone.GetComponent<UIButton>().onClick, clone.GetComponent<ItemBtn>().OnClick);
                }

                parent.transform.gameObject.SetActive(true);
                i++;
            }
            else
            {
                Debug.LogError("bundle is null!");
            }

        }
    }

    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    /// </summary>
    /// <param name="objectID">使用Name找Object</param>
    /// <returns>回傳 ( GameObject / null )</returns>
    public GameObject ActiveObject(string objectID)
    {
        //Debug.Log("_dictObject.Count:" + _dictObject.Count);
        //int objectID = _dictObject.FirstOrDefault(x => x.Value == objectName).Key;       // 找Key

        if (ObjectPool.transform.FindChild(objectID).childCount == 0)
        {
            string bundleName = (string)ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemName", objectID.ToString());
            GameObject bundle = assetLoader.GetAsset(bundleName);
            if (bundle != null) Instantiate(objectID, bundle);
        }

        for (int i = 0; i < ObjectPool.transform.FindChild(objectID).childCount; i++)
        {
            GameObject clone = ObjectPool.transform.FindChild(objectID).GetChild(i).gameObject;
            MiceBase mice = clone.GetComponent(typeof(MiceBase)) as MiceBase;
            MiceAttr miceAttr = attrFactory.GetMiceProperty(objectID);
            mice.Initialize(lerpSpeed, upSpeed, upDantance, miceAttr.LifeTime);
            if (mice.enabled == false) mice.enabled = true;


            if (clone.name == objectID && !clone.gameObject.activeSelf)
            {
                miceAttr.SetHP(1);
                mice.SetArribute(miceAttr);
                mice.SetAnimState(new MiceAnimState(clone, false, lerpSpeed, upSpeed, upDantance, miceAttr.LifeTime));
                clone.SetActive(true);

                return clone;
            }
        }
        return null;
    }

    void Instantiate(string objectID, GameObject bundle)
    {
        Transform objGroup = ObjectPool.transform.FindChild(objectID);

        // 建立空GameObject群組
        if (objGroup == null)
        {
            clone = new GameObject();
            clone.name = objectID;
            clone.transform.parent = ObjectPool.transform;
            clone.layer = clone.transform.parent.gameObject.layer;
            clone.transform.localScale = Vector3.one;
            objGroup = clone.transform;
        }

        clone = objFactory.Instantiate(bundle, objGroup, objectID, Vector3.zero, Vector3.one, Vector2.zero, -1);
        MiceAttr miceAttr = attrFactory.GetMiceProperty(objectID);
        miceAttr.SetMaxHP(1);
        // 附加 Mice 並初始化
        Mice mice = clone.AddMissingComponent<Mice>();
        mice.Initialize(lerpSpeed, upSpeed, upDantance, miceAttr.LifeTime);

        // 設定數值
        mice.SetArribute(miceAttr);

        // 設定動畫
        mice.SetAnimState(new MiceAnimState(clone, false, lerpSpeed, upSpeed, upDantance, miceAttr.LifeTime));
        clone.gameObject.SetActive(false);    // 新版 子物件隱藏
    }






    public void MergeMice()
    {
        Dictionary<string, object> dictMyMice = Global.dictTeam;
        Dictionary<string, object> dictOtherMice = Global.OtherData.Team;

        foreach (KeyValuePair<string, object> item in dictMyMice)
        {
            _dictMiceObject.Add(ObjectFactory.GetIDFromName(Global.miceProperty, "MiceID", item.Value.ToString()), item.Value.ToString());
        }

        foreach (KeyValuePair<string, object> item in dictOtherMice)
        {
            if (!dictMyMice.ContainsValue(item.Value))
                _dictMiceObject.Add(ObjectFactory.GetIDFromName(Global.miceProperty, "MiceID", item.Value.ToString()), item.Value.ToString());
        }

        if (!_dictMiceObject.ContainsKey(10001))
            _dictMiceObject.Add(10001, "EggMice");
        //  Debug.Log(_dictObject);
        _mergeFlag = true;
    }


    private void LoadItem()
    {
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            int itemID = Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
            string itemName = ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
            _dictSkillObject.Add(itemID, itemName);
        }

    }
}
