﻿using UnityEngine;
using System.Collections;
using MPProtocol;

public class BattleHUD : MonoBehaviour
{
    float ckeckTime;

    public GameObject HPBar;
    public GameObject ComboLabel;
    public GameObject BlueScore;
    public GameObject RedScore;
<<<<<<< HEAD
=======
    public GameObject EnergyBar;
    public GameObject MissionObject;
    public GameObject WaitObject;
    public GameObject StartObject;
    public GameObject ScorePlusObject;
    public GameObject OtherPlusObject;
>>>>>>> test

    [Range(0.1f, 1.0f)]
    public float _beautyHP;                // 美化血條用

    private BattleManager battleManager;


    void Start()
    {
        battleManager = GetComponent<BattleManager>();

        Global.photonService.WaitingPlayerEvent += OnWaitingPlayer;
        Global.photonService.ExitRoomEvent += OnExitRoom;
    }

    void Update()
<<<<<<< HEAD
    {  
=======
    {
        #region 動畫類判斷 DisActive
        if (WaitObject.activeSelf)
        {
            Animator waitAnims = WaitObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo waitState = waitAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (waitState.nameHash == Animator.StringToHash("Layer1.Waiting"))                  // 如果 目前 動化狀態 是 Waiting
                if (waitState.normalizedTime > 0.1f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (StartObject.activeSelf)
        {
            Animator startAnims = WaitObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo startState = startAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (startState.nameHash == Animator.StringToHash("Layer1.Waiting"))                  // 如果 目前 動化狀態 是 Waiting
                if (startState.normalizedTime > 3.0f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (MissionObject.activeSelf)
        {
            Animator missionAnims = WaitObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo waitState = missionAnims.GetCurrentAnimatorStateInfo(0);          // 取得目前動畫狀態 (0) = Layer1

            if (waitState.nameHash == Animator.StringToHash("Layer1.FadeIn"))                   // 如果 目前 動化狀態 是 FadeIn
                if (waitState.normalizedTime > 1.5f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
            if (waitState.nameHash == Animator.StringToHash("Layer1.Completed"))                // 如果 目前 動化狀態 是 Completed
                if (waitState.normalizedTime > 1.5f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (ScorePlusObject.activeSelf)
        {
            Animator startAnims = WaitObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo startState = startAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (startState.nameHash == Animator.StringToHash("Layer1.ScorePlus"))                  // 如果 目前 動化狀態 是 Waiting
                if (startState.normalizedTime > 1.0f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (OtherPlusObject.activeSelf)
        {
            Animator startAnims = WaitObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo startState = startAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (startState.nameHash == Animator.StringToHash("Layer1.ScorePlus"))                  // 如果 目前 動化狀態 是 Waiting
                if (startState.normalizedTime > 1.0f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }
        #endregion

>>>>>>> test
        if (ckeckTime > 15)
        {
            Global.photonService.CheckStatus();
            ckeckTime = 0;
        }
        else
        {
            ckeckTime += Time.deltaTime;
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 100), "ExitRoom"))
        {
            Global.photonService.KickOther();
            Global.photonService.ExitRoom();
        }

        BlueScore.GetComponent<UILabel>().text = battleManager.score.ToString();         // 畫出分數值
        RedScore.GetComponent<UILabel>().text = battleManager.otherScore.ToString();     // 畫出分數值

        float value = battleManager.score / (battleManager.score + battleManager.otherScore);                      // 得分百分比 兩邊都是0會 NaN

        if (_beautyHP == value)                                             // 如果HPBar值在中間 (0.5=0.5)
        {
            HPBar.GetComponent<UISlider>().value = value;
        }
        else if (_beautyHP > value)                                         // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;               // 先等於目前值，然後慢慢減少

            if (_beautyHP >= value)
                _beautyHP -= 0.01f;                                         // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (_beautyHP < value)                                         // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;               // 先等於目前值，然後慢慢增加

            if (_beautyHP <= value)
                _beautyHP += 0.01f;                                         // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (battleManager.score == 0 && battleManager.otherScore == 0)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;
            if (_beautyHP <= HPBar.GetComponent<UISlider>().value && HPBar.GetComponent<UISlider>().value > 0.5f)
            {
                _beautyHP -= 0.01f;
            }

            if (_beautyHP >= HPBar.GetComponent<UISlider>().value && HPBar.GetComponent<UISlider>().value < 0.5f)
                _beautyHP += 0.01f;
        }

<<<<<<< HEAD
        ComboLabel.GetComponent<UILabel>().text = battleManager.combo.ToString();        // 畫出Combo值

=======
        #region Energy Bar動畫

        if (battleManager.energy == Math.Round(_beautyEnergy, 6))
        {
            EnergyBar.GetComponent<UISlider>().value = (float)(_beautyEnergy = battleManager.energy);
        }

        if (battleManager.energy > _beautyEnergy)                           // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        {
            EnergyBar.GetComponent<UISlider>().value = (float)_beautyEnergy;           // 先等於目前值，然後慢慢減少
            _beautyEnergy = Mathf.Lerp((float)_beautyEnergy, (float)battleManager.energy, 0.1f);                                        // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (battleManager.energy < _beautyEnergy)                      // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        {
            EnergyBar.GetComponent<UISlider>().value = (float)_beautyEnergy;           // 先等於目前值，然後慢慢增加
            _beautyEnergy = Mathf.Lerp((float)_beautyEnergy, (float)battleManager.energy, 0.1f);                                        // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        }
        else
        {
            EnergyBar.GetComponent<UISlider>().value = (float)battleManager.energy;
        }

        #endregion
        ComboLabel.GetComponent<UILabel>().text = battleManager.combo.ToString();        // 畫出Combo值

        //        Debug.Log("_beautyEnergy: " + _beautyEnergy);
        //        Debug.Log("battleManager.energy: " + battleManager.energy);
>>>>>>> test
    }


    public void HPBar_Shing()
    {
        Debug.Log("HPBar_Shing !");
    }

    public void MissionMsg(Mission mission, float value)
    {
        MissionObject.SetActive(true);
        switch (mission)
        {
            case Mission.Harvest:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = value.ToString();
                Debug.Log("Mission : Harvest! 收穫:" + value + " 糧食");
                break;
            case Mission.HarvestRate:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫增加       ";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = value.ToString();
                Debug.Log("Mission : HarvestRate UP+! 收穫倍率:" + value);
                break;
            case Mission.Exchange:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "交換收穫的糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission : Exchange! 交換收穫的糧食");
                break;
            case Mission.Reduce:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "豐收祭典     糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = value.ToString();
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + value + " 糧食");
                break;
            case Mission.DrivingMice:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "驅趕老鼠       隻";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = value.ToString();
                Debug.Log("Mission : DrivingMice! 驅趕老鼠 數量: " + value + " 隻");
                break;
            case Mission.WorldBoss:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "世界王出現!!";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission WARNING 世界王出現!!");
                break;
        }
        MissionObject.transform.GetChild(2).GetComponent<UILabel>().text = "Mission";
        MissionObject.GetComponent<Animator>().Play("FadeIn");
    }

    public void MissionCompletedMsg(Mission mission, float missionReward)
    {
        if (missionReward != 0)     // ScorePlus 動畫
        {
            ScorePlusObject.SetActive(true);

            if (missionReward > 0)
            {
                ScorePlusObject.transform.GetChild(0).GetComponent<UILabel>().text = "+" + missionReward.ToString();
            }
            else if (missionReward < 0)
            {
                ScorePlusObject.transform.GetChild(0).GetComponent<UILabel>().text = missionReward.ToString();
            }
            ScorePlusObject.GetComponent<Animator>().Play("ScorePlus");
        }

        // Mission Completed 動畫
        MissionObject.SetActive(true);
        switch (mission)
        {
            case Mission.Harvest:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                Debug.Log("Mission : Completed! 取得: " + missionReward + " 糧食");
                break;
            case Mission.HarvestRate:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫倍率復原";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission 收穫倍率復原 = 1");
                break;
            case Mission.Exchange:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務結束:不再交換糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission 任務結束:不再交換糧食");
                break;
            case Mission.Reduce:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "豐收祭典任務結束";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + missionReward + " 糧食");
                break;
            case Mission.DrivingMice:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
            case Mission.WorldBoss:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
        }
        MissionObject.transform.GetChild(2).GetComponent<UILabel>().text = "Completed!";
        MissionObject.GetComponent<Animator>().Play("Completed");
    }

    public void OtherScoreMsg(float missionReward)
    {
        if (missionReward != 0) // ScorePlus 動畫
        {
            OtherPlusObject.SetActive(true);

            if (missionReward > 0)
            {
                OtherPlusObject.transform.GetChild(0).GetComponent<UILabel>().text = "+" + missionReward.ToString();
            }
            else if (missionReward < 0)
            {
                OtherPlusObject.transform.GetChild(0).GetComponent<UILabel>().text = missionReward.ToString();
            }
            OtherPlusObject.GetComponent<Animator>().Play("ScorePlus");
        }

        Debug.Log("Other MissionCompleted! + " + missionReward);
    }

    public void MissionFailedMsg()
    {
        MissionObject.SetActive(true);
        MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務失敗";
        MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
        MissionObject.transform.GetChild(2).GetComponent<UILabel>().text = "Mission Failed!";
        MissionObject.GetComponent<Animator>().Play("Completed");
        Debug.Log("Mission Failed!");
    }



    void OnWaitingPlayer()
    {
        if (!Global.isGameStart)
        {
            WaitObject.transform.gameObject.SetActive(true);
        }
        else
        {
            WaitObject.GetComponent<Animator>().Play("Wait");
        }
    }

    void OnExitRoom()
    {
        Global.photonService.WaitingPlayerEvent -= OnWaitingPlayer;
        Global.photonService.ExitRoomEvent -= OnExitRoom;
    }
}