using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum GameStatus
{
    //游戏状态：等待，游戏中，暂停，结束
    Idle,Gaming,Pause,End
}

/// <summary>
/// 敌人类
/// </summary>
public class EnemyInfo
{
    public bool Boss;//是否是boss
    public int MaxHp;//最大血量
    public int CurHp;//当前血量
    public int Speed;//移动速度

    public EnemyInfo(bool boss, int maxHp, int speed)
    {
        Boss = boss;
        CurHp = MaxHp = maxHp;
        Speed = speed;
    }
}

public class GameController : MonoBehaviour
{
    private static GameController instance;

    public static GameController Instance
    {
        get { return instance; }
        set { instance = value; }
    }

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
    public Text[] ShieldText;//护盾说明文字,0为Shield,1为效能说明
    private int MaxBaseHp = 100;//基地最大血量
    private int CurBaseHp = 0;//基地当前血量
    public float ShieldReduce = 0.6f;//护盾减速效能
    public int Damage = 1;//单次点击伤害量

    public GameStatus mStatus = GameStatus.Idle;//游戏状态

    void Start()
    {
        Init();
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    void Init()
    {
        CurBaseHp = MaxBaseHp = 100;
        ShieldReduce = 0.6f;
        Damage = 1;
        BaseHp.fillAmount = 1;
        ShieldImg.CrossFadeAlpha(1, 0, false);
        ShieldText[1].text = "Speed Reduce: 60%";
        for (int i = 0; i < ShieldText.Length; i++)
            ShieldText[i].CrossFadeAlpha(1, 0, false);
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
        if (mStatus == GameStatus.Idle && Input.GetKeyDown(KeyCode.G))
            mStatus = GameStatus.Gaming;
    }

    public float GenerateIntervalTime = 0.3f;//生成间隔
    private float _lastTime = 0;//上次生成的时间点
    private int totalEnemyCount = 0;//当前总共生成的敌人数量
    public int totalBossCount = 0;//当前总共生成的boss数量，充当难度等级
    public int BossWave = 10;//当totalEnemyCount达到指定BossWave的倍数后生成Boss，随波数递增
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
        enemy.transform.position = new Vector3(Random.Range(50, 851), 1650); //设定生成位置
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
        ShieldText[1].text = "Speed Reduce: " + (ShieldReduce*100).ToString("f0") + "%";
        ShieldText[0].CrossFadeAlpha(percentage, 0, false);
        //for (int i = 0; i < ShieldText.Length; i++)
        //    ShieldText[i].CrossFadeAlpha(percentage, 0, false);
    }
}
