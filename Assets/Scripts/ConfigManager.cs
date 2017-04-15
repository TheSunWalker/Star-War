using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Newtonsoft.Json;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    #region CONSTS
    public const int MAX_SOHPITEM_LEVEL = 5;//商品最高等级
    #endregion

    private string configPath = "";

    /// <summary>
    /// 载入商店道具配置表
    /// </summary>
    public Dictionary<int, ShopItem> ShopItems = new Dictionary<int, ShopItem>();

    void Start()
    {
        configPath = Application.streamingAssetsPath;
        LoadShopConfig();
    }

    void LoadShopConfig()
    {
        ShopItems.Clear();
        string filepath = configPath + "/Shop.xml";
        TextAsset asset = Resources.Load<TextAsset>("Shop");
        XmlReader reader = XmlReader.Create(new StringReader(asset.text));
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(reader);
        XmlNodeList nodeList = xmlDoc.SelectSingleNode("root").ChildNodes;
        foreach (XmlElement e in nodeList)
        {
            ShopItem item = new ShopItem();
            item.ID = int.Parse(e.GetAttribute("ID"));
            item.Info = e.GetAttribute("Info");
            item.AttributeName = e.GetAttribute("AttributeName").Split(',');

            string[] prices = e.GetAttribute("Price").Split(',');
            item.Price = new int[prices.Length];
            for (int i = 0; i < prices.Length; ++i)
            {
                item.Price[i] = int.Parse(prices[i]);
            }

            string[] atts = e.GetAttribute("Attributes").Split(';');
            item.Attributes = new float[atts.Length][];
            for (int i = 0; i < atts.Length; ++i)
            {
                string[] atts_1 = atts[i].Split(',');
                float[] f = new float[atts_1.Length];
                for (int j = 0; j < atts_1.Length; ++j)
                    f[j] = float.Parse(atts_1[j]);
                item.Attributes[i] = f;
            }
            ShopItems.Add(item.ID, item);
        }
    }
}
