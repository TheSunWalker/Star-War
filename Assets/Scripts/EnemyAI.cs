using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyAI : MonoBehaviour
{
    public EnemyInfo Info;//各项数据
    public float ForceDestroyTime = 10;//强制销毁时间
    private float generateTime = 0;//生成时间
    public Text HpText;//显示血量
    public AudioClip HitClip;//被击中音效
    public AudioClip EndClip;//死亡音效
    private AudioSource mAudioSource;//

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="boss">是否是Boss</param>
    public void Init(bool boss)
    {
        mAudioSource = GetComponent<AudioSource>();
        int diff = GameController.Instance.totalBossCount;
        generateTime = Time.time;
        int maxHp = 0;
        int speed = 0;
        if (boss)
        {
            maxHp = 8;
            speed = 100;
        }
        else
        {
            int min = 1 + (diff > 5 ? diff - 5 : 0);
            min = min > 5 ? 5 : min;
            int max = diff > 6 ? 6 : diff;
            maxHp = Random.Range(min, max);
            speed = 100 + (int)(diff*5f);
        }
        Info = new EnemyInfo(boss, maxHp, speed);
        if (!boss)
        {
            List<int> SkillList = new List<int>();
            List<int> skills = new List<int>();
            if (GameController.Instance.totalBossCount >= 2)//第2波开始加入*加速*技能
                SkillList.Add(101);
            if (GameController.Instance.totalBossCount >= 4)//第4波开始加入*闪烁*技能
                SkillList.Add(103);
            if (GameController.Instance.totalBossCount >= 6)//第6波开始加入*隐身*技能
                SkillList.Add(102);
            int skillNum = Random.Range(0, SkillList.Count + 1);
            for (int i = 0; i < skillNum; ++i)
            {
                int id = SkillList[Random.Range(0, SkillList.Count)];
                skills.Add(id);
            }
            Info.SetSkill(skills);
        }
        for (int i = 0; i < Info.SkillList.Count; ++i)
        {
            if (Info.SkillList[i] != 103)
                SkillManager.Instance.DoSkill(Info.SkillList[i], gameObject);
        }
        EventTriggerListener.Get(gameObject).onClick = OnHit;
    }

    void Update()
    {
        CheckLogic();
        CheckDestroy();
    }

    /// <summary>
    /// 主逻辑
    /// </summary>
    void CheckLogic()
    {
        if (Info == null)
            return;
        transform.Translate(0, -Info.Speed*Time.deltaTime, 0);
        HpText.text = Info.CurHp.ToString();
    }

    /// <summary>
    /// 如果存活的时间过长，强制销毁
    /// </summary>
    void CheckDestroy()
    {
        if (Time.time - generateTime >= ForceDestroyTime || GameController.Instance.mStatus == GameStatus.End)
            Destroy(gameObject);
    }

    /// <summary>
    /// 进入触发器
    /// </summary>
    void OnTriggerEnter(Collider co)
    {
        switch (co.tag)
        {
            case "Base":
                GameController.Instance.OnHit(Info.Boss ? 5 : 1);
                CheckEnd(true);
                break;
            case "Shield":
                FixSpeed((int)(Info.Speed * (1 - GameController.Instance.ShieldReduce)));
                break;
            default:
                break;
        }
    }

    private bool isBombDead = false;//是否被大爆炸致死

    /// <summary>
    /// 点击以消灭此星星
    /// </summary>
    public void OnHit(GameObject go)
    {
        if (GameController.Instance.mStatus != GameStatus.Gaming)
            return;
        isBombDead = false;
        Info.CurHp -= GameController.Instance.Damage;
        if (Info.CurHp <= 0)
            CheckEnd();
        else
        {
            mAudioSource.PlayOneShot(HitClip);
            if (Info.SkillList.Contains(103))
                SkillManager.Instance.DoSkill(103, gameObject);
        }
    }

    /// <summary>
    /// 直接对此星星造成伤害，如果是大爆炸导致死亡，传入true来阻止循环充能
    /// </summary>
    public void OnHit(int damage, bool isBomb = false)
    {
        isBombDead = isBomb;
        Info.CurHp -= damage;
        if (Info.CurHp <= 0)
            CheckEnd();
        else
        {
            mAudioSource.PlayOneShot(HitClip);
            if (Info.SkillList.Contains(103))
                SkillManager.Instance.DoSkill(103, gameObject);
        }
    }

    /// <summary>
    /// 检查销毁自己时是否触发特效
    /// <param name="isBase">是否击中基地</param>
    /// </summary>
    void CheckEnd(bool isBase = false)
    {
        if (!isBombDead)
            GameController.Instance.CheckRecharge(Info.Boss ? 3 : 1);
        if (!isBase)
        {
            mAudioSource.PlayOneShot(EndClip);
            if (Info.Boss) //BOSS技能: 分裂
            {
                GameController.Instance.GenerateExtraEnemy(Random.Range(3, 6), transform.position);
                GameController.Instance.killBossCount++;
            }
            else
                GameController.Instance.killEnemyCount++;
            GameController.Instance.FixMoney(Info.MaxHp + Info.SkillList.Count);
        }
        transform.position = new Vector3(9999, 9999);
        Destroy(gameObject, .5f);
        GameController.Instance.RemoveEnemy(this);
    }

    /// <summary>
    /// 修改移动速度
    /// </summary>
    /// <param name="targetSpeed">目标速度</param>
    /// <param name="multiple">是否成倍</param>
    public void FixSpeed(float targetSpeed, bool multiple = false)
    {
        Info.Speed = multiple ? (int) (Info.Speed*targetSpeed) : (int) targetSpeed;
    }

    /// <summary>
    /// 移动至指定位置
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="duration">时间</param>
    /// <param name="delay">延迟</param>
    public void Move(Vector3 target, float duration, float delay = 0)
    {
        Image img = GetComponent<Image>();
        Tweener tweener = img.rectTransform.DOMove(target, duration);
        tweener.SetEase(duration == 0 ? Ease.Flash : Ease.OutQuad);
        tweener.SetDelay(delay);
    }
}
