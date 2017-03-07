using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BulletAI : MonoBehaviour
{
    private int dmg = 0;
    private EnemyAI target;

    /// <summary>
    /// 设定攻击对象和伤害值
    /// </summary>
    public void SetTarget(EnemyAI target, int damage)
    {
        dmg = damage;
        this.target = target;
        //transform.LookAt(target.transform);
        Tweener tweener = transform.DOMove(target.transform.position, .2f);
        tweener.OnComplete(DoDamage);
    }

    void DoDamage()
    {
        Destroy(gameObject);
        if (target == null)
            return;
        target.OnHit(dmg);
    }
}
