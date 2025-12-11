using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public int id;
    public string name;
    public int value;
    public Quality quality;
    public int width = 1;
    public int height = 1;
    public string spritePath;

    public ItemData(int id, string name, int value, Quality quality, int width, int height, string spritePath)
    {
        this.id = id;
        this.name = name;
        this.value = value;
        this.quality = quality;
        this.width = width;
        this.height = height;
        this.spritePath = spritePath;
    }

}
/// <summary>
/// 物品品质枚举
/// </summary>
public enum Quality
{
    Rare,      // 稀有
    Epic,      // 史诗
    Legendary, // 传说
}