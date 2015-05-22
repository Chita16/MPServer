﻿namespace MPProtocol
{
    public enum MatchGameOperationCode
    {
        MatchGame = 31,             // 配對遊戲
        JoinRoom = 32,              // 加入房間
        QuitRoom = 33,              // 離開房間
        ExitWaiting = 35,           // 離開等待房間
    }

    public enum MatchGameResponseCode
    {
        Match = 34,                 // 配對成功
        ExitWaiting = 35,           // 離開等待房間
    }

    public enum MatchGameParameterCode
    {
        PrimaryID,                  // 主索引
        RoomID,                     // 房間ID
        Nickname,                   // 暱稱
        Team,                       // 隊伍資料
    }
}