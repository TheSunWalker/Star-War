using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyInfo Info;//各项数据
    public float ForceDestroyTime = 10;//强制销毁时间
    private float generateTime = 0;//生成时间

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="boss">是否是Boss</param>
    /// <param name="diff">当前难度等级</param>
    public void Init(bool boss, int diff)
    {
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
            speed = 100 + (int)(diff*2.5f);
        }
        Info = new EnemyInfo(boss, maxHp, speed);
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
    }

    /// <summary>
    /// 如果存活的时间过长，强制销毁
    /// </summary>
    void CheckDestroy()
    {
        if (Time.time - generateTime >= ForceDestroyTime)
            Destroy(gameObject);
    }
}
