using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GuardAI : MonoBehaviour
{
    public Text LevelText;
    public int ItemID;
    public int Lv;
    public ShopItem mItem;
    public GameObject Effect;
    public GameObject EffectParent;
    public AudioSource BombAudio;

    /// <summary>
    /// 更新信息
    /// </summary>
    public void UpdateInfo(ShopItem item, int lv)
    {
        Lv = lv;
        LevelText.text = "Lv." + lv;
        ItemID = item.ID;
        mItem = item;
    }

    void Update()
    {
        CheckSkill();
    }

    /// <summary>
    /// 实现道具逻辑
    /// </summary>
    void CheckSkill()
    {
        if (Lv == 0)
            return;
        switch (ItemID)
        {
            case 10001:
                DoAttack();
                break;
            case 10002:
                //CheckRecharge();
                break;
            case 10003:
                DoHeal();
                break;
        }
    }

    private float lastAttackTime = 0;
    /// <summary>
    /// 对随机敌人进行打击
    /// </summary>
    void DoAttack()
    {
        float interval = mItem.Attributes[Lv - 1][1];
        int damage = (int)mItem.Attributes[Lv - 1][0];
        if (Time.time - lastAttackTime < interval || GameController.Instance.mStatus != GameStatus.Gaming)
            return;
        EnemyAI enemy = GameController.Instance.GetRandomEnemy();
        if (enemy == null)
            return;

        lastAttackTime = Time.time;
        BulletAI ai = Instantiate(Effect).GetComponent<BulletAI>();
        ai.transform.position = transform.position;
        ai.transform.parent = EffectParent.transform;
        ai.SetTarget(enemy, damage);
    }

    public Image MaskImage;
    public Image BombImage;
    private int currentCharge = 0;
    /// <summary>
    /// 杀死敌人后进行充能
    /// </summary>
    public void DoRecharge(int charge)
    {
        if (Lv == 0 || GameController.Instance.mStatus != GameStatus.Gaming)
            return;
        float needCharge = mItem.Attributes[Lv - 1][1];
        if (currentCharge >= needCharge)
            return;
        currentCharge += charge;
        MaskImage.fillAmount = currentCharge/needCharge;
    }

    /// <summary>
    /// 执行大爆炸
    /// </summary>
    public void DoBomb()
    {
        float needCharge = mItem.Attributes[Lv - 1][1];
        if (Lv == 0 || GameController.Instance.mStatus != GameStatus.Gaming || currentCharge < needCharge)
            return;
        currentCharge = 0;
        MaskImage.fillAmount = 0;
        BombAudio.Play();
        BombImage.raycastTarget = true;
        BombImage.DOColor(new Color(1, 56/255f, 58/255f, 1), 0.2f);
        Tweener tweener = BombImage.DOColor(new Color(1, 56 / 255f, 58 / 255f, 0), .5f);
        tweener.SetDelay(0.3f);
        tweener.OnComplete(CloseBombEffect);
        EnemyAI[] enemyList = GameController.Instance.EnemyList.ToArray();
        int damage = (int)mItem.Attributes[Lv - 1][0];
        for (int i = 0; i < enemyList.Length; i++)
        {
            EnemyAI enemy = enemyList[i];
            if (enemy != null)
                enemy.OnHit(damage, true);
        }
    }

    void CloseBombEffect()
    {
        BombImage.raycastTarget = false;
    }

    private float lastHealTime = 0;
    /// <summary>
    /// 对基地进行治疗
    /// </summary>
    void DoHeal()
    {
        if (Time.time - lastHealTime < 1 || GameController.Instance.mStatus != GameStatus.Gaming)
            return;
        lastHealTime = Time.time;
        float heal = mItem.Attributes[Lv - 1][0];
        GameController.Instance.OnHit(-heal);
        Effect.SetActive(true);
        Invoke("CloseEffect",.3f);
    }

    void CloseEffect()
    {
        Effect.SetActive(false);
    }

}
