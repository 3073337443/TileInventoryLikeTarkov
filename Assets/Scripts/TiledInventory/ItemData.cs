using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    public int width = 1;

    public int height = 1;

    [Tooltip("图标")]
    public Sprite itemIcon;

}
