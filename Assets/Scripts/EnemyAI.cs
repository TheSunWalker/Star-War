using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public EnemyInfo Info;//各项数据
    public float ForceDestroyTime = 10;//强制销毁时间
    private float generateTime = 0;//生成时间
    public Text HpText;//显示血量

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="boss">是否是Boss</param>
    public void Init(bool boss)
    {
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
        if (Time.time - generateTime >= ForceDestroyTime)
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
                CheckEnd();
                break;
            case "Shield":
                Info.Speed = (int)(Info.Speed*(1 - GameController.Instance.ShieldReduce));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 点击以消灭此星星
    /// </summary>
    public void OnHit(GameObject go)
    {
        Info.CurHp -= GameController.Instance.Damage;
        if (Info.CurHp <= 0)
            CheckEnd();
    }

    /// <summary>
    /// 检查销毁自己时是否触发特效
    /// </summary>
    void CheckEnd()
    {
        Destroy(gameObject);
    }
}
