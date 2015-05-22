﻿using ExitGames.Logging;
using MPProtocol;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using Gansol;

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
 * 主要的伺服器邏輯區塊 負責交換訊息 Request Response
 * 每一個玩家都會有一個MPServerPeer
 * 
 * Match Game 完成 還需加入 傳給對方的值
 * room.waitingList[1].TryGetValue("RoomActor1", out otherActor);
 * 1是代表字典檔中的 第1間房 多間房需要修改值
 * 這裡的otherAcotr都是單一玩家，需要取得多人要改寫
 * ***************************************************************/

namespace MPServer
{
    public class MPServerPeer : PeerBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Guid peerGuid { get; protected set; }
        private MPServerApplication _server;
        private bool isCreateRoom;
        private Actor actor;
        private int roomID;
        private int primaryID;
        MPServerPeer peerOther;

        //初始化
        public MPServerPeer(IRpcProtocol rpcProtocol, IPhotonPeer nativePeer, MPServerApplication ServerApplication)
            : base(rpcProtocol, nativePeer)
        {
            isCreateRoom = false;
            peerGuid = Guid.NewGuid();      // 建立一個Client唯一識別GUID
            _server = ServerApplication;

            _server.Actors.AddConnectedPeer(peerGuid, this);    // 加入連線中的peer列表
        }

        //當斷線時
        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            Log.Debug("peer:" + peerGuid);
            Log.Debug("已登出玩家：" + _server.Actors.GetAccountFromGuid(peerGuid) + "  時間:" + DateTime.Now);

            Actor existPlayer = _server.Actors.GetActorFromGuid(peerGuid);                                      // 用GUID找到離線的玩家
            try
            {
                if (_server.room.GetRoomFromGuid(peerGuid) > 0)                                                 // 假如用離開的玩家guid 從GameRoomList中找的到房間 (遊戲中中斷)
                {
                    int roomID = _server.room.GetRoomFromGuid(peerGuid);                                        //取得房間ID
                    Room.RoomActor otherActor = _server.room.GetOtherPlayer(roomID, existPlayer.PrimaryID);     // 取得房間內的其他玩家

                    _server.room.RemovePlayingRoom(roomID, peerGuid, otherActor.guid);                          // 移除房間 與房間內玩家
                }

                if (_server.room.bGetWaitActorFromGuid(peerGuid))                                               // 如果這個玩家在等待房間中 
                {
                    _server.room.RemoveWatingRoom();                                                            // 移除等待房間(這裡要改應位以後可能有很多等待房間)
                }
            }
            catch (Exception e)
            {
                Log.Debug("DisConnect Error : " + e.Message + " Track:" + e.StackTrace);
            }
            _server.Actors.ActorOffline(peerGuid); // 把玩家離線
        }

        //當收到Client回應時
        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            try
            {
                if (operationRequest.OperationCode < 9) // 若參數小於2則返回錯誤 (用來接收Client錯誤訊息使用)
                {
                    Log.Debug("IN <9 :" + operationRequest.OperationCode.ToString());
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = "Login Fail" };
                    SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    Log.Debug("IN >9 :" + operationRequest.OperationCode.ToString());

                    switch (operationRequest.OperationCode)
                    {

                        #region JoinMember 加入會員 (這裡面有IP、email的參數亂打)

                        case (byte)JoinMemberOperationCode.JoinMember:
                            try
                            {
                                string account = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Account];
                                string password = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Password];
                                string nickname = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Nickname];
                                byte age = (byte)operationRequest.Parameters[(byte)JoinMemberParameterCode.Age];
                                byte sex = (byte)operationRequest.Parameters[(byte)JoinMemberParameterCode.Sex];
                                string IP = LocalIP;
                                string email = "example@example.com";
                                string joinTime = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.JoinDate];

                                MPCOM.MemberUI memberUI = new MPCOM.MemberUI();
                                MPCOM.MemberData memberData = (MPCOM.MemberData)TextUtility.DeserializeFromStream(memberUI.JoinMember(account, password, nickname, age, sex, IP, email, joinTime));
                                if (memberData.ReturnCode == "S101")
                                {
                                    Log.Debug("Join Member Successd ! ");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)JoinMemberParameterCode.Ret, memberData.ReturnCode}
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)JoinMemberResponseCode.JoinMember, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else
                                {
                                    Log.Debug("Join Member Failed ! " + memberData.ReturnCode + memberData.ReturnMessage);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)JoinMemberParameterCode.Ret, memberData.ReturnCode}
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)JoinMemberResponseCode.JoinMember, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Member Error ! " + e.Message + "Track: " + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region Login 會員登入

                        case (byte)LoginOperationCode.Login: 
                            try
                            {
                                string account = (string)operationRequest.Parameters[(byte)LoginParameterCode.Account];
                                string passowrd = (string)operationRequest.Parameters[(byte)LoginParameterCode.Password];

                                MPCOM.MemberUI memberUI = new MPCOM.MemberUI(); //實體化 UI (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                MPCOM.MemberData memberData = (MPCOM.MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin(account, passowrd)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");
                                Log.Debug("memberData.Account: " + memberData.Account);
                                //＊＊＊＊＊這有問題 如果重複登入 不能去拿伺服器資料 會偷資料＊＊＊＊＊　
                                switch (memberData.ReturnCode)　//取得資料庫資料成功
                                {
                                    case "S200":
                                        Log.Debug("會員資料內部程式錯誤！");
                                        break;
                                    case "S201":
                                        Log.Debug("memberData.ReturnCode == S201");
                                        primaryID = memberData.PrimaryID; //將資料傳入變數
                                        string nickname = memberData.Nickname;
                                        byte sex = memberData.Sex;
                                        byte age = memberData.Age;
                                        string IP = memberData.IP;

                                        //加入線上玩家列表
                                        ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, primaryID, account, nickname, age, sex, IP, this);
                                        Log.Debug("ReturnCode :" + actorReturn.ReturnCode);
                                        Log.Debug("ReturnMessage :" + actorReturn.DebugMessage);

                                        if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                        {
                                            Log.Debug("actorReturn.ReturnCode == S301");
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, primaryID }, { (byte)LoginParameterCode.Account, account }, { (byte)LoginParameterCode.Nickname, nickname }, { (byte)LoginParameterCode.Age, age }, { (byte)LoginParameterCode.Sex, sex } 
                                    };

                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());
                                        }
                                        else if (actorReturn.ReturnCode == "S302") //登入錯誤 重複登入
                                        {
                                            Log.Debug("重複區(3) PK:" + primaryID);

                                            // 用這個peer guid的PrimaryUD來找guid 踢掉線上玩家
                                            MPServerPeer peer;
                                            peer = _server.Actors.GetPeerFromPrimary(primaryID); //primaryID取得登入peer


                                            // 送出 重複登入事件 叫另外一個他斷線
                                            EventData eventData = new EventData((byte)LoginOperationCode.ReLogin);
                                            peer.SendEvent(eventData, new SendParameters());


                                            // 回傳給登入者 通知重複登入
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                        { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } 
                                    };

                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());

                                            peer.OnDisconnect(new DisconnectReason(), "重複登入");
                                            Log.Debug("重複登入了!");
                                        }
                                        else // 登入錯誤 回傳空值
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                        { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } 
                                    };

                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());
                                            break;
                                        }
                                        break;
                                    default: // 無效的登入資訊 無法取得伺服器資料
                                        Log.Debug("無效的登入資訊 無法取得伺服器資料!");
                                        OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug("(S299)Login Failed ! " + e.Message + "  data:" + e.Data + "  Trace:" + e.StackTrace);
                            }
                            break;
                        #endregion

                        #region MatchGame 配對遊戲
                        case (byte)MatchGameOperationCode.MatchGame:
                            Log.Debug("Match Game");
                            primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];

                            actor = _server.Actors.GetActorFromPrimary(primaryID);  // 用primaryID取得角色資料
                            Log.Debug("waitingList Count:" + _server.room.waitingList.Count);

                            try
                            {
                                if (_server.room.waitingList.Count == 0) //假如等待房間列表中沒房間 建立等待房間
                                {
                                    isCreateRoom = _server.room.CreateRoom(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP);
                                    Log.Debug("Create Room !" + "  Count:" + _server.room.waitingList.Count);
                                }
                                else //假如等待列表中有等待配對的房間
                                {
                                    if (_server.room.JoinRoom(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP))
                                    {
                                        Log.Debug("Join MyRoom:" + _server.room.myRoom);
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.

                                        Log.Debug("Count:" + _server.room.waitingList.Count);
                                        _server.room.waitingList[1].TryGetValue("RoomActor1", out otherActor);

                                        MPCOM.PlayerDataUI playerDataUI = new MPCOM.PlayerDataUI();
                                        MPCOM.PlayerData otherPlayerData = (MPCOM.PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(otherActor.Account));

                                        Dictionary<byte, object> matchMyParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, otherActor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, otherActor.Nickname }, { (byte)MatchGameParameterCode.RoomID, _server.room.myRoom }, { (byte)MatchGameParameterCode.Team, otherPlayerData.Team }};
                                        Dictionary<byte, object> matchOtherParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, actor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, actor.Nickname }, { (byte)MatchGameParameterCode.RoomID, _server.room.myRoom }, { (byte)MatchGameParameterCode.Team, otherPlayerData.Team } };

                                        // 玩家2加入房間成功後 發送配對成功事件給房間內兩位玩家
                                        EventData matchMyEventData = new EventData((byte)MatchGameResponseCode.Match, matchMyParameter);
                                        EventData matchOthertEventData = new EventData((byte)MatchGameResponseCode.Match, matchOtherParameter);
                                        MPServerPeer peerMe;

                                        //取得雙方玩家的peer
                                        peerMe = _server.Actors.GetPeerFromGuid(this.peerGuid);
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                        Log.Debug("Me Peer:" + this.peerGuid + "  " + actor.Nickname);
                                        Log.Debug("Other Peer:" + otherActor.guid + "  " + otherActor.Nickname);

                                        peerMe.SendEvent(matchMyEventData, new SendParameters());
                                        peerOther.SendEvent(matchOthertEventData, new SendParameters());

                                        // 移除等待列表房間
                                        _server.room.RemoveWatingRoom();
                                    }
                                    else
                                    {
                                        Log.Debug("Join Room Fail !  RoomID: " + _server.room.myRoom);
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Failed ! " + e.Message + "  於程式碼:" + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region ExitRoom 離開房間
                        case (byte)BattleOperationCode.ExitRoom:
                            {
                                try
                                {
                                    Log.Debug("IN ExitRoom");

                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    Log.Debug("RoomID: " + roomID);
                                    //to do exit room
                                    Log.Debug("1 playingRoomList Count:" + _server.room.playingRoomList.Count);
                                    Log.Debug("in Dictionary 1 :" + _server.room.playingRoomList[roomID].ContainsKey("RoomActor1"));
                                    Log.Debug("in Dictionary 1 :" + _server.room.playingRoomList[roomID].ContainsKey("RoomActor2"));

                                    // 取得雙方角色
                                    actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    Room.RoomActor otherActor = _server.room.GetOtherPlayer(roomID, actor.PrimaryID);

                                    // 移除遊戲中房間、玩家
                                    _server.room.RemovePlayingRoom(roomID, actor.guid, otherActor.guid);
                                    Log.Debug("2 playingRoomList Count:" + _server.room.playingRoomList.Count);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };

                                    OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                    SendOperationResponse(response, new SendParameters());

                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region ExitWaitingRoom 離開等待房間
                        case (byte)MatchGameOperationCode.ExitWaiting:
                            {
                                try
                                {
                                    Log.Debug("IN ExitWaitingRoom");

                                    _server.room.RemoveWatingRoom();    // 移除等待房間
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object>();

                                    // 等不到人 離開房間吧
                                    OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, parameter) { DebugMessage = "等待超時，已離開房間！" };
                                    SendOperationResponse(response, new SendParameters());

                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion


                        #region KickOther 踢人
                        case (byte)BattleOperationCode.KickOther:
                            {
                                try
                                {
                                    Log.Debug("IN KickOther");

                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    Log.Debug("RoomID: " + roomID);
                                    //to do exit room
                                    Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                    //Log.Debug("playingRoomList Count:" + room.playingRoomList.Count);
                                    //Log.Debug("room.playingRoomList[roomID] Vaule:" + room.playingRoomList[roomID].Values.Count);

                                    // 取得對方玩家>用GUID找Peer
                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                    peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                    // 把他給踢了
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                                    EventData exitEventData = new EventData((byte)BattleResponseCode.KickOther, parameter);
                                    peerOther.SendEvent(exitEventData, new SendParameters());
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region CheckStatus 檢查遊戲狀態
                        case (byte)BattleOperationCode.CheckStatus:
                            {
                                try
                                {
                                    Log.Debug("IN CheckStatus");

                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];

                                    Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                    MPServerPeer peer;

                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);    // 取得其他玩家

                                    Log.Debug("otherActor : " + otherActor);
                                    if (otherActor == null) // 如果他跑了 我也玩不了 離開房間
                                    {
                                        peer = _server.Actors.GetPeerFromGuid(peerGuid);
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                                        EventData eventData = new EventData((byte)BattleResponseCode.Offline, parameter);
                                        peer.SendEvent(eventData, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("CheckStatus Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region SendDamage 發動技能傷害
                        case (byte)BattleOperationCode.SendDamage:
                            {
                                Log.Debug("IN SendDamage");

                                int miceID = (int)operationRequest.Parameters[(byte)BattleParameterCode.MiceID];
                                roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];

                                Room.RoomActor otherActor;
                                otherActor = _server.room.GetOtherPlayer(roomID, primaryID);    // 找對手
                                peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);    // 對手GUID找Peer

                                // 送給對手 技能傷害
                                Dictionary<byte, object> damageParameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MiceID, miceID }, { (byte)BattleResponseCode.DebugMessage, "" } };
                                EventData damageEventData = new EventData((byte)BattleResponseCode.ApplyDamage, damageParameter);
                                peerOther.SendEvent(damageEventData, new SendParameters());
                            }
                            break;
                        #endregion

                        #region LoadPlayerData 載入玩家資料
                        case (byte)PlayerDataOperationCode.Load:
                            {
                                Log.Debug("IN LoadPlayerData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                MPCOM.PlayerDataUI playerDataUI = new MPCOM.PlayerDataUI(); //初始化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                MPCOM.PlayerData playerData = (MPCOM.PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                byte rank = playerData.Rank;
                                byte exp = playerData.EXP;
                                Int16 maxCombo = playerData.MaxCombo;
                                Int32 maxScore = playerData.MaxScore;
                                Int32 sumScore = playerData.SumScore;
                                string miceAll = playerData.MiceAll;            // Json資料
                                string team = playerData.Team;                  // Json資料
                                string miceAmount = playerData.MiceAmount;      // Json資料
                                string friend = playerData.Friend;              // Json資料

                                if (playerData.ReturnCode == "S401")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S401");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.EXP, exp }, 
                                        { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore } ,
                                        { (byte)PlayerDataParameterCode.MiceAll, miceAll } , { (byte)PlayerDataParameterCode.Team, team } ,{ (byte)PlayerDataParameterCode.MiceAmount, miceAmount } , 
                                        { (byte)PlayerDataParameterCode.Friend, friend } 
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region UpdatePlayerData 更新玩家資料
                        case (byte)PlayerDataOperationCode.Update:
                            {
                                Log.Debug("IN UpdatePlayerData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                byte rank = (byte)operationRequest.Parameters[(byte)PlayerDataParameterCode.Rank];
                                byte exp = (byte)operationRequest.Parameters[(byte)PlayerDataParameterCode.EXP];
                                Int16 maxCombo = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                                int maxScore = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxScore];
                                int sumScore = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumScore];
                                string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];        // Json資料
                                string team = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Team];              // Json資料
                                string miceAmount = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAmount];  // Json資料
                                string friend = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Friend];          // Json資料

                                MPCOM.PlayerDataUI playerDataUI = new MPCOM.PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                MPCOM.PlayerData playerData = (MPCOM.PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, miceAll, team, miceAmount, friend)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                if (playerData.ReturnCode == "S401")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S401");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.EXP, exp }, 
                                        { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore } ,
                                        { (byte)PlayerDataParameterCode.MiceAll, miceAll } , { (byte)PlayerDataParameterCode.Team, team } ,{ (byte)PlayerDataParameterCode.MiceAmount, miceAmount } , 
                                        { (byte)PlayerDataParameterCode.Friend, friend } 
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else// 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion


                        #region LoadCurrency 載入貨幣資料
                        case (byte)CurrencyOperationCode.Load:
                            {
                                Log.Debug("IN LoadCurrency");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                MPCOM.CurrencyUI currencyUI = new MPCOM.CurrencyUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                MPCOM.CurrencyData currencyData = (MPCOM.CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account)); 
                                Log.Debug("Data OK");

                                int rice = currencyData.Rice;
                                Int16 gold = currencyData.Gold;

                                if (currencyData.ReturnCode == "S701")  // 取得遊戲貨幣成功 回傳玩家資料
                                {
                                    Log.Debug("currencyData.ReturnCode == S701");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Rice, rice }, { (byte)CurrencyParameterCode.Gold, gold } 
                                    };

                                    OperationResponse response = new OperationResponse((byte)CurrencyResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else  // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region LoadRice 載入遊戲幣資料
                        case (byte)CurrencyOperationCode.LoadRice:
                            {
                                Log.Debug("IN LoadRice");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                MPCOM.CurrencyUI currencyUI = new MPCOM.CurrencyUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                MPCOM.CurrencyData currencyData = (MPCOM.CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                int rice = currencyData.Rice;

                                if (currencyData.ReturnCode == "S703")//取得遊戲幣成功 回傳玩家資料
                                {
                                    Log.Debug("currencyData.ReturnCode == S703");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Rice, rice }
                                    };

                                    OperationResponse response = new OperationResponse((byte)CurrencyResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else// 失敗
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region LoadGold 載入金幣資料
                        case (byte)CurrencyOperationCode.LoadGold:
                            {
                                Log.Debug("IN LoadGold");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                MPCOM.CurrencyUI currencyUI = new MPCOM.CurrencyUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                MPCOM.CurrencyData currencyData = (MPCOM.CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account)); 
                                Log.Debug("Data OK");

                                Int16 gold = currencyData.Gold;

                                if (currencyData.ReturnCode == "S705")//取得金幣成功 回傳玩家資料
                                {
                                    Log.Debug("currencyData.ReturnCode == S705");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Gold, gold } 
                                    };

                                    OperationResponse response = new OperationResponse((byte)CurrencyResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else    // 失敗
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region UpdateScore 更新分數 還沒寫好
                        case (byte)BattleOperationCode.UpdateScore:
                            {
                                try
                                {
                                    Log.Debug("IN UpdateScore");

                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];

                                    byte miceID = (byte)operationRequest.Parameters[(byte)BattleParameterCode.MiceID];
                                    float time = (float)operationRequest.Parameters[(byte)BattleParameterCode.Time];
                                    float eatingRate = (float)operationRequest.Parameters[(byte)BattleParameterCode.EatingRate];
                                    Int16 score = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Score];

                                    MPCOM.BattleUI battleUI = new MPCOM.BattleUI(); //初始化 UI 
                                    Log.Debug("BattleUI OK");
                                    MPCOM.BattleData battleData = (MPCOM.BattleData)TextUtility.DeserializeFromStream(battleUI.ClacScore(miceID, time, eatingRate, score)); //計算分數
                                    Log.Debug("BattleData OK");

                                    score = battleData.score;

                                    if (battleData.ReturnCode == "S501")//計算分數成功 回傳玩家資料
                                    {
                                        //回傳給原玩家
                                        Log.Debug("battleData.ReturnCode == S501");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.Score, score } 
                                    };

                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.UpdateScore, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());

                                        //回傳給另外一位玩家
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                        otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.OtherScore, score }, { (byte)BattleResponseCode.DebugMessage, "取得對方分數資料" } };
                                        EventData getScoreEventData = new EventData((byte)BattleResponseCode.GetScore, parameter2);

                                        peerOther.SendEvent(getScoreEventData, new SendParameters());
                                    }
                                    else
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse((byte)BattleResponseCode.UpdateScore, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region LoadMice 載入老鼠資料
                        case (byte)MiceOperationCode.LoadMice:
                            {
                                try
                                {
                                    Log.Debug("IN LoadMice");

                                    MPCOM.MiceDataUI miceDataUI = new MPCOM.MiceDataUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("LoadMice IO OK");
                                    MPCOM.MiceData miceData = (MPCOM.MiceData)TextUtility.DeserializeFromStream(miceDataUI.LoadMiceData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("LoadMice Data OK");

                                    if (miceData.ReturnCode == "S801")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        Log.Debug("miceData.ReturnCode == S801");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)MiceParameterCode.Ret, miceData.ReturnCode }, { (byte)MiceParameterCode.MiceData,miceData.miceProperty } 
                                    };

                                        OperationResponse response = new OperationResponse((byte)MiceResponseCode.LoadMice, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = miceData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = miceData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("ServerPeer例外情況: " + e.Message);
            }

        }
    }
}