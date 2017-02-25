using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public List<ShopItemUI> ShopItemUis = new List<ShopItemUI>();
    public AudioSource mAudio;

    void Start()
    {
        StartCoroutine(Init());
    }

    /// <summary>
    /// 初始化，使用协程来先让ConfigManager完成载入，才能进行这部分逻辑
    /// </summary>
    IEnumerator Init()
    {
        while (ConfigManager.Instance.ShopItems.Count == 0)
            yield return null;
        yield return null;
        int id = 10001;
        for (int i = 0; i < ShopItemUis.Count; ++i)
        {
            ShopItemUis[i].Init(id);
            id++;
            EventTriggerListener.Get(ShopItemUis[i].PurchaseButton).onClick = PurchaseShopItem;
        }
    }

    /// <summary>
    /// 点击购买按钮
    /// </summary>
    void PurchaseShopItem(GameObject go)
    {
        ShopItemUI ui = go.GetComponentInParent<ShopItemUI>();
        if (ui == null)
            return;
        int lv = ui.GetLevel();
        if (lv >= 5)
        {
            GameController.Instance.ShowTips("This item is already level max.");
            return;
        }
        int price = ui.GetPrice();
        if (GameController.Instance.Money < ui.GetPrice())
        {
            GameController.Instance.ShowTips("You cannot afford this item. (not enough money)");
            return;
        }
        mAudio.Play();
        GameController.Instance.FixMoney(-price);
        ui.UpdateInfo(lv + 1);
        GameController.Instance.UpdateItem(ui.mItem);
    }

    /// <summary>
    /// 刷新金额颜色
    /// </summary>
    public void RefreshPriceColor()
    {
        for (int i = 0; i < ShopItemUis.Count; ++i)
        {
            ShopItemUis[i].FixPriceColor();
        }
    }
}
