﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;

public class BattleManager : MonoBehaviour
{
    public BattleAIState battleAIState;           // 產生AI狀態 正式版要改private
    public PlayerAIState playerAIState;             // Player狀態
    public GameObject startAnim;
    public UILabel energyLabel;
    public UISprite gameMode;

    private PoolManager poolManager;        // 物件池     
    private MPFactory mpFactory;            // 老鼠生產者
    private SkillFactory skillFactory;      // 技能工廠
    private MissionManager missionManager;  // 任務管理員
    private BattleHUD battleHUD;            // HUD
    private BotAI BotAI;
    //   public Dictionary<GameObject, GameObject> mice = new Dictionary<GameObject, GameObject>();


    private static Int16 _eggMiceUsage = 0; // 老鼠使用量
    private static Int16 _energyUsage = 0;  // 能量使用量    
    private static Int16 _lostMice = 0;     // 失誤數           統計用
    private static Int16 _spawnCount = 0;   // 產生了多少老鼠   統計用
    private static Int16 _maxCombo = 0;     // 最大連擊數
    private static Int16 _tmpCombo = 0;     // 達到多少連擊 用來判斷連擊中
    private Int16 _killMice = 0;            // 計算打了幾隻老鼠 統計用
    private Int16 _combo = 0;               //  任務用連擊 連擊數

    private static float _energy = 0, _tmpEnergy;       // 能量
    private float _maxScore = 0;            // 最高得分
    private float _gameScore = 0;           // 遊戲進行時所得到的分數
    private float _myDPS = 0;               // 我的平均輸出
    private float _otherDPS = 0;            // 對手的平均輸出
    private static float _scoreRate = 1;           // 分數倍率
    private float _otherRate = 1;           // 對手分數倍率
    private float _lastTime;                // 我的平均輸出
    private float _score = 0;               // 分數
    private float _otherScore = 0;          // 對手分數
    private float _otherEnergy = 0;        // 對手能量
    private float checkTime = 0;             // 檢查時間
    private float checkPoint = 15;          // 檢查時間點
    private static float _elapsedGameTime = 0;     // 經過時間
    private static readonly int _defaultStartTime = 3;     // 經過時間
    private bool _isCombo, flag;                  // 是否連擊
    private bool _isHighScore = false;              // 是否破分數紀錄
    private bool _isHighCombo = false;              // 是否破Combo紀錄
    private bool isSyncStart;               // 同步開始
    private static bool _isPropected = false;   // 錯誤 保護 要在伺服器判斷 偷懶亂寫 
    private static bool _isReflection = false;  // 錯誤 反射 要在伺服器判斷 偷懶亂寫 
    private static bool _isInvincible = false;  // 錯誤 無敵 要在伺服器判斷 偷懶亂寫 
    private Dictionary<int, GameObject> dictSkillBossMice = new Dictionary<int, GameObject>();
    public Dictionary<string, Dictionary<string, object>> dictMiceUseCount = new Dictionary<string, Dictionary<string, object>>();

    public ENUM_BattleAIState battleState = ENUM_BattleAIState.EasyMode;        // 正式版 要改private
    public SpawnStatus spawnStatus = SpawnStatus.LineL;  // 測試用
    public bool tSpawnFlag { get; set; }

    public int missionCombo;


    // Use this for initialization
    void Awake()
    {
        Debug.Log("Battle Start");

        poolManager = GetComponent<PoolManager>();
        mpFactory = GetComponent<MPFactory>();
        missionManager = GetComponent<MissionManager>();
        battleHUD = GetComponent<BattleHUD>();
        SetSpawnState(new EasyBattleAIState());
        playerAIState = new PlayerAIState(this);
        skillFactory = new SkillFactory();

        Global.dictBattleMice.Clear();
        Global.photonService.OtherScoreEvent += OnOtherScore;
        Global.photonService.MissionCompleteEvent += OnMissionComplete;
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.UpdateScoreEvent += OnUpdateScore;
        Global.photonService.OtherMissionScoreEvent += OnOtherMissionComplete;
        Global.photonService.GameStartEvent += OnGameStart;
        Global.photonService.GameOverEvent += OnGameOver;
        Global.photonService.LoadSceneEvent += OnDestory;
        Global.photonService.ApplySkillMiceEvent += OnApplySkillMice;
        Global.photonService.ApplySkillItemEvent += OnApplySkillItem;
        Global.photonService.LoadSceneEvent += OnLoadScene;
        Global.photonService.BossSkillEvent += OnBossSkill;
        Global.isExitingRoom = false;
        _isCombo = flag = false;
        isSyncStart = true;

        _combo = _maxCombo = _tmpCombo = _eggMiceUsage = _energyUsage = _lostMice = _killMice = _spawnCount = 0;
        _elapsedGameTime = _score = _maxScore = _gameScore = _otherScore = 0;
        _energy = 0;
        checkPoint = 3;
        initMiceUseCount();
    }

    /// <summary>
    /// 建立 初始 老鼠用量
    /// </summary>
    private void initMiceUseCount()
    {
        Debug.Log(Global.dictTeam.Count);
        List<string> keys = new List<string>(Global.dictTeam.Keys);
        Dictionary<string, object> data;

        foreach (string key in keys)
        {
            data = new Dictionary<string, object>();
            data.Add("UseCount", 0);
            Debug.Log(short.Parse(key).ToString());
            dictMiceUseCount.Add(key, data);
        }
    }

    void OnGUI()
    {
        switch (battleState)
        {
            case ENUM_BattleAIState.EasyMode:
                gameMode.spriteName = "Easy Mode";
                break;
            case ENUM_BattleAIState.NormalMode:
                gameMode.spriteName = "NormalMode";
                break;
            case ENUM_BattleAIState.HardMode:
                gameMode.spriteName = "HardMode";
                break;
            case ENUM_BattleAIState.CarzyMode:
                gameMode.spriteName = "CarzyMode";
                break;
            case ENUM_BattleAIState.EndTimeMode:
                gameMode.spriteName = "EndMode";
                break;
        }
    }

    private void UpdateEnergyText()
    {
        float percent = _energy / 100f;
        //if (percent < .2 )
        //    energyLabel.text = "0 / 5";
        //if (percent >= .2 && percent < .4)
        //    energyLabel.text = "1 / 5";
        //if (percent >= .4 && percent < .6)
        //    energyLabel.text = "2 / 5";
        //if (percent >= .6 && percent < .8)
        //    energyLabel.text = "3 / 5";
        //if (percent >= .8 && percent < 1)
        //    energyLabel.text = "4 / 5";
        //if (percent == 1)
        //    energyLabel.text = "5 / 5";

        energyLabel.text = Math.Floor(energy * 100).ToString();

    }
    public void OnExit()
    {
        Global.photonService.KickOther();
        Global.photonService.ExitRoom();
    }

    void Update()
    {
        // update energy text
        UpdateEnergyText();
        battleState = battleAIState.GetState();
        GameConnStatusChk();
        if (!Global.isGameStart)
            _lastTime = Time.time; // 沒作用

        // 同步開始遊戲
        if (poolManager.mergeFlag && poolManager.poolingFlag && isSyncStart && poolManager.dataFlag)
        {
            Debug.Log("Pooling Completed Start SyncGame");
            isSyncStart = false;
            Global.photonService.SyncGameStart();
        }

        if (Global.isGameStart && Time.time > _lastTime + _defaultStartTime)
        {
            _elapsedGameTime = Time.time - _lastTime - _defaultStartTime;    // 遊戲經過時間
            battleState = battleAIState.GetState();
            if (_combo > _maxCombo) _maxCombo = _combo;     // 假如目前連擊數 大於 _maxCombo  更新 _maxCombo

            if (Global.MemberType == MemberType.Bot && !flag)
            {
                BotAI = new BotAI(poolManager.GetPoolMiceIDs(), poolManager.GetPoolSkillIDs());
                flag = !flag;
            }


            BotAI.UpdateAI();

            // 更新最高分
            if (_score > _maxScore)
            {
                _maxScore = _score;
                _isHighScore = true;
            }
            
            // 遊戲結束判斷
            if (_elapsedGameTime >= Global.GameTime)
            {
                Global.isGameStart = false;
                List<string> columns = new List<string>();
                columns.Add("UseCount");
                columns.Add("Rank");
                columns.Add("Exp");
                columns.Add("ItemCount");
                string jUseCount = MiniJSON.Json.Serialize(dictMiceUseCount);
                Debug.Log("jUseCount:" + jUseCount);
                Global.photonService.GameOver((short)_gameScore, (short)_otherScore, (short)_elapsedGameTime, _maxCombo, _killMice, _lostMice, spawnCount, jUseCount, columns.ToArray());
                Debug.Log("GameOver Time!" + _elapsedGameTime);
            }
        }
    }

    void FixedUpdate()
    {
        if (Global.isGameStart && Time.time > _lastTime + 3)
        {

            battleAIState.UpdateState();            // 更新SpawnState邏輯 這裡寫時間 Time + lastime + 間隔
            playerAIState.UpdatePlayerState();      // 更新PlayerState邏輯


            if (_elapsedGameTime % 5 == 0)
            {
                _myDPS = _score / Time.timeSinceLevelLoad;
                _otherDPS = _otherScore / Time.timeSinceLevelLoad;
                //Debug.Log("_myDPS: " + _myDPS + "\n  _otherDPS: " + _otherDPS);
            }
        }
    }


    #region UpadateScore 更新分數
    public void UpadateScore(short miceID, float aliveTime)
    {
        if (miceID != -1 && miceID > 10000 && miceID < 20000)
        {
            UpadateCombo();
            missionCombo++;
            _spawnCount++;
            _killMice++;
            Global.MiceCount--;
            Global.photonService.UpdateScore(miceID, _combo, aliveTime);
        }
    }
    #endregion

    #region LostScore 失去分數
    public void LostScore(short miceID, float aliveTime)
    {
        if (!_isInvincible)
        {
            //計分公式 存活時間 / 食量 / 吃東西速度 ex:4 / 1 / 0.5 = 8
            if (miceID != -1 && miceID > 10000 && miceID < 20000)
            {
                if (!_isPropected)
                    BreakCombo();
                Global.photonService.UpdateScore(miceID, _combo, aliveTime);
                _spawnCount++;
                _lostMice++;
                Global.MiceCount--;
            }
        }
    }
    #endregion

    #region UpadateEnergy 更新能量
    public void UpadateEnergy(float value)
    {
        _energy += value;

        _energy = Math.Min(_energy, 1);
        _energy = Math.Max(_energy, 0);
        //        Debug.Log(_energy);
    }
    #endregion

    # region UpdateUseCount 更新使用量
    public void UpdateUseCount(short miceID, short useCount)
    {
        Dictionary<string, object> data;
        dictMiceUseCount.TryGetValue(miceID.ToString(), out data);
        data["UseCount"] = useCount;
    }
    #endregion

    private void UpadateCombo()
    {
        _tmpCombo++;
        //如果還在連擊 增加連擊數

        if (_isCombo)
            _combo++;

        if (_tmpCombo == 5 && !_isCombo)
        {
            _isCombo = true;
            _combo = 5;
            _tmpCombo = 0;
        }

        if (_isCombo)
            battleHUD.ComboMsg(_combo);
    }

    public void BreakCombo()
    {
        if (!_isInvincible || !_isPropected && _combo != 0)
        {
            _isCombo = false;           // 結束 連擊
            _combo = 0;                 // 恢復0
            _tmpCombo = 0;
            battleHUD.ComboMsg(_combo);
            battleAIState.SetValue(0, 0, battleAIState.GetIntervalTime() + .1f, 0);
        }
    }

    void OnUpdateScore(Int16 score, Int16 energy)    // 更新分數時
    {
        //Debug.Log("Get Energy:" + energy);
        _tmpEnergy = energy*10;

        if (_isPropected || _isInvincible)
            score = Math.Max((short)0, score);
        //        Debug.Log("(Update)OnUpdateScore" + value);
        if (Global.isGameStart)
        {
            Int16 _tmpScore = (Int16)(score * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，則不取得自己增加的分數
            if (missionManager.missionMode == MissionMode.Opening)
            {
                if (missionManager.mission == Mission.Exchange && score > 0)
                {
                    _otherScore += _tmpScore;
                    if (_combo >= 5)
                        UpadateEnergy(_tmpEnergy / 100);
                }

                if (missionManager.mission == Mission.Exchange && score < 0)    // 如果再交換分數任務下，則取得自己減少的分數
                    _score = (this.score + _tmpScore > 0) ? (_score += _tmpScore) : 0;
            }

            if (missionManager.mission != Mission.Exchange)
            {
                _score = (this.score + _tmpScore < 0) ? 0 : _score += _tmpScore;
                if (_tmpScore > 0) _gameScore += _tmpScore;
            }

            if (_combo >= 5)
                UpadateEnergy(_tmpEnergy / 100);
        }
        //        Debug.Log(_tmpEnergy);
    }



    void OnOtherScore(Int16 value, Int16 energy)     // 接收對手分數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ＢＵＧ　接收對手分數要ｘ對方的倍率＊＊＊＊＊＊＊＊＊＊＊＊
    {
        if (Global.isGameStart)
        {
            Int16 _tmpScore = (Int16)(value * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，取得對方增加的分數
            if (missionManager.missionMode == MissionMode.Opening)
            {
                if (missionManager.mission == Mission.Exchange && value > 0)
                {
                    _score += _tmpScore;
                    _gameScore += _tmpScore;
                }

                if (missionManager.mission == Mission.Exchange && value < 0)
                    _otherScore = (this.otherScore + _tmpScore > 0) ? (_otherScore += _tmpScore) : 0;

                if (missionManager.mission == Mission.HarvestRate)
                {
                    Int16 otherScore = (Int16)(value * _otherRate);
                    _otherScore = (this.otherScore + otherScore > 0) ? _otherScore += otherScore : 0;
                }
            }

            if (missionManager.mission != Mission.Exchange && missionManager.mission != Mission.HarvestRate)
                _otherScore = (this.otherScore + _tmpScore < 0) ? 0 : _otherScore += _tmpScore;


            _otherEnergy = Math.Min((energy / 100), 1f); //   1/100  1%

        }
    }

    void OnMissionComplete(Int16 missionReward)
    {
        Debug.Log("On BattleManager missionReward: " + missionReward);
        if (Global.isGameStart)
        {
            if (missionManager.mission == Mission.HarvestRate)
            {
                _scoreRate = 1;
            }
            else
            {
                _score = (this.score + missionReward < 0) ? 0 : _score += missionReward;
            }

            if (missionManager.mission == Mission.DrivingMice)
            {
                missionCombo = 0;
            }
            //            Debug.Log("(Battle)_score:" + _score);
        }
    }

    void OnOtherMissionComplete(Int16 otherMissionReward)
    {
        if (Global.isGameStart)
        {
            if (missionManager.mission == Mission.HarvestRate)
            {
                _scoreRate = 1;
            }
            else
            {
                _otherScore = (this.otherScore + otherMissionReward < 0) ? 0 : _otherScore += otherMissionReward;
            }

            //            Debug.Log("(Battle)_otherScore:" + _otherScore);
        }
    }


    private void OnApplyMission(Mission mission, short value)
    {
        float msgValue = 0;

        if (Global.isGameStart)
        {


            switch (mission)
            {
                case Mission.Harvest: // 收穫
                    {
                        msgValue = value;
                        break;
                    }
                case Mission.DrivingMice: // 趕老鼠
                    {
                        missionCombo = 0;
                        msgValue = value;
                        break;
                    }
                case Mission.Exchange: // 交換分數
                    {
                        break;
                    }
                case Mission.HarvestRate:
                    {
                        Debug.Log("Rate:" + value);
                        if (_score > _otherScore)
                        {
                            _scoreRate -= ((float)value / 100); //scoreRate 在伺服器以整數百分比儲存 這裡是0~1 所以要/100
                        }
                        else
                        {
                            _scoreRate += ((float)value / 100); //scoreRate 在伺服器以整數0~100 儲存 這裡是0~1 所以要/100
                        }
                        msgValue = _scoreRate;
                        _otherRate = 1 - _scoreRate;
                        _otherRate = (_otherRate == 0) ? 1 : _otherRate = 1 + _otherRate;                       // 如果 1(原始) - 1(我) = 0 代表倍率相同不調整倍率 ; 如果1 - 0.9(我) = 0.1 代表他多出0.1 ;  如果1 - 1.1(我) = -0.1 代表他少0.1 。最後 1+(+-0.1)就是答案
                        break;
                    }
                case Mission.Reduce: // 減少分數
                    {
                        break;
                    }

                case Mission.WorldBoss:
                    {
                        mpFactory.SpawnBoss(value, 0.1f, 0.1f, 6, 60);    //missionScore這裡是HP SpawnBoss 的屬性錯誤 手動輸入的
                        break;
                    }
            }
        }
    }
    private void GameConnStatusChk()
    {
        if (checkTime > checkPoint)
        {
            Global.photonService.CheckStatus();
            checkPoint += checkPoint;
            Debug.Log("Check");
        }

        checkTime += Time.deltaTime;
    }


    void OnBossSkill(ENUM_Skill skill)
    {
        SetPlayerState((short)skill);
    }


    void OnGameOver(int score, short exp, short sliverReward, short goldReward, string jItemReward, string evaluate, short battleResult)
    {
        bool result;
        result = (battleResult > 0) ? true : false;
        battleHUD.GoodGameMsg(score, result, exp, sliverReward, goldReward, jItemReward, _combo, _killMice, _lostMice, evaluate, _isHighScore, _isHighCombo);
        Global.photonService.LoadPlayerData(Global.Account);
    }

    void OnGameStart()
    {
        Global.isGameStart = true;
        startAnim.SetActive(true);
        Debug.Log(" ----  Game Start!  ---- ");
    }

    public void OnExitRoom()
    {
        Global.photonService.ExitRoom();
        Debug.Log("ExitRoom");
    }

    private void OnDestory()                       // 離開房間時
    {
        Global.isExitingRoom = true;        // 離開房間了
        Global.isMatching = false;          // 無配對狀態
        Global.BattleStatus = false;        // 不在戰鬥中

        // 取消委派
        Global.photonService.OtherScoreEvent -= OnOtherScore;
        Global.photonService.MissionCompleteEvent -= OnMissionComplete;
        Global.photonService.ApplyMissionEvent -= OnApplyMission;
        Global.photonService.UpdateScoreEvent -= OnUpdateScore;
        Global.photonService.OtherMissionScoreEvent -= OnOtherMissionComplete;
        Global.photonService.GameStartEvent -= OnGameStart;
        Global.photonService.GameOverEvent -= OnGameOver;
        Global.photonService.LoadSceneEvent -= OnDestory;
    }

    public Dictionary<int, GameObject> GetSkillBossMice()
    {
        return dictSkillBossMice;
    }


    public AssetLoader GetAssetLoader()
    {
        return poolManager.gameObject.GetComponent<AssetLoader>();
    }






    private void SetPlayerState(short skillID)
    {
        Debug.Log("SetPlayerState:" + skillID);
        // BattleHUD show skill image
        SkillBase skill = skillFactory.GetSkill(Global.dictSkills, skillID);
        skill.SetAIController(playerAIState);
        playerAIState.SetPlayerAIState(skill.GetPlayerState(), skill);
        skill.Display();
    }


    public void SetSpawnState(BattleAIState spawnState)
    {
        this.battleAIState = spawnState;
        this.battleAIState.SetAIController(this);
    }




    void OnApplySkillMice(short miceID)     // 收到技能攻擊 
    {
        Debug.Log("OnApplySkill miceID:" + miceID);

        //   0.OnApplySkill需要改寫，接收數量、參數
        mpFactory.SpawnByRandom(miceID, (sbyte[])SpawnData.GetSpawnData(SpawnStatus.LineL), 1.5f, 0.25f, 0.25f, 6, true);
    }

    void OnApplySkillItem(short itemID)     // 收到技能攻擊 
    {
        if (!_isReflection || !_isPropected)
        {
            Debug.Log("OnApplySkillItem itemID:" + itemID);

            short skillID = Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "SkillID", itemID.ToString()));

            SetPlayerState(skillID);
            // to do Display Skill
        }
    }

    void OnLoadScene()
    {
        Global.photonService.ApplySkillMiceEvent -= OnApplySkillMice;
        Global.photonService.ApplySkillItemEvent -= OnApplySkillItem;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }

    public void OnHighCombo()
    {
        battleAIState.SetValue(0, 0, battleAIState.GetIntervalTime() - .1f, 0);
    }

    /// <summary>
    /// 限測試使用
    /// </summary>
    /// <param name="lerpTime"></param>
    /// <param name="spawnTime"></param>
    /// <param name="intervalTime"></param>
    /// <param name="spawnCount"></param>
    public void SetValue(float lerpTime, float spawnTime, float intervalTime, int spawnCount)
    {
        battleAIState.SetValue(lerpTime, spawnTime, intervalTime, spawnCount);
    }



    //---亂寫區域 ---
    public static void SetInvincible(bool value)
    {
        _isInvincible = value;
    }
    public static void SetPropected(bool value)
    {
        _isPropected = value;
    }
    public static void SetRefelcetion(bool value)
    {
        _isReflection = value;
    }

    //---亂寫區域 ---

    // 外部取用 戰鬥資料
    public Int16 combo { get { return _combo; } }
    public Int16 maxCombo { get { return _maxCombo; } }
    public int tmpCombo { get { return _tmpCombo; } }
    public static float gameTime { get { return _elapsedGameTime; } }
    public float score { get { return _score; } }
    public float gameScore { get { return _gameScore; } }
    public float otherScore { get { return _otherScore; } }
    public float otherEnergy { get { return _otherEnergy; } }
    public float Energy { get { return _energy; } }
    public float maxScore { get { return _maxScore; } }
    public int eggMiceUsage { get { return _eggMiceUsage; } }
    public int energyUsage { get { return _energyUsage; } }
    public Int16 lostMice { get { return _lostMice; } }
    public int hitMice { get { return _killMice; } }
    public Int16 spawnCount { get { return _spawnCount; } }
    public float myDPS { get { return _myDPS; } }
    public float otherDPS { get { return _otherDPS; } }
    public static double energy { get { return _energy; } }
    public bool isCombo { get { return _isCombo; } }

    /* 超亂亂數
* using System;
public Guid RNGGuid() // 超亂亂數
{
var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
var data = new byte[16];
rng.GetBytes(data);
return new Guid(data);
}
*/


}
