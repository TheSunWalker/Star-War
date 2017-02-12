using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
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
    public List<int> SkillList = new List<int>();//技能列表

    public EnemyInfo(bool boss, int maxHp, int speed)
    {
        Boss = boss;
        CurHp = MaxHp = maxHp;
        Speed = speed;
    }

    public void SetSkill(List<int> skills)
    {
        SkillList = skills;
    }
}

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
        //EventTriggerListener.Get(ShopButton).onClick = OnShop;
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
        ShieldText.text = "Speed Reduce: 60%";
        Money = 0;
        MoneyText.text = "0";
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
            while (newX > 850 || newX < 50)
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
}
