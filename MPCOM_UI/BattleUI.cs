﻿using System;
using System.EnterpriseServices;
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
 * 對戰界面層 提供外部存取使用
 * 計算分數
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface IBattleUI  // 使用介面 可以提供給不同程式語言繼承使用        
    {
        byte[] ClacScore(byte miceID, float time, float eatingRate, Int16 score);
    }

    public class BattleUI : ServicedComponent, IBattleUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region ClacScore 計算老鼠命中分數
        /// <summary>
        /// 計算老鼠命中分數
        /// </summary>
        /// <param name="miceID"></param>
        /// <param name="time"></param>
        /// <param name="eatingRate"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public byte[] ClacScore(byte miceID, float time,float eatingRate, Int16 score)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.ClacScore(miceID, time,eatingRate, score);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace; ;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion

        #region ClacScore 計算任務完成的分數
        /// <summary>
        /// 計算任務完成的分數
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="missionRate"></param>
        /// <returns></returns>
        public byte[] ClacScore(byte mission , float missionRate)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.ClacScore(mission,missionRate);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: "+e.StackTrace;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion

        #region SelectMission 選擇任務
        /// <summary>
        /// 計算任務完成的分數
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="missionRate"></param>
        /// <returns></returns>
        public byte[] SelectMission(byte mission, float missionRate)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.SelectMission(mission, missionRate);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion
    }
}
