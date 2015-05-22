﻿using System;

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
 * 這個檔案是用來儲存COM+ 玩家資料所使用
 * 
 * ***************************************************************/

namespace MPCOM
{
    [Serializable()]
    
    public struct PlayerData
    {
        public string ReturnCode; //回傳值
        public string ReturnMessage; //回傳說明文字<30全形字

        public byte Rank;			// 等級
        public byte EXP;			// 經驗
        public Int16 MaxCombo;		// 最大Combo數
        public int MaxScore;		// 最高分數
        public int SumScore;		// 總分數
        public string MiceAll;		// 所有老鼠		＊＊＊類型ＪＳＯＮ＊＊＊＊
        public string Team;			// 隊伍			＊＊＊類型ＪＳＯＮ＊＊＊＊
        public string MiceAmount;	// 老鼠數量		＊＊＊類型ＪＳＯＮ＊＊＊＊
        public string Friend;		// 好友			＊＊＊類型ＪＳＯＮ＊＊＊＊






        /*
        public Int32 total_inventory_limted; //背包 數量限制
        public Int32 total_miceCost_limted; //老鼠 花費限制
        public Int32 total_inventory_character; //玩家 全部 老鼠資料
        public Int32 total_inventory_equipment; //玩家 裝備 老鼠資料

        */

    }
}