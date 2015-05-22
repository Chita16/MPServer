﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using MPProtocol;
using MiniJSON;

public class PhotonService : MonoBehaviour, IPhotonPeerListener
{
    protected PhotonPeer peer;		    // 連線用
    protected bool isConnected;	        // 是否已連接連線伺服器  true:已連線 false:已斷線
    protected string DebugMessage;	    // 錯誤訊息

    //委派事件 連接伺服器
    public delegate void ConnectEventHandler(bool ConnectStatus);
    public event ConnectEventHandler ConnectEvent;

    //委派事件 加入會員
    public delegate void JoinMemberEventHandler(bool joinStatus,string returnCode, string message);
    public event JoinMemberEventHandler JoinMemberEvent;

    //委派事件 登入
    public delegate void LoginEventHandler(bool loginStatus, string nessage, string returnCode, int primaryID, string account, string nickname, byte sex, byte age);
    public event LoginEventHandler LoginEvent;

    //委派事件 接收技能傷害
    public delegate void ApplyDamageEventHandler(int miceID);
    public event ApplyDamageEventHandler ApplyDamageEvent;

    //委派事件 接收分數
    public delegate void OtherScoreEventHandler(Int16 Score);
    public event OtherScoreEventHandler OtherScoreEvent;

    //委派事件 離開房間
    public delegate void ExitRoomEventHandler();
    public event ExitRoomEventHandler ExitRoomEvent;

    public bool ServerConnected
    {
        get { return this.isConnected; }
    }

    public string ServerDebugMessage
    {
        get { return this.DebugMessage; }
    }

    //  Connect to Server
    public void Connect(string IP, int Port, string ServerName)
    {
        try
        {
            string ServerAddress = IP + ":" + Port.ToString();
            this.peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            if (!this.peer.Connect(ServerAddress, ServerName))
            {
                ConnectEvent(false);
            }
        }
        catch (Exception e)
        {
            ConnectEvent(false);
            throw e;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (peer != null)
            {
                this.peer.Disconnect();
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }


    /// <summary>
    /// Call on Update() 必須要一直呼叫才能持續連線
    /// </summary>
 
    public void Service()
    {
        try
        {
            if (this.peer != null)
                this.peer.Service();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        this.DebugMessage = message;
    }

    void IPhotonPeerListener.DebugReturn(DebugLevel level, string message)
    {
        throw new NotImplementedException();
    }

    // 收到 伺服器傳來的事件時
    void IPhotonPeerListener.OnEvent(EventData eventData)
    {
        //Debug.Log(eventData.Code.ToString());
        switch (eventData.Code)
        {
                // 重複登入
            case (byte)LoginOperationCode.ReLogin:
                Global.LoginStatus = false;
                Global.isMatching = false;
                break;

                // 配對成功 傳入 房間ID、對手資料、老鼠資料
            case (byte)MatchGameResponseCode.Match:
                Global.RoomID = (int)eventData.Parameters[(byte)MatchGameParameterCode.RoomID];
                Global.OtherData.Nickname = (string)eventData.Parameters[(byte)MatchGameParameterCode.Nickname];
                Global.OtherData.PrimaryID = (int)eventData.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                Global.OtherData.Team = (string)eventData.Parameters[(byte)MatchGameParameterCode.Team];
                Debug.Log(Global.OtherData.Team);
                Global.BattleStatus = true;
                break;

                // 被踢出房間了
            case (byte)BattleResponseCode.KickOther:
                ExitRoomEvent();
                Debug.Log("Recive Kick!" + (string)eventData.Parameters[(byte)BattleResponseCode.DebugMessage]);
                break;

                // 他斷線 我也玩不了 離開房間
            case (byte)BattleResponseCode.Offline:
                ExitRoomEvent();
                Debug.Log("Recive Offline!" + (string)eventData.Parameters[(byte)BattleResponseCode.DebugMessage]);
                break;

                // 接收 技能傷害
            case (byte)BattleResponseCode.ApplyDamage:
                int miceID = (int)eventData.Parameters[(byte)BattleParameterCode.MiceID];
                ApplyDamageEvent(miceID);
                Debug.Log("Recive Damage!" + (string)eventData.Parameters[(byte)BattleResponseCode.DebugMessage]);
                break;

                //取得對方分數
            case (byte)BattleResponseCode.GetScore:

                Int16 otherScore = (Int16)eventData.Parameters[(byte)BattleParameterCode.OtherScore];
                //Debug.Log(("Recive otherScore!"+otherScore);
                OtherScoreEvent(otherScore);
                break;
        }

    }

    //當收到Server資料時
    void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {

            #region JoinMember 加入會員

            case (byte)JoinMemberResponseCode.JoinMember://登入
                {
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)  // if success
                    {
                        string returnCode = (string)operationResponse.Parameters[(byte)JoinMemberParameterCode.Ret];
                        JoinMemberEvent(true, returnCode , operationResponse.DebugMessage.ToString()); // send member data to loginEvent

                    }
                    else//假如登入失敗 
                    {
                        string returnCode = (string)operationResponse.Parameters[(byte)JoinMemberParameterCode.Ret];
                        JoinMemberEvent(false,returnCode, operationResponse.DebugMessage.ToString()); // send member data to loginEvent
                    }
                    break;
                }

            #endregion

            #region Login 登入

            case (byte)LoginOperationCode.Login://登入
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)  // if success
                        {
                            Debug.Log("login");
                            string getReturn = Convert.ToString(operationResponse.Parameters[(byte)LoginParameterCode.Ret]);
                            string getMemberID = Convert.ToString(operationResponse.Parameters[(byte)LoginParameterCode.Account]);
                            string getNickname = Convert.ToString(operationResponse.Parameters[(byte)LoginParameterCode.Nickname]);
                            byte getSex = Convert.ToByte(operationResponse.Parameters[(byte)LoginParameterCode.Sex]);
                            byte getAge = Convert.ToByte(operationResponse.Parameters[(byte)LoginParameterCode.Age]);
                            int getPirmaryID = Convert.ToInt32(operationResponse.Parameters[(byte)LoginParameterCode.PrimaryID]);

                            LoginEvent(true, "", getReturn, getPirmaryID, getMemberID, getNickname, getSex, getAge); // send member data to loginEvent

                        }
                        else//假如登入失敗 傳空值
                        {
                            Debug.Log("login fail :" + operationResponse.OperationCode);
                            DebugReturn(0, operationResponse.DebugMessage.ToString());
                            LoginEvent(false, operationResponse.DebugMessage.ToString(), operationResponse.ReturnCode.ToString(), 0, "", "", 0, 0); // send error message to loginEvent
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message +e.StackTrace);
                    }
                }
                break;

            #endregion

            #region ExitRoom 離開房間

            case (byte)BattleOperationCode.ExitRoom:    // 離開房間
                {
                    try
                    {
                        ExitRoomEvent();
                        Debug.Log("房間資訊："+operationResponse.DebugMessage.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region ExitWaitingRoom  離開等待房間 這裡以後可能發生錯誤 當MatchGame改變時

            case (byte)MatchGameResponseCode.ExitWaiting:// 離開等待房間
                {
                    try
                    {
                        Global.isMatching = false;
                        Debug.Log("房間資訊：" + operationResponse.DebugMessage.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region LoadPlayerData 載入玩家資料

            case (byte)PlayerDataResponseCode.Loaded:   // 載入玩家資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Global.Rank = (byte)operationResponse.Parameters[(byte)PlayerDataParameterCode.Rank];
                            Global.EXP = (byte)operationResponse.Parameters[(byte)PlayerDataParameterCode.EXP];
                            Global.MaxCombo = (Int16)operationResponse.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                            Global.MaxScore = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.MaxScore];
                            Global.SumScore = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumScore];
                            Global.MiceAll = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.MiceAll];
                            Global.Team = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Team];
                            Global.MiceAmount = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.MiceAmount];
                            Global.Friend = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Friend];
                            Global.isPlayerDataLoaded=true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region LoadCurrency 載入貨幣資料

            case (byte)CurrencyResponseCode.Loaded: // 取得貨幣資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Global.Rice = (int)operationResponse.Parameters[(byte)CurrencyParameterCode.Rice];
                            Global.Gold = (Int16)operationResponse.Parameters[(byte)CurrencyParameterCode.Gold];

                            Global.isCurrencyLoaded = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region LoadMice 載入老鼠資料

            case (byte)MiceResponseCode.LoadMice:   // 取得老鼠資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            string miceData= (string)operationResponse.Parameters[(byte)MiceParameterCode.MiceData];
                            Dictionary<string, object> miceProperty = Json.Deserialize(miceData) as Dictionary<string, object>;
                            Global.miceProperty = miceProperty;
                                
                            Global.isMiceLoaded = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region UpdateScore(Verification Fail) 還沒寫完

            case (byte)BattleResponseCode.UpdateScore://取得老鼠資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.InvalidParameter)
                        {
                            //to do  exit room
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion
        }


    }

    // 當連線狀態改變時
    void IPhotonPeerListener.OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                this.peer.EstablishEncryption(); //  連線後開啟加密功能
                break;
            case StatusCode.Disconnect:
                this.peer = null;
                this.isConnected = false;
                ConnectEvent(false);
                break;
            case StatusCode.EncryptionEstablished: // 加密啟動成功後
                this.isConnected = true;
                ConnectEvent(true); // 此時才算是完成連線動作
                break;
        }
    }

    #region Login 登入會員
    /// <summary>
    /// 登入會員 傳送資料到Server
    /// </summary>
    public void Login(string Account, string Password)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { 
                             { (byte)LoginParameterCode.Account,Account },   { (byte)LoginParameterCode.Password, Password }
                        };

            this.peer.OpCustom((byte)LoginOperationCode.Login, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }

    }
    #endregion

    #region JoinMember 加入會員
    /// <summary>
    /// 加入會員 
    /// </summary>
    public void JoinMember(string Account, string Password, string Nickname, byte Age, byte Sex)
    {
        try
        {
            byte age = Convert.ToByte(Age);
            byte sex = Convert.ToByte(Sex);


            Dictionary<byte, object> parameter = new Dictionary<byte, object> { 
                             { (byte)JoinMemberParameterCode.Account,Account },   { (byte)JoinMemberParameterCode.Password, Password }  ,{ (byte)JoinMemberParameterCode.Nickname, Nickname }  ,
                             { (byte)JoinMemberParameterCode.Age, age }  ,{ (byte)JoinMemberParameterCode.Sex, sex }  , { (byte)JoinMemberParameterCode.JoinDate, DateTime.Now.ToString() }  
                        };

            this.peer.OpCustom((byte)JoinMemberOperationCode.JoinMember, parameter, true, 0, true); // operationCode is 21
        }
        catch (Exception e)
        {
            throw e;
        }

    }
    #endregion

    #region ExitRoom 離開房間
    /// <summary>
    /// 離開房間
    /// </summary>
    public void ExitRoom()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 {(byte)BattleParameterCode.RoomID,Global.RoomID},{(byte)BattleParameterCode.PrimaryID,Global.PrimaryID}
            };

            this.peer.OpCustom((byte)BattleOperationCode.ExitRoom, parameter, true, 0, true); 
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region CheckStatus 檢查對手線上狀態
    /// <summary>
    /// 檢查對手線上狀態
    /// </summary>
    public void CheckStatus()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 {(byte)BattleParameterCode.RoomID,Global.RoomID},{(byte)BattleParameterCode.PrimaryID,Global.PrimaryID} 
            };

            this.peer.OpCustom((byte)BattleOperationCode.CheckStatus, parameter, true, 0, true); 
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region KickOther 把對手踢了
    /// <summary>
    /// 把對手踢了
    /// </summary>
    public void KickOther()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 {(byte)BattleParameterCode.RoomID,Global.RoomID},{(byte)BattleParameterCode.PrimaryID,Global.PrimaryID}
            };

            this.peer.OpCustom((byte)BattleOperationCode.KickOther, parameter, true, 0, true); // operationCode is RoomSpeak
            Debug.Log("Send Kick!");
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion


    #region MatchGame 開始配對遊戲
    /// <summary>
    /// 開始配對遊戲
    /// </summary>
    public void MatchGame(int PrimaryID)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 { (byte)MatchGameParameterCode.PrimaryID,PrimaryID}
            };

            this.peer.OpCustom((byte)MatchGameOperationCode.MatchGame, parameter, true, 0, true); 
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region SendDamage 傳送技能攻擊
    /// <summary>
    /// 傳送技能攻擊 傳送資料到Server
    /// </summary>
    public void SendDamage(int miceID) //攻擊測試
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 { (byte)BattleParameterCode.MiceID,miceID } ,{ (byte)BattleParameterCode.RoomID,Global.RoomID },{ (byte)BattleParameterCode.PrimaryID,Global.PrimaryID }
            };

            this.peer.OpCustom((byte)BattleOperationCode.SendDamage, parameter, true, 0, true); 
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region ExitWaitingRoom 離開等待房間
    /// <summary>
    /// 離開等待房間 傳送資料到Server
    /// </summary>
    public void ExitWaitingRoom()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> ();
            this.peer.OpCustom((byte)MatchGameOperationCode.ExitWaiting, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadPlayerData 載入玩家資料
    /// <summary>
    /// 載入玩家資料 傳送資料到Server
    /// </summary>
    public void LoadPlayerData(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)PlayerDataParameterCode.Account, account } };
            this.peer.OpCustom((byte)PlayerDataOperationCode.Load, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdatePlayerData 更新玩家資料
    /// <summary>
    /// 更新玩家資料
    /// </summary>
    public void UpdatePlayerData(string account,byte rank,byte exp , Int16 maxCombo, int maxScore,int sumScore,string miceAll,string team,string miceAmount,string friend)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, account }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.EXP, exp },
             { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore },
             { (byte)PlayerDataParameterCode.MiceAll, miceAll }, { (byte)PlayerDataParameterCode.Team, team }, { (byte)PlayerDataParameterCode.MiceAmount, miceAmount },
             { (byte)PlayerDataParameterCode.Friend, friend }};
            this.peer.OpCustom((byte)PlayerDataOperationCode.Update, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion


    #region LoadCurrency 載入貨幣資料
    /// <summary>
    /// 載入貨幣資料
    /// </summary>
    public void LoadCurrency(string account) 
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)CurrencyParameterCode.Account, account } };
            this.peer.OpCustom((byte)CurrencyOperationCode.Load, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadRice 載入遊戲幣資料
    /// <summary>
    /// 載入遊戲幣資料
    /// </summary>
    public void LoadRice(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)CurrencyParameterCode.Account, account } };
            this.peer.OpCustom((byte)CurrencyOperationCode.LoadRice, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadGold 載入金幣資料
    /// <summary>
    /// 載入金幣資料
    /// </summary>
    public void LoadGold(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)CurrencyParameterCode.Account, account } };
            this.peer.OpCustom((byte)CurrencyOperationCode.LoadGold, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadMiceData 載入老鼠資料
    /// <summary>
    /// 載入老鼠資料
    /// </summary>
    public void LoadMiceData()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {};
            this.peer.OpCustom((byte)MiceOperationCode.LoadMice, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdateScore 更新分數
    /// <summary>
    /// 更新分數
    /// </summary>
    public void UpdateScore(float time,float eatingRate,Int16 score)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.PrimaryID, Global.PrimaryID }, { (byte)BattleParameterCode.RoomID, Global.RoomID }, { (byte)BattleParameterCode.MiceID, (byte)1 },{ (byte)BattleParameterCode.Time, time }, { (byte)BattleParameterCode.EatingRate, eatingRate }, { (byte)BattleParameterCode.Score, score } };
            this.peer.OpCustom((byte)BattleOperationCode.UpdateScore, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion
}