using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDataManager : Singleton<ItemDataManager>
{
    private List<ItemData> itemDataList = new List<ItemData>();
    private Dictionary<int, ItemData> itemDataDictionary = new Dictionary<int, ItemData>();

    void Awake()
    {
        LoadItemsFromJson();
    }

    /// <summary>
    /// 从json文件中加载物品数据
    /// </summary>
    void LoadItemsFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Items/Items");
        ItemsData itemsData = JsonUtility.FromJson<ItemsData>(jsonFile.text);
        foreach (ItemJsonData itemJsonData in itemsData.items)
        {
            CreateItemFromJson(itemJsonData);
        }
        foreach(var itemData in itemDataList)
        {
            if(!itemDataDictionary.ContainsKey(itemData.id))
            {
                itemDataDictionary.Add(itemData.id, itemData);
            }
        }
    }
    /// <summary>
    /// 从json数据中创建物品数据
    /// </summary>
    /// <param name="itemJsonData"></param>
    private void CreateItemFromJson(ItemJsonData itemJsonData)
    {
        int id = itemJsonData.id;
        string name = itemJsonData.name;
        int value = itemJsonData.value;
        string qualityStr = itemJsonData.qualityStr;
        int width = itemJsonData.width;
        int height = itemJsonData.height;
        string spritePath = itemJsonData.spritePath;

        Quality itemQuality = ParseQuality(qualityStr);

        ItemData itemData = new ItemData(id, name, value, itemQuality, width, height, spritePath);
        itemDataList.Add(itemData);
    }

    /// <summary>
    /// 解析物品品质
    /// </summary>
    /// <param name="qualityStr"></param>
    /// <returns></returns>
    private Quality ParseQuality(string qualityStr)
    {
        switch(qualityStr.ToLower())
        {
            case "rare": return Quality.Rare;
            case "epic": return Quality.Epic;
            case "legendary": return Quality.Legendary;
            case "treasure": return Quality.Treasure;
            default: return Quality.Rare;
        }
    }
    /// <summary>
    /// 根据id返回物品数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemData GetItemDataByID(int id)
    {
        if(itemDataDictionary.ContainsKey(id))
        {
            return itemDataDictionary[id];
        }
        return null;
    }
    public int GetItemCount()
    {
        return itemDataList.Count;
    }
}
[Serializable]
public class ItemsData
{
    public ItemJsonData[] items;
}

[Serializable]
public class ItemJsonData
{
    public int id;
    public string name;
    public int value;
    public string qualityStr;
    public int width = 1;
    public int height = 1;
    public string spritePath;
}