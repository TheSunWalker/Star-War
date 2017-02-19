using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public GameObject StarPrefab;//普通怪预设
    public GameObject BossPrefab;//BOSS预设
    public Transform EnemyParent;//敌人的父物体

    public Image BaseHp;//基地血条
    public Image ShieldImg;//需要改变透明度的护盾图片
    public Text ShieldText;//护盾说明文字
    private int MaxBaseHp = 100;//基地最大血量
    private int CurBaseHp = 0;//基地当前血量
    public float ShieldReduce = 0.6f;//护盾减速效能
    public int Damage = 1;//单次点击伤害量
    public int Money = 0;//金额
    public Text MoneyText;//显示金额的文字
    public GameObject ShopButton;//商店按钮
    public GameObject MuteButton;//静音按钮
    public AudioSource mAudioSource;//
    public AudioClip BaseHitClip;//基地被击中音效
    public AudioClip GameEndClip;//游戏结束音效

    public Image GameEndPanel;//结束界面
    public GameObject RestartButton;//重新开始
    public GameObject QuitButton;//退出按钮
    public Text ResultText;//积分文本

    public Image ShopPanel;//商店界面
    public GameObject CloseShopButton;//关闭商店按钮

    public GameStatus mStatus = GameStatus.Idle;//游戏状态

    void Start()
    {
        Init();
        AddEvents();
    }

    /// <summary>
    /// 添加点击事件
    /// </summary>
    void AddEvents()
    {
        EventTriggerListener.Get(ShopButton).onClick = OnShop;
        EventTriggerListener.Get(MuteButton).onClick = OnMute;
        EventTriggerListener.Get(RestartButton).onClick = OnRestart;
        EventTriggerListener.Get(QuitButton).onClick = OnQuit;
        EventTriggerListener.Get(CloseShopButton).onClick = CloseShop;
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    void Init()
    {
        ShieldReduce = 0.6f;
        Damage = 1;
        BaseHp.fillAmount = 1;
        ShieldImg.CrossFadeAlpha(1, 0, false);
        ShieldText.text = "Speed Reduce: 60%";
        Money = 0;
        MoneyText.text = "0";
        killEnemyCount = 0;
        killBossCount = 0;
        totalEnemyCount = 0;
        totalBossCount = 0;
        mStatus = GameStatus.Gaming;
        LoadShopConfig();
    }

    void Update()
    {
        CheckStart();
        CheckGenerateLogic();
        CheckEnd();
    }

    /// <summary>
    /// 判断启动游戏
    /// </summary>
    void CheckStart()
    {
        if (mStatus == GameStatus.Idle && (Input.GetKeyDown(KeyCode.G) || Input.GetMouseButtonDown(0)))
            mStatus = GameStatus.Gaming;
    }

    public float GenerateIntervalTime = 0.3f;//生成间隔
    private float _lastTime = 0;//上次生成的时间点
    private int totalEnemyCount = 0;//当前总共生成的敌人数量
    public int totalBossCount = 0;//当前总共生成的boss数量，充当难度等级
    public int BossWave = 10;//当totalEnemyCount达到指定BossWave的倍数后生成Boss，随波数递增
    public int killEnemyCount = 0;//当前总共杀死的敌人数量
    public int killBossCount = 0;//当前总共杀死的boss数量，充当难度等级

    void CheckGenerateLogic()
    {
        if(mStatus == GameStatus.Gaming)
        {
            if (Time.time - _lastTime > +GenerateIntervalTime)
            {
                GenerateEnemy();
                _lastTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 判断结束游戏
    /// </summary>
    void CheckEnd()
    {
        if (mStatus == GameStatus.Gaming && Input.GetKeyDown(KeyCode.E))
            mStatus = GameStatus.End;
    }

    /// <summary>
    /// 实例化敌人
    /// </summary>
    /// <param name="num">生成的数量</param>
    void GenerateEnemy()
    {
        bool bBoss = totalEnemyCount > 0 && totalEnemyCount % BossWave == 0;//判断是否是Boss
        GameObject enemy = Instantiate(bBoss ? BossPrefab : StarPrefab);//实例化敌人预设体
        enemy.transform.position = new Vector3(Random.Range(50, Screen.width - 50), Screen.height + 50); //设定生成位置
        enemy.transform.parent = EnemyParent;//加入父物体
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        ai.Init(bBoss);
        if (bBoss)
        {
            totalBossCount++;
            BossWave += 5;
            totalEnemyCount = 0;
        }
        else
            totalEnemyCount++;
    }

    /// <summary>
    /// 生成不计入总数的*小型*敌人,用于技能效果,避免生成boss
    /// </summary>
    /// <param name="num">生成的数量</param>
    /// <param name="point">中心点，生成在这个位置附近</param>
    public void GenerateExtraEnemy(int num, Vector3 point)
    {
        for (int i = 0; i < num; ++i)
        {
            GameObject enemy = Instantiate(StarPrefab);//实例化敌人预设体
            enemy.transform.position = point; //设定生成位置
            enemy.transform.parent = EnemyParent;//加入父物体
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            ai.Init(false);
            float newX = 0;
            while (newX > Screen.width - 50 || newX < 50)
                newX = point.x + Random.Range(-300, 300);
            float newY = point.y + Random.Range(-300, 300);
            Vector3 target = new Vector3(newX, newY);
            ai.Move(target, 0.35f);
        }
    }

    /// <summary>
    /// 基地被星星击中
    /// </summary>
    /// <param name="dmg">伤害量</param>
    public void OnHit(int dmg)
    {
        CurBaseHp -= dmg;
        float percentage = (float) CurBaseHp/MaxBaseHp;
        BaseHp.fillAmount = percentage;
        ShieldReduce = CurBaseHp*0.006f;
        ShieldImg.CrossFadeAlpha(percentage, 0, false);
        ShieldText.text = "Speed Reduce: " + (ShieldReduce*100).ToString("f0") + "%";
        if (CurBaseHp <= 0)
        {
            mAudioSource.PlayOneShot(GameEndClip);
            mStatus = GameStatus.End;
            ShowEnd();
        }
        else
        {
            mAudioSource.PlayOneShot(BaseHitClip);
        }
    }

    /// <summary>
    /// 显示结束界面
    /// </summary>
    void ShowEnd()
    {
        GetGameResult();
        GameEndPanel.rectTransform.DOLocalMove(Vector3.zero, .5f);
    }

    /// <summary>
    /// 获取游戏结果显示在结束界面上
    /// </summary>
    void GetGameResult()
    {
        ResultText.text = killEnemyCount + "\n" + killBossCount + "\n\n" + (killEnemyCount * 10 + killBossCount * 50);
    }

    /// <summary>
    /// 修改金额, *每1血量可获得1金额 *每个技能额外获得1金额
    /// </summary>
    /// <param name="num">修改量</param>
    public void FixMoney(int num)
    {
        Money += num;
        MoneyText.text = Money.ToString();
    }

    /// <summary>
    /// 进入商店
    /// </summary>
    void OnShop(GameObject go)
    {
        Tweener tweener = ShopPanel.rectTransform.DOLocalMoveX(0, .3f);
        tweener.SetUpdate(true);
        mStatus = GameStatus.Pause;
        Time.timeScale = 0;
    }

    /// <summary>
    /// 载入商店界面
    /// </summary>
    void LoadShopConfig()
    {
    }

    /// <summary>
    /// 关闭商店界面
    /// </summary>
    void CloseShop(GameObject go)
    {
        Tweener tweener = ShopPanel.rectTransform.DOLocalMoveX(1200, .3f);
        tweener.SetUpdate(true);
        tweener.OnComplete(ResetTimeScale);
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    void ResetTimeScale()
    {
        mStatus = GameStatus.Gaming;
        Time.timeScale = 1;
    }

    private bool isMute = false;
    void OnMute(GameObject go)
    {
        isMute = !isMute;
        AudioListener.volume = isMute ? 0 : 1;

    }

    void OnRestart(GameObject go)
    {
        Init();
        GameEndPanel.rectTransform.DOLocalMove(new Vector3(0, 2000), .5f);
    }

    void OnQuit(GameObject go)
    {
        Application.Quit();
    }
}
