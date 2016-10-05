﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 開啟/關閉/載入 Team的的所有處理
 * NGUI BUG : Team交換時Tween會卡色
 * + pageVaule 還沒加入翻頁值
 * ***************************************************************
 *                           ChangeLog
 * 20160914 v1.0.1b  2次重購，獨立實體化物件  
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/

public class TeamManager : MonoBehaviour
{
    #region 欄位
    private AssetLoader assetLoader;
    public GameObject[] infoGroupsArea;                                             // 物件群組位置
    /// <summary>
    /// MiceIcon名稱、Mice按鈕
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedMice { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    /// <summary>
    /// TeamIcon名稱、Mice按鈕索引物件
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedTeam { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    [Range(0.2f, 0.4f)]
    public float delayBetween2Clicks = 0.3f;                                        // Change value in editor
    public Vector3 actorScale;

    private GameObject _clone, _miceActor, _tmpActor, _btnClick, _doubleClickChk;   // 克隆、老鼠角色、暫存角色、按下按鈕、雙擊檢查
    private static Dictionary<string, object> _dictMiceData, _dictTeamData;         // Json老鼠、隊伍資料
    private Dictionary<string, GameObject> _dictActor;                              // 已載入角色參考

    private int _page;                                                             // 材質數量、物件數量、翻一頁+10
    private float _lastClickTime;
    private bool _diableBtn, _LoadedIcon, _LoadedActor;
    #endregion

    void Awake()
    {
        Debug.Log("AAAAAAAAAAA  " + Global.MiceAll);
        actorScale = new Vector3(0.8f, 0.8f, 1);
        _page = 0;
        _miceActor = infoGroupsArea[1].transform.GetChild(0).gameObject;    // 方便程式辨認用 infoGroupsArea[1].transform.GetChild(0).gameObject = image
        dictLoadedMice = new Dictionary<string, GameObject>();
        dictLoadedTeam = new Dictionary<string, GameObject>();
        _dictMiceData = Json.Deserialize(Global.MiceAll) as Dictionary<string, object>;
        _dictTeamData = Json.Deserialize(Global.Team) as Dictionary<string, object>;
        _dictActor = new Dictionary<string, GameObject>();

        assetLoader = gameObject.AddComponent<AssetLoader>();
    }

    void Update()
    {
        if (!_LoadedActor || !_LoadedIcon)                                          // 除錯訊息
            if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
                Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (assetLoader.loadedObj && !_LoadedIcon)
        {
            _LoadedIcon = !_LoadedIcon;
            assetLoader.init();
            InstantiateIcon(_dictMiceData, infoGroupsArea[0].transform);
            InstantiateIcon(_dictTeamData, infoGroupsArea[2].transform);
            HideMice();
        }

        if (assetLoader.loadedObj && !_LoadedActor)
        {
            _LoadedActor = !_LoadedActor;
            assetLoader.init();
            InstantiateActor();
        }
    }

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnMiceClick(GameObject btn_mice)
    {
        if (Time.time - _lastClickTime < delayBetween2Clicks && _doubleClickChk == btn_mice)    // Double Click
            btn_mice.SendMessage("Mice2Click");
        else
            StartCoroutine(OnClickCoroutine(btn_mice));

        _lastClickTime = Time.time;
        _doubleClickChk = btn_mice;
    }

    IEnumerator OnClickCoroutine(GameObject btn_mice)
    {
        yield return new WaitForSeconds(delayBetween2Clicks);

        if (Time.time - _lastClickTime < delayBetween2Clicks)
            yield break;

        LoadActor(btn_mice);
        Debug.Log(btn_mice.transform.GetChild(0).name);
        LoadProperty loadProerty = new LoadProperty();
        loadProerty.LoadMiceProperty(btn_mice.transform.GetChild(0).gameObject, infoGroupsArea[1], 0);
        //Debug.Log("Simple click");
    }
    #endregion

    #region -- OnMessage 載入老鼠(外部呼叫) -- 這裡未來會錯 如果載入超過1張老鼠Icon圖
    void OnMessage()
    {
        assetLoader.LoadAsset("MiceICON/", "MiceICON"); // 載入老鼠資產
        LoadIconObject(_dictMiceData);  // 載入老鼠物件
    }
    #endregion

    #region -- LoadIconObject 載入老鼠物件 --
    void LoadIconObject(Dictionary<string, object> dictionary)    // 載入遊戲物件
    {
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            assetLoader.LoadPrefab("MiceICON/", item.Value.ToString() + "ICON");
        }
    }
    #endregion

    #region -- InstantiateIcon 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateIcon(Dictionary<string, object> dictionary, Transform myParent)
    {
        int i = 0;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            string bundleName = item.Value.ToString() + "ICON";
            if (assetLoader.GetAsset("MiceICON/", bundleName))                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset("MiceICON/", bundleName);
                Transform miceBtn = myParent.GetChild(i);

                InstantiateObject insObj = new InstantiateObject();
                _clone = insObj.Instantiate(bundle, miceBtn, item.Value.ToString(), Vector3.zero, Vector3.one, Vector2.zero, -1);

                Add2Refs(bundle, miceBtn);     // 加入物件參考

                miceBtn.GetComponent<TeamSwitcher>().enabled = true;           // 開啟老鼠隊伍交換功能
                miceBtn.GetComponent<TeamSwitcher>().SendMessage("EnableBtn"); // 開啟按鈕功能

                i++;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _LoadedIcon = true;
    }
    #endregion

    #region -- LoadActor 載入老鼠角色 --
    private void LoadActor(GameObject btn_mice)
    {
        GameObject _miceImage;
        string miceName = btn_mice.transform.GetChild(0).name;

        _btnClick = btn_mice;

        if (_tmpActor != null) _tmpActor.SetActive(false);          // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

        if (_dictActor.TryGetValue(miceName, out _miceImage))       // 假如已經載入老鼠圖片了 直接顯示
        {
            _miceImage.SetActive(true);
            _tmpActor = _miceImage;
        }
        else
        {
            string key = (int.Parse(btn_mice.name.Remove(0, 4)) + _page).ToString(); // + pageVaule 還沒加入翻頁值
            string assetName = btn_mice.transform.GetChild(0).name;
            if (assetLoader.GetAsset(assetName + "/", assetName) != null)
            {
                InstantiateActor();
            }
            else
            {
                _LoadedActor = false;
                assetLoader.LoadAsset(assetName + "/", assetName);
                assetLoader.LoadPrefab(assetName + "/", assetName);
            }

            //LoadMiceAsset(btn_mice);
        }
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    private void InstantiateActor()
    {
        GameObject _tmp;
        string miceName = _btnClick.transform.GetChild(0).name;
        InstantiateObject insObj = new InstantiateObject();
        GameObject bundle = (GameObject)assetLoader.GetAsset(miceName + "/", miceName);

        if (bundle != null)                  // 已載入資產時
        {
            _clone = insObj.InstantiateActor(bundle, _miceActor.transform, miceName, actorScale);
        }
        else
        {
            Debug.LogError("Assetbundle reference not set to an instance. at InstantiateActor.");
        }

        Destroy(_clone.transform.GetChild(0).GetComponent(_clone.name));    // 刪除Battle用腳本

        if (_dictActor.TryGetValue(_clone.name, out _tmp) != false)
            _dictActor.Add(_clone.name, _clone);

        _tmpActor = _clone;
        _LoadedActor = true;

    }
    #endregion

    #region -- HideMice 隱藏老鼠 --
    void HideMice() // 把按鈕變成無法使用 如果老鼠已Team中
    {
        foreach (KeyValuePair<string, object> item in _dictTeamData)
        {
            if (dictLoadedMice.ContainsKey(item.Value.ToString()))
                dictLoadedMice[item.Value.ToString()].SendMessage("DisableBtn");
        }
    }
    #endregion

    #region -- Add2Refs 加入老鼠參考 --
    /// <summary>
    /// 加入老鼠參考
    /// </summary>
    /// <param name="bundle">AssetBundle</param>
    /// <param name="myParent">參考按鈕</param>
    void Add2Refs(GameObject bundle, Transform myParent)
    {
        string btnArea = myParent.parent.name;                          //按鈕存放區域名稱 Team / Mice 區域
        string miceName = bundle.name.Remove(bundle.name.Length - 4) + "Mice";

        if (btnArea == "Mice")
            dictLoadedMice.Add(miceName, myParent.gameObject);          // 加入索引 老鼠所在的MiceBtn位置
        else
            dictLoadedTeam.Add(miceName, GetLoadedMice(miceName));      // 參考至 老鼠所在的MiceBtn位置
    }
    #endregion

    #region --字典 檢查/取值 片段程式碼 --
    public bool bLoadedTeam(string miceName)
    {
        GameObject obj;
        return dictLoadedTeam.TryGetValue(miceName, out obj);
    }

    public bool bLoadedMice(string miceName)
    {
        GameObject obj;
        return dictLoadedMice.TryGetValue(miceName, out obj);
    }

    public GameObject GetLoadedMice(string miceName)
    {
        GameObject obj;
        if (dictLoadedMice.TryGetValue(miceName, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string miceName)
    {
        GameObject obj;
        if (dictLoadedTeam.TryGetValue(miceName, out obj))
            return obj;
        return null;
    }
    #endregion
}