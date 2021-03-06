﻿using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using MiniJSON;
using ExitGames.Logging;
using MPProtocol;
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
 * 這個檔案是用來進行 驗證玩家資料所使用
 * 載入玩家資料、更新玩家資料
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>邏輯都沒寫(還要再看看)
 * UpdatePlayerItem沒寫邏輯
 * ***************************************************************/

namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class PlayerDataLogic : ServicedComponent    // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        PlayerData playerData = new PlayerData();
        Dictionary<string, object> clinetData;
        Dictionary<string, object> serverData;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private byte maxExp = 100;
        private byte maxRank = 100;

        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPlayerData 載入玩家資料

        [AutoComplete]
        public PlayerData LoadPlayerData(string account)
        {
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //to do check 

                PlayerDataIO playerDataIO = new PlayerDataIO();
                playerData = playerDataIO.LoadPlayerData(account);

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;
        }

        #endregion

        #region LoadPlayerItem 載入玩家道具(單筆)資料
        /// <summary>
        /// 載入玩家道具(單筆)資料
        /// </summary>
        public PlayerData LoadPlayerItem(string account, Int16 itemID)
        {
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //to do check 

                PlayerDataIO playerDataIO = new PlayerDataIO();
                playerData = playerDataIO.LoadPlayerItem(account, itemID);
            }
            catch
            {
                throw;
            }

            return playerData;
        }
        #endregion

        #region LoadPlayerItem 載入玩家道具(全部)資料
        /// <summary>
        /// 載入玩家道具(全部)資料
        /// </summary>
        public PlayerData LoadPlayerItem(string account)
        {
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //to do check 

                PlayerDataIO playerDataIO = new PlayerDataIO();
                playerData = playerDataIO.LoadPlayerItem(account);
            }
            catch
            {
                throw;
            }

            return playerData;
        }
        #endregion

        #region UpdatePlayerData 更新玩家(全部)資料 這裡怪怪的(Admin才能使用)

        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, Int16 sumLost, int sumKill, string item, string miceAll, string team , string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                RankChk(rank);
                EXPChk(exp);
                //MaxComboChk(maxCombo);
                //ScoreChk(score);

                if (maxCombo < 0 && maxCombo > 1000)
                {
                    playerData.ReturnCode = "S409";
                    playerData.ReturnMessage = "玩家Combo異常！";
                }

                if (maxScore < 0 && maxScore > int.MaxValue)
                {
                    playerData.ReturnCode = "S410";
                    playerData.ReturnMessage = "玩家分數異常！";
                }

                if (sumScore < 0 && sumScore > int.MaxValue)
                {
                    playerData.ReturnCode = "S411";
                    playerData.ReturnMessage = "玩家總分異常！";
                }

                if (sumLost < 0 && sumLost > Int16.MaxValue)
                {
                    playerData.ReturnCode = "S417";
                    playerData.ReturnMessage = "玩家跑掉的老鼠數量異常！";
                }

                if (sumKill < 0 && sumKill > int.MaxValue)
                {
                    playerData.ReturnCode = "S418";
                    playerData.ReturnMessage = "玩家趕跑的老鼠數量異常！";
                }

                //載入伺服器玩家資料提供比對 錯誤 如果玩家資料減少或變多，卻無法去更新資料
                playerData = LoadPlayerData(account);

                if (playerData.ReturnCode == "S401")
                {
                    clinetData = MiniJSON.Json.Deserialize(miceAll) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAll) as Dictionary<string, object>;

                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S412";
                        playerData.ReturnMessage = "老鼠成員異常！";
                    }


                    clinetData = MiniJSON.Json.Deserialize(team) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.Team) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S413";
                        playerData.ReturnMessage = "隊伍老鼠異常！";
                    }

                    /*
                    clinetData = MiniJSON.Json.Deserialize(miceAmount) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAmount) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count == clinetData.Count)
                    {
                        foreach (KeyValuePair<string, object> serverMice in serverData)
                        {
                            if (serverMice.Value != clinetData[serverMice.Key])
                            {
                                playerData.ReturnCode = "S414";
                                playerData.ReturnMessage = "老鼠數量異常！";
                            }
                        }
                    }
                    else
                    {
                        playerData.ReturnCode = "S414";
                        playerData.ReturnMessage = "老鼠數量異常！";
                    }
                    */

                    clinetData = MiniJSON.Json.Deserialize(friend) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.Friend) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S415";
                        playerData.ReturnMessage = "好友名單異常！";
                    }


                    clinetData = MiniJSON.Json.Deserialize(item) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.SortedItem) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count == clinetData.Count)
                    {
                        foreach (KeyValuePair<string, object> serverItem in serverData)
                        {
                            if (serverItem.Value != clinetData[serverItem.Key])
                            {
                                playerData.ReturnCode = "S419";
                                playerData.ReturnMessage = "道具數量異常！";
                            }
                        }
                    }
                    else
                    {
                        playerData.ReturnCode = "S419";
                        playerData.ReturnMessage = "道具數量異常！";
                    }

                    //如果驗證成功 寫入玩家資料
                    PlayerDataIO playerDataIO = new PlayerDataIO();
                    playerData = playerDataIO.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, sumLost, sumKill, item, miceAll, team, friend);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;

        }

        #endregion

        #region UpdatePlayerData 更新玩家(Team)資料

        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, string miceAll, string team )
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //載入伺服器玩家資料提供比對
                playerData = LoadPlayerData(account);

                if (playerData.ReturnCode == "S401")
                {
                    clinetData = MiniJSON.Json.Deserialize(miceAll) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAll) as Dictionary<string, object>;

                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S412";
                        playerData.ReturnMessage = "老鼠成員異常！";
                    }


                    clinetData = MiniJSON.Json.Deserialize(team) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.Team) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S413";
                        playerData.ReturnMessage = "隊伍老鼠異常！";
                    }

                    /*
                    clinetData = MiniJSON.Json.Deserialize(miceAmount) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAmount) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count == clinetData.Count)
                    {
                        foreach (KeyValuePair<string, object> serverMice in serverData)
                        {
                            if (serverMice.Value != clinetData[serverMice.Key])
                            {
                                playerData.ReturnCode = "S414";
                                playerData.ReturnMessage = "老鼠數量異常！";
                            }
                        }
                    }
                    else
                    {
                        playerData.ReturnCode = "S414";
                        playerData.ReturnMessage = "老鼠數量異常！";
                    }
                    */
                    /*
                    clinetData = MiniJSON.Json.Deserialize(item) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.Item) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count == clinetData.Count)
                    {
                        foreach (KeyValuePair<string, object> serverItem in serverData)
                        {
                            if (serverItem.Value != clinetData[serverItem.Key])
                            {
                                playerData.ReturnCode = "S419";
                                playerData.ReturnMessage = "道具數量異常！";
                            }
                        }
                    }
                    else
                    {
                        playerData.ReturnCode = "S419";
                        playerData.ReturnMessage = "道具數量異常！";
                    }
                    */
                    //如果驗證成功 寫入玩家資料
                    PlayerDataIO playerDataIO = new PlayerDataIO();
                    playerData = playerDataIO.UpdatePlayerData(account, miceAll, team);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;

        }
        #endregion

        #region UpdatePlayerData 更新玩家(老鼠)資料

        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, string miceAll , string miceName, int amount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //載入伺服器玩家資料提供比對
                playerData = LoadPlayerData(account);

                if (playerData.ReturnCode == "S401")
                {
                    clinetData = MiniJSON.Json.Deserialize(miceAll) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAll) as Dictionary<string, object>;

                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S412";
                        playerData.ReturnMessage = "老鼠成員異常！";
                    }

                    if (!serverData.ContainsValue(miceName))
                    {
                        serverData.Add((serverData.Count + 1).ToString(), miceName);
                        miceAll = MiniJSON.Json.Serialize(serverData);
                    }
                    /*
                    clinetData = MiniJSON.Json.Deserialize(miceAmount) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAmount) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同  這沒驗證個別道具數量 只有驗證總數
                    if (serverData.Count == clinetData.Count)
                    {
                        foreach (KeyValuePair<string, object> serverMice in serverData)
                        {
                            if (serverMice.Value != clinetData[serverMice.Key])
                            {
                                playerData.ReturnCode = "S414";
                                playerData.ReturnMessage = "老鼠數量異常！";
                            }
                        }
                    }
                    else
                    {
                        playerData.ReturnCode = "S414";
                        playerData.ReturnMessage = "老鼠數量異常！";
                    }

                    if (!serverData.ContainsKey(miceName))
                    {
                        serverData.Add(miceName, amount);
                    }
                    else
                    {
                        int miceCount = int.Parse(serverData[miceName].ToString());
                        serverData[miceName] = miceCount + amount;
                    }
                    miceAmount = MiniJSON.Json.Serialize(serverData);
*/
                    //如果驗證成功 寫入玩家資料
                    PlayerDataIO playerDataIO = new PlayerDataIO();
                    playerData = playerDataIO.UpdatePlayerData(account, miceAll);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;

        }
        #endregion

        #region UpdateGameOver 更新玩家(GameOver時)資料

        [AutoComplete]
        public PlayerData UpdateGameOver(string account, Int16 score, byte exp, Int16 maxCombo, int maxScore, Int16 lostMice, int killMice, int battleResult, string item )
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                EXPChk(exp);
                if (playerData.ReturnCode == "S401") MaxComboChk(maxCombo);
                if (playerData.ReturnCode == "S401") ScoreChk(score);
                if (playerData.ReturnCode == "S401") MaxScoreChk(maxScore);
                if (playerData.ReturnCode == "S401") LostMiceChk(lostMice);
                if (playerData.ReturnCode == "S401") KillMiceChk(killMice);

                //載入伺服器玩家資料提供比對
                playerData = LoadPlayerData(account);

                // if (playerData.ReturnCode == "S401") MiceAmountChk(miceAmount);  // 還沒寫好
                // if (playerData.ReturnCode == "S401")    ItemChk(item)    // 還沒寫好

                if (playerData.ReturnCode == "S401")
                {
                    playerData.SumScore += score;
                    if (maxScore > playerData.MaxScore) playerData.MaxScore = maxScore;
                    playerData.SumLost += lostMice;
                    playerData.SumKill += killMice;
                    playerData.SumBattle += 1;
                    playerData.SumWin += battleResult > 0 ? 1 : 0;

                    if (playerData.EXP + exp >= maxExp && playerData.Rank != 100)
                    {
                        playerData.Rank += 1;
                        exp -= maxExp;
                        playerData.EXP += exp;
                    }



                    //如果驗證成功 寫入玩家資料
                    PlayerDataIO playerDataIO = new PlayerDataIO();
                    playerData = playerDataIO.UpdateGameOver(account, playerData.Rank, playerData.EXP, maxCombo, maxScore, playerData.SumScore, playerData.SumLost, playerData.SumKill, playerData.SumWin, playerData.SumBattle, item);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;

        }

        #endregion

        #region UpdatePlayerItem 更新玩家(道具)資料
        public PlayerData UpdatePlayerItem(string account, Int16 itemID, string itemName, byte itemType, Int16 itemCount)
        {
            try
            {
                PlayerDataIO playerDataIO = new PlayerDataIO();
                PlayerData playerData2 = playerDataIO.LoadPlayerData(account);


                if (itemType == (byte)StoreType.Mice)
                {
                    var dictMiceAll = Json.Deserialize(playerData2.MiceAll) as Dictionary<string, object>;
                    if (!dictMiceAll.ContainsValue(itemName))
                    {
                        dictMiceAll.Add((dictMiceAll.Count + 1).ToString(), itemName);
                        string miceAll = Json.Serialize(dictMiceAll);
                        playerDataIO.UpdatePlayerData(account, miceAll);
                    }
                }

                playerData = playerDataIO.LoadPlayerItem(account, itemID);



                playerData.ItemCount += itemCount;

                playerData = playerDataIO.UpdatePlayerItem(account, itemID, itemType, playerData.ItemCount);


                return playerData;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region UpdatePlayerItem 更新玩家(多筆道具)資料
        public PlayerData UpdatePlayerItem(string account, string jItemUsage)
        {
            PlayerDataIO playerDataIO = new PlayerDataIO();
            playerData = playerDataIO.LoadPlayerItem(account);

            var dictClinetData = Json.Deserialize(jItemUsage) as Dictionary<string, object>;
            var dictServerData = Json.Deserialize(playerData.PlayerItem) as Dictionary<string, object>;

            Dictionary<Int16, Dictionary<string, object>> dictSendData = new Dictionary<Int16, Dictionary<string, object>>();
            Log.Debug("FUCK");
            //Log.Debug("dictServerItem.Count:" + dictServerItem.Count + "dictClient.Count:" + dictClient.Count);

            foreach (KeyValuePair<string, object> dictNestedClinetData in dictClinetData) // 玩家道具使用量 迴圈
            {
                //var innClientItem = dictClinetItemUsage as Dictionary<string, object>;  // 道具資料字典

                foreach (KeyValuePair<string, object> dictNestedServerData in dictServerData)   // 伺服器道具資料 迴圈
                {
                    Log.Debug("dictServerData:" + dictServerData.Count + " dictClinetData:" + dictClinetData.Count);


                    object itemID, serverItemCount; // 道具ID
                    object serverItemObejct, clientItemObejct; // 道具資料

                    //dictServerItem = dictNestedServerData.Value as Dictionary<string, object>;

                    dictServerData.TryGetValue(dictNestedClinetData.Key, out serverItemObejct);    // 找到與伺服器對應的道具資料 key=10001
                    var dictServerItem = serverItemObejct as Dictionary<string, object>;


                    Log.Debug("  dictServerItem Key:" + dictServerItem.Keys + "  dictServerItem value:" + dictServerItem.Values);







                    dictServerItem.TryGetValue(((short)PlayerItem.ItemID).ToString(), out itemID); // 取出道具數量
                    dictServerItem.TryGetValue(((short)PlayerItem.ItemCount).ToString(), out serverItemCount); // 取出道具數量


                    dictClinetData.TryGetValue(itemID.ToString(), out clientItemObejct);
                    // to do compare data

                    var dictClient = clientItemObejct as Dictionary<string, object>;

                    object serverCount, serverUseage, clientCount, clientUseage;

                    dictServerItem.TryGetValue(((short)PlayerItem.ItemCount).ToString(), out serverCount);
                    dictServerItem.TryGetValue(((short)PlayerItem.UseCount).ToString(), out serverUseage);
                    dictClient.TryGetValue(((short)PlayerItem.ItemCount).ToString(), out clientCount);
                    dictClient.TryGetValue(((short)PlayerItem.UseCount).ToString(), out clientUseage);

                    Log.Debug("serverCount: " + serverCount + "serverUseage: " + serverUseage + "clientCount: " + clientCount + "clientUseage: " + clientUseage);
                    Log.Debug(serverCount + "  " + clientUseage);

                    int sCount = Convert.ToInt32(serverCount);
                    int sUseage = Convert.ToInt32(serverUseage);
                    int cCount = Convert.ToInt32(clientCount);
                    int cUseage = Convert.ToInt32(clientUseage);

                    //|| (int)clientCount > Int16.MaxValue
                    if (sCount < cUseage)
                    {
                        playerData.ReturnCode = "S427";
                        playerData.ReturnMessage = "玩家道具資料異常！";
                        return playerData;
                    }
                    else if (sCount == cCount)
                    {
                        int itemCount, useCount;
                        itemCount = sCount - cUseage;
                        useCount = cUseage + sUseage;
                        dictClient[((short)PlayerItem.ItemCount).ToString()] = itemCount;
                        dictClient[((short)PlayerItem.UseCount).ToString()] = useCount;
                        dictSendData.Add(Int16.Parse(itemID.ToString()), dictClient);
                        Log.Debug("ItemCount: " + itemCount + "  UseCount: " + useCount);
                    }/*
                        else if (serverCount == clientUseage)
                        {
                            // 未來如果要移除為0的道具 在啟用
                          * // IO 需新增 刪除
                          return playerData;
                        }*/
                    else
                    {
                        playerData.ReturnCode = "S427";
                        playerData.ReturnMessage = "玩家道具資料異常！";
                        return playerData;
                    }

                    break;
                }
            }
            playerData = playerDataIO.UpdatePlayerItem(account, dictSendData);

            return playerData;
        }
        #endregion

        #region UpdatePlayerItem 更新玩家(道具)裝備狀態
        public PlayerData UpdatePlayerItem(string account, Int16 itemID, bool isEquip)
        {
            try
            {
                PlayerDataIO playerDataIO = new PlayerDataIO();
                playerData = playerDataIO.UpdatePlayerItem(account, itemID, isEquip);

                return playerData;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region SortPlayerItem 更新玩家(道具排序)資料
        public PlayerData SortPlayerItem(string account, string jString)
        {
            try
            {
                PlayerDataIO playerDataIO = new PlayerDataIO();
                playerData = playerDataIO.SortPlayerItem(account, jString);

                return playerData;
            }
            catch
            {
                throw;
            }
        }
        #endregion
















        private PlayerData RankChk(byte rank)
        {
            PlayerData playerData = new PlayerData();

            if (rank < 0 && rank > 99)
            {
                playerData.ReturnCode = "S407";
                playerData.ReturnMessage = "玩家等級異常！";
                return playerData;
            }

            playerData.ReturnCode = "S401";
            return playerData;
        }

        private PlayerData EXPChk(byte exp)
        {
            PlayerData playerData = new PlayerData();

            if (exp < 0 && exp > 100)
            {
                playerData.ReturnCode = "S408";
                playerData.ReturnMessage = "玩家經驗異常！";
            }
            playerData.ReturnCode = "S401";
            return playerData;
        }

        private PlayerData MaxComboChk(Int16 maxCombo)
        {
            PlayerData playerData = new PlayerData();

            if (maxCombo < 0 && maxCombo > 1000)
            {
                playerData.ReturnCode = "S409";
                playerData.ReturnMessage = "玩家Combo異常！";
            }
            playerData.ReturnCode = "S401";
            return playerData;
        }

        private PlayerData ScoreChk(Int16 score)
        {
            PlayerData playerData = new PlayerData();

            if (score < 0 && score > Int16.MaxValue)
            {
                playerData.ReturnCode = "S410";
                playerData.ReturnMessage = "玩家分數異常！";
            }
            playerData.ReturnCode = "S401";
            return playerData;
        }

        private PlayerData MaxScoreChk(int maxScore)
        {
            PlayerData playerData = new PlayerData();

            if (maxScore < 0 && maxScore > int.MaxValue)
            {
                playerData.ReturnCode = "S410";
                playerData.ReturnMessage = "玩家分數異常！";
            }

            playerData.ReturnCode = "S401";
            return playerData;
        }

        private PlayerData LostMiceChk(Int16 lostMice)
        {
            PlayerData playerData = new PlayerData();

            if (lostMice < 0 && lostMice > Int16.MaxValue)
            {
                playerData.ReturnCode = "S417";
                playerData.ReturnMessage = "玩家跑掉的老鼠數量異常！";
            }
            playerData.ReturnCode = "S401";
            return playerData;
        }

        private PlayerData KillMiceChk(int killMice)
        {
            PlayerData playerData = new PlayerData();

            if (killMice < 0 && killMice > int.MaxValue)
            {
                playerData.ReturnCode = "S418";
                playerData.ReturnMessage = "玩家趕跑的老鼠數量異常！";
            }
            playerData.ReturnCode = "S401";
            return playerData;
        }
        /*
        private PlayerData MiceAmountChk(string miceAmount)
        {
            clinetData = MiniJSON.Json.Deserialize(miceAmount) as Dictionary<string, object>;
            serverData = MiniJSON.Json.Deserialize(playerData.MiceAmount) as Dictionary<string, object>;

            //如果與伺服器資料 數量不相同
            if (serverData.Count == clinetData.Count)
            {
                foreach (KeyValuePair<string, object> serverMice in serverData)
                {
                    if (serverMice.Value != clinetData[serverMice.Key])
                    {
                        playerData.ReturnCode = "S414";
                        playerData.ReturnMessage = "老鼠數量異常！";
                        return playerData;
                    }
                }
            }
            else
            {
                playerData.ReturnCode = "S414";
                playerData.ReturnMessage = "老鼠數量異常！";
                return playerData;
            }

            playerData.ReturnCode = "S401";
            playerData.ReturnMessage = "";
            return playerData;
        }
        */
        private PlayerData ItemChk(string item)
        {
            clinetData = MiniJSON.Json.Deserialize(item) as Dictionary<string, object>;
            serverData = MiniJSON.Json.Deserialize(playerData.SortedItem) as Dictionary<string, object>;

            //如果與伺服器資料 數量不相同
            if (serverData.Count == clinetData.Count)
            {
                foreach (KeyValuePair<string, object> serverItem in serverData)
                {
                    if (serverItem.Value != clinetData[serverItem.Key])
                    {
                        playerData.ReturnCode = "S419";
                        playerData.ReturnMessage = "道具數量異常！";
                        return playerData;
                    }
                }
            }
            else
            {
                playerData.ReturnCode = "S419";
                playerData.ReturnMessage = "道具數量異常！";
                return playerData;
            }

            playerData.ReturnCode = "S401";
            playerData.ReturnMessage = "道具數量異常！";
            return playerData;
        }
    }
}
