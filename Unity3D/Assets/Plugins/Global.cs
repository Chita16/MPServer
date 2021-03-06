﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MPProtocol;
using System.Linq;
using System.Diagnostics;

public static class Global
{
    public static readonly string serverPath = "http://49.159.128.17:58767/MicePow";//Server路徑
    //Android or iOS or Win 伺服器中的 檔案列表路徑
    public static readonly string serverListPath = serverPath +
#if UNITY_ANDROID
 "/AndroidList/";
#elif UNITY_IPHONE
    "/iOSList/";
#elif UNITY_STANDALONE_WIN
    "/WebList/";
#else
 string.Empty;
#endif
    //Android or iOS or Win 伺服器中的 檔案路徑
    public static readonly string assetBundlesPath = serverPath +
#if UNITY_ANDROID
 "/AndroidBundles/";
#elif UNITY_IPHONE
    "/iOSBundles/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
    "/WebBundles/";
#else
 string.Empty;
#endif

    public static PhotonService photonService = new PhotonService();    // Photon ServerClient服務

    public static readonly string sItemList = "ItemList.json";          // 道具列表
    public static readonly string sVisionList = "VisionList.json";      // 版本列表
    public static readonly string sDownloadList = "DownloadList.json";  // 下載列表
    public static readonly string sFullPackage = "FullPackageList.json";// 完整下載列表

    public static bool isCheckBundle = false;       // 是否檢查資源
    public static bool isNewlyVision = true;        // 是否為新版本
    public static bool isVisionDownload = false;    // 是否開始下載版本列表
    public static bool isCompleted = false;         // 是否下載完成
    public static bool isReplaced = false;          // 是否取代列表完成
    public static bool isJoinMember = true;         // 是否加入會員

    public static bool isPlayerDataLoaded = false;  // 是否載入玩家資料
    public static bool isPlayerItemLoaded = false;  // 是否載入玩家資料
    public static bool isCurrencyLoaded = false;    // 是否載入玩家資料
    public static bool isMiceLoaded = false;        // 是否載入老鼠資料
    public static bool isStoreLoaded = false;        // 是否載入老鼠資料
    public static bool isItemLoaded = false;        // 是否載入老鼠資料
    public static bool isArmorLoaded = false;        // 是否載入老鼠資料
    public static bool isLoaded = false;            // 是否載入場景

    public static bool LoginStatus = false;	        // true = 已登入,  false = 未登入
    public static bool BattleStatus = false;        // 是否開始對戰
    public static bool isMatching = false;          // 是否配對成功
    public static bool isExitingRoom = false;       // 是否離開房間
    public static bool isGameStart = false;         // 是否開始遊戲
    public static bool isApplySkill = false;        // 是否受到技能傷害
    public static bool spawnFlag = false;           // 是否產生完成
    public static bool isMissionCompleted = false;  // 是否任務完成
    public static bool missionFlag = true;         // 是否執行任務

    public static int prevScene = (int)Scene.MainGame;  // 上一個場景
    public static int nextScene = (int)Scene.MainGame;  // 要被載入的場景

    public static int maxConnTimes = 5;  // 重新連限次數

    public static string Ret = "";          // 回傳值
    public static int PrimaryID = 0;       // 主索引
    public static string Account = "";      // 帳號
    public static string Nickname = "";     // 暱稱
    public static int RoomID = -1;          // 房間ID
    public static byte Sex = 0;              // 性別
    public static byte Age = 0;              // 年齡
    public static MemberType MemberType;     // 
    public static int Rice = 0;             // 遊戲幣
    public static Int16 Gold = 0;           // 金幣

    public static byte Rank = 0;            // 等級
    public static byte EXP = 0;             // 經驗
    public static Int16 MaxCombo = 0;       // 最大連擊
    public static int MaxScore = 0;         // 最高分
    public static int SumScore = 0;         // 總分
    public static Int16 SumLost = 0;          // 總漏掉的老鼠
    public static int SumKill = 0;          // 總除掉的老鼠
    public static int SumWin = 0;          // 總勝場
    public static int SumBattle = 0;          // 總場次

    public static int MeunObjetDepth = 10000; // 主選單物件深度
    public static Dictionary<string, object> SortedItem = new Dictionary<string, object>();         // 全部老鼠 JSON資料;         // 漏掉的老鼠
    public static Dictionary<string, object> MiceAll = new Dictionary<string, object>();         // 全部老鼠 JSON資料
    public static Dictionary<string, object> Team = new Dictionary<string, object>();         // 隊伍老鼠 JSON資料
    public static Dictionary<string, object> Friend = new Dictionary<string, object>();       // 好友列表 JSON資料
    public static string ReturnMessage = "";       // 回傳訊息

    public static string ext = ".unity3d";       // AB副檔名

    public static class OtherData
    {
        public static int PrimaryID = 0;        // 主索引
        public static string Nickname = "";     // 暱稱
        public static byte Sex = 0;              // 性別
        public static Dictionary<string, object> Team = new Dictionary<string, object>();         // 隊伍老鼠 JSON資料
        public static string RoomPlace = "";    // 另一位玩家的房間位置
    }

    public static int MiceCount = 0;        // 目前 對戰老鼠數量 要移到BattleData

    public static Dictionary<string, object> miceProperty = new Dictionary<string, object>();   // 老鼠屬性資料 
    public static Dictionary<string, object> itemProperty = new Dictionary<string, object>();   // 道具屬性資料 
    public static Dictionary<string, object> storeItem = new Dictionary<string, object>();   // 商店屬性資料 
    public static Dictionary<string, object> playerItem = new Dictionary<string, object>();   // 商店屬性資料 

    public static Dictionary<string, GameObject> dictLoadedScene = new Dictionary<string, GameObject>();
    /*
    public class MiceProperty
    {
        int miceID { get; set; }
        string miceName { get; set; }
        float eatingRate { get; set; }
        float miceSpeed { get; set; }
        int eatFull { get; set; }
        int skill { get; set; }
        int hp { get; set; }
        int miceCost { get; set; }
    }
    */
    public enum Scene : int
    {
        BundleCheck = 0,
        MainGame = 1,
        Battle = 2,
        LoadScene = 3,
    }

    public enum UILayer
    {
        Nothing = 0,
        Default = 1,
        Battle = 8,
        HUD = 9,
        Store = 10,
        Player = 11,
        ItemInfo = 12,
        Message = 13,
    }

    public enum Camrea
    {
        MainCamera,
        HUDCamera,
    }
    
    public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                  TKey fromKey, TKey toKey)
    {
        TValue value = dic[fromKey];
        dic.Remove(fromKey);
        dic[toKey] = value;
    }

    /// <summary>
    /// 交換字典物件
    /// </summary>
    /// <param name="vaule1"></param>
    /// <param name="value2"></param>
    /// <param name="dict"></param>
    public static void SwapDictKeyByValue(string vaule1, string value2, Dictionary<string, object> dict)
    {
        string myKey = "", otherKey = "";
        object myValue = "", otherValue = "";

        myKey = dict.FirstOrDefault(x => x.Value.ToString() == vaule1).Key;
        otherKey = dict.FirstOrDefault(x => x.Value.ToString() == value2).Key;

        dict[myKey] = value2;
        dict[otherKey] = vaule1;
        RenameKey(dict, myKey, "x");
        RenameKey(dict, otherKey, myKey);
        RenameKey(dict, "x", otherKey);
    }

    /// <summary>
    /// 交換字典物件
    /// </summary>
    /// <param name="vaule1"></param>
    /// <param name="key2"></param>
    /// <param name="dict"></param>
    public static void SwapDictValueByKey(string key1, string key2, Dictionary<string, object> dict)
    {
        object value1 = "", value2 = "";

        dict.TryGetValue(key1, out value1);
        dict.TryGetValue(key2, out value2);

        dict[key1] = value2;
        dict[key2] = value1;
    }

    public static TimeSpan Time(Action action)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}