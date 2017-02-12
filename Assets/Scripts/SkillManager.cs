using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    /// <summary>
    /// 临时用于技能效果，日后更新为xlua+配置表
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="go">起效的物体</param>
    public void DoSkill(int skillId, GameObject go)
    {
        EnemyAI ai = go.GetComponent<EnemyAI>();
        if (ai == null)
            return;
        switch (skillId)
        {
            case 101:
                //加速
                ai.FixSpeed(1.5f, true);
                break;
            case 102:
                //隐身
                StartCoroutine("DoInvisible", ai);
                break;
            case 103:
                //闪烁
                ai.Move(new Vector3(Random.Range(80, 851), ai.transform.position.y), 0);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 隐身技能效果
    /// </summary>
    /// <param name="ai">要隐身的单位</param>
    /// <returns></returns>
    IEnumerator DoInvisible(EnemyAI ai)
    {
        if (ai == null)
            yield break;

        Image star = ai.GetComponent<Image>();
        Text hp = ai.GetComponentInChildren<Text>();
        while (ai.Info.CurHp > 0)
        {
            yield return new WaitForSeconds(1);
            if (star == null)
                yield break;
            star.CrossFadeAlpha(0, .2f, false);
            hp.CrossFadeAlpha(0, .2f, false);
            yield return new WaitForSeconds(2);
            if (star == null)
                yield break;
            star.CrossFadeAlpha(1, .2f, false);
            hp.CrossFadeAlpha(1, .2f, false);
        }
    }
}
