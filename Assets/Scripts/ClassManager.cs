using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum GameStatus
{
    //游戏状态：等待，游戏中，暂停，结束
    Idle, Gaming, Pause, End
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

/// <summary>
/// 商店道具
/// </summary>
public class ShopItem
{
    public int ID;//ID
    public string Info;//介绍语句
    public int[] Price;//价格
    public string[] AttributeName;//加成属性名字
    public float[][] Attributes;//加成具体数值
}
