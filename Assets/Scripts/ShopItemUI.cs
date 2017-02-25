using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Text CurrentLevelText;
    public Text IntroText;
    public GameObject PurchaseButton;
    public Text PriceText;
    private int currentLevel = 0;
    public ShopItem mItem;

    /// <summary>
    /// 初始化道具信息
    /// </summary>
    /// <param name="itemId">此道具的ID</param>
    public void Init(int itemId)
    {
        if (!ConfigManager.Instance.ShopItems.ContainsKey(itemId))
            return;
        mItem = ConfigManager.Instance.ShopItems[itemId];
        UpdateInfo(0);
    }

    /// <summary>
    /// 更新并显示道具信息
    /// </summary>
    /// <param name="lv">当前等级</param>
    public void UpdateInfo(int lv)
    {
        currentLevel = lv;
        CurrentLevelText.text = "Lv." + lv;
        PriceText.text = lv >= ConfigManager.MAX_SOHPITEM_LEVEL ? "Max level" : "￥" + mItem.Price[lv].ToString("N0");
        FixPriceColor();
        IntroText.text = mItem.Info + "\n";
        for (int i = 0; i < mItem.AttributeName.Length; ++i)
        {
            for (int j = 0; j < mItem.Attributes[i].Length; ++j)
            {
                IntroText.text += mItem.AttributeName[j] + ": ";
                IntroText.text += (lv == 0 ? "0" : mItem.Attributes[lv - 1][j].ToString()) + "(" +
                                  (lv >= ConfigManager.MAX_SOHPITEM_LEVEL ? "MAX" : mItem.Attributes[lv][j].ToString()) + ")\n";
            }
        }
    }

    /// <summary>
    /// 修正购买金额的颜色
    /// </summary>
    public void FixPriceColor()
    {
        int lv = Mathf.Min(currentLevel, ConfigManager.MAX_SOHPITEM_LEVEL - 1);
        PriceText.color = GameController.Instance.Money >= mItem.Price[lv]
            ? (lv >= ConfigManager.MAX_SOHPITEM_LEVEL ? Color.black : Color.white)
            : Color.red;
    }

    /// <summary>
    /// 返回道具当前等级
    /// </summary>
    public int GetLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// 返回当前价格
    /// </summary>
    public int GetPrice()
    {
        return mItem.Price[currentLevel];
    }
}
