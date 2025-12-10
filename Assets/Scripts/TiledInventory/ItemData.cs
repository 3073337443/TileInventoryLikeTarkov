using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Tooltip("宽")]
    public int width = 1;
    [Tooltip("高")]
    public int height = 1;

    [Tooltip("图标")]
    public Sprite itemIcon;

}
