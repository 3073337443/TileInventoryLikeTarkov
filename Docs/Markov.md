
```csharp 纯数据类，用于存储马尔科夫链的配置参数
using System;
using UnityEngine;

/// <summary>
/// 物品生成配置数据
/// </summary>
[Serializable]
public class GenerationConfig
{
    [Header("转移矩阵 (4x4: Rare, Epic, Legendary, Treasure)")]
    [Tooltip("行=当前状态，列=下一状态的概率")]
    public float[] transitionMatrixFlat = new float[16]; // Unity无法序列化二维数组，使用一维展开
    
    [Header("初始品质分布")]
    [Tooltip("依次为: Rare, Epic, Legendary, Treasure 的初始概率")]
    public float[] initialDistribution = new float[4];
    
    [Header("价值约束")]
    public int minTotalValue = 100;
    public int maxTotalValue = 270;
    
    [Header("物品数量约束")]
    public int maxItemCount = 5;
    public int maxFillerItems = 2; // 补偿物品最大数量
    
    /// <summary>
    /// 获取转移概率 P(nextQuality | currentQuality)
    /// </summary>
    public float GetTransitionProbability(Quality current, Quality next)
    {
        int row = (int)current;
        int col = (int)next;
        return transitionMatrixFlat[row * 4 + col];
    }
    
    /// <summary>
    /// 获取从当前品质出发的所有转移概率
    /// </summary>
    public float[] GetTransitionRow(Quality current)
    {
        float[] row = new float[4];
        int startIndex = (int)current * 4;
        for (int i = 0; i < 4; i++)
        {
            row[i] = transitionMatrixFlat[startIndex + i];
        }
        return row;
    }
    
    /// <summary>
    /// 验证配置有效性
    /// </summary>
    public bool Validate()
    {
        // 检查初始分布和为1
        float sum = 0;
        foreach (var p in initialDistribution) sum += p;
        if (Mathf.Abs(sum - 1f) > 0.01f)
        {
            Debug.LogError("初始分布概率和不为1");
            return false;
        }
        
        // 检查每行转移概率和为1
        for (int row = 0; row < 4; row++)
        {
            float rowSum = 0;
            for (int col = 0; col < 4; col++)
            {
                rowSum += transitionMatrixFlat[row * 4 + col];
            }
            if (Mathf.Abs(rowSum - 1f) > 0.01f)
            {
                Debug.LogError($"转移矩阵第{row}行概率和不为1");
                return false;
            }
        }
        return true;
    }
}
```

```csharp ScriptableObject让配置可在编辑器中调整，支持不同容器类型的独立配置。
using UnityEngine;

/// <summary>
/// 容器物品生成配置 - ScriptableObject
/// 创建方式: Project窗口右键 -> Create -> Inventory -> Container Generation Data
/// </summary>
[CreateAssetMenu(fileName = "ContainerGenerationData", menuName = "Inventory/Container Generation Data")]
public class ContainerGenerationData : ScriptableObject
{
    [Header("普通容器配置 (Chest)")]
    public GenerationConfig chestConfig = new GenerationConfig
    {
        // 转移矩阵展开 (行优先): Rare→[R,E,L,T], Epic→[R,E,L,T], Legendary→[R,E,L,T], Treasure→[R,E,L,T]
        transitionMatrixFlat = new float[]
        {
            0.35f, 0.30f, 0.25f, 0.10f,  // Rare →
            0.30f, 0.30f, 0.30f, 0.10f,  // Epic →
            0.25f, 0.25f, 0.35f, 0.15f,  // Legendary →
            0.40f, 0.25f, 0.25f, 0.10f   // Treasure →
        },
        initialDistribution = new float[] { 0.40f, 0.25f, 0.25f, 0.10f },
        minTotalValue = 100,
        maxTotalValue = 270,
        maxItemCount = 5,
        maxFillerItems = 2
    };
    
    [Header("珍贵容器配置 (Precious)")]
    public GenerationConfig preciousConfig = new GenerationConfig
    {
        transitionMatrixFlat = new float[]
        {
            0.20f, 0.25f, 0.30f, 0.25f,  // Rare →
            0.15f, 0.25f, 0.30f, 0.30f,  // Epic →
            0.10f, 0.20f, 0.35f, 0.35f,  // Legendary →
            0.15f, 0.20f, 0.25f, 0.40f   // Treasure →
        },
        initialDistribution = new float[] { 0.15f, 0.20f, 0.30f, 0.35f },
        minTotalValue = 120,
        maxTotalValue = 380,
        maxItemCount = 5,
        maxFillerItems = 2
    };
    
    /// <summary>
    /// 根据容器类型获取对应配置
    /// </summary>
    public GenerationConfig GetConfig(ContainerType type)
    {
        return type switch
        {
            ContainerType.chest => chestConfig,
            ContainerType.precious => preciousConfig,
            _ => chestConfig
        };
    }
    
    /// <summary>
    /// 验证所有配置
    /// </summary>
    [ContextMenu("验证配置")]
    public void ValidateAll()
    {
        Debug.Log($"Chest配置验证: {chestConfig.Validate()}");
        Debug.Log($"Precious配置验证: {preciousConfig.Validate()}");
    }
}
```

```csharp 这是核心生成器，负责根据马尔科夫链生成物品列表。
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 基于马尔科夫链的物品生成器
/// </summary>
public class MarkovItemGenerator
{
    private ContainerGenerationData generationData;
    private ItemDataManager itemDataManager;
    
    // 按品质分类的物品缓存
    private Dictionary<Quality, List<ItemData>> itemsByQuality;
    
    public MarkovItemGenerator(ContainerGenerationData data, ItemDataManager itemManager)
    {
        generationData = data;
        itemDataManager = itemManager;
        CacheItemsByQuality();
    }
    
    /// <summary>
    /// 缓存按品质分类的物品
    /// </summary>
    private void CacheItemsByQuality()
    {
        itemsByQuality = new Dictionary<Quality, List<ItemData>>
        {
            { Quality.Rare, new List<ItemData>() },
            { Quality.Epic, new List<ItemData>() },
            { Quality.Legendary, new List<ItemData>() },
            { Quality.Treasure, new List<ItemData>() }
        };
        
        int itemCount = itemDataManager.GetItemCount();
        for (int i = 0; i < itemCount; i++)
        {
            ItemData item = itemDataManager.GetItemDataByID(i);
            if (item != null && itemsByQuality.ContainsKey(item.quality))
            {
                itemsByQuality[item.quality].Add(item);
            }
        }
    }
    
    /// <summary>
    /// 为指定容器类型生成物品列表
    /// </summary>
    public List<ItemData> GenerateItems(ContainerType containerType)
    {
        GenerationConfig config = generationData.GetConfig(containerType);
        List<ItemData> result = new List<ItemData>();
        int totalValue = 0;
        
        // 1. 根据初始分布选择第一个品质
        Quality currentQuality = SampleQuality(config.initialDistribution);
        
        // 2. 主生成循环
        int attempts = 0;
        int maxAttempts = config.maxItemCount * 3; // 防止死循环
        
        while (result.Count < config.maxItemCount && attempts < maxAttempts)
        {
            attempts++;
            
            // 获取当前品质的候选物品
            List<ItemData> candidates = itemsByQuality[currentQuality];
            if (candidates.Count == 0)
            {
                currentQuality = TransitionToNextQuality(currentQuality, config);
                continue;
            }
            
            // 计算剩余可用价值
            int remainingValue = config.maxTotalValue - totalValue;
            
            // 筛选不超过剩余价值的物品
            List<ItemData> fittingItems = candidates.Where(item => item.value <= remainingValue).ToList();
            
            if (fittingItems.Count == 0)
            {
                // 当前品质没有合适物品，尝试降级到更低价值品质
                Quality lowerQuality = TryGetLowerQuality(currentQuality);
                if (lowerQuality != currentQuality)
                {
                    currentQuality = lowerQuality;
                    continue;
                }
                else
                {
                    break; // 无法再添加任何物品
                }
            }
            
            // 根据rarity加权选择物品
            ItemData selectedItem = WeightedSelectByRarity(fittingItems);
            
            // 添加物品
            result.Add(selectedItem);
            totalValue += selectedItem.value;
            
            // 马尔科夫转移：决定下一个品质
            currentQuality = TransitionToNextQuality(currentQuality, config);
            
            // 检查是否已达到最大价值附近
            if (totalValue >= config.maxTotalValue - 5)
            {
                break;
            }
        }
        
        // 3. 补偿机制：价值不足时补充低价值物品
        int fillerCount = 0;
        while (totalValue < config.minTotalValue && fillerCount < config.maxFillerItems)
        {
            int needed = config.minTotalValue - totalValue;
            ItemData filler = FindFillerItem(needed);
            
            if (filler != null)
            {
                result.Add(filler);
                totalValue += filler.value;
                fillerCount++;
            }
            else
            {
                break;
            }
        }
        
        Debug.Log($"[MarkovGenerator] 容器类型:{containerType}, 生成物品数:{result.Count}, 总价值:{totalValue}");
        return result;
    }
    
    /// <summary>
    /// 根据概率分布采样品质
    /// </summary>
    private Quality SampleQuality(float[] distribution)
    {
        float random = Random.value;
        float cumulative = 0;
        
        for (int i = 0; i < distribution.Length; i++)
        {
            cumulative += distribution[i];
            if (random <= cumulative)
            {
                return (Quality)i;
            }
        }
        return Quality.Rare;
    }
    
    /// <summary>
    /// 马尔科夫转移：根据当前品质和转移矩阵确定下一个品质
    /// </summary>
    private Quality TransitionToNextQuality(Quality current, GenerationConfig config)
    {
        float[] transitionRow = config.GetTransitionRow(current);
        return SampleQuality(transitionRow);
    }
    
    /// <summary>
    /// 根据rarity加权随机选择物品
    /// rarity越高，被选中概率越高
    /// </summary>
    private ItemData WeightedSelectByRarity(List<ItemData> items)
    {
        if (items.Count == 0) return null;
        if (items.Count == 1) return items[0];
        
        // 使用 (1 - rarity) 作为权重，使稀有物品更难获得
        // 或者直接使用 rarity 作为权重，使高rarity物品更容易获得
        // 这里使用 rarity 作为权重
        float totalWeight = items.Sum(item => item.rarity);
        float random = Random.value * totalWeight;
        float cumulative = 0;
        
        foreach (var item in items)
        {
            cumulative += item.rarity;
            if (random <= cumulative)
            {
                return item;
            }
        }
        return items[items.Count - 1];
    }
    
    /// <summary>
    /// 尝试获取更低价值的品质
    /// </summary>
    private Quality TryGetLowerQuality(Quality current)
    {
        return current switch
        {
            Quality.Treasure => Quality.Legendary,
            Quality.Legendary => Quality.Epic,
            Quality.Epic => Quality.Rare,
            _ => Quality.Rare
        };
    }
    
    /// <summary>
    /// 查找用于补偿的低价值物品
    /// </summary>
    private ItemData FindFillerItem(int maxValue)
    {
        // 优先从Rare中找
        var rareItems = itemsByQuality[Quality.Rare]
            .Where(item => item.value <= maxValue)
            .OrderByDescending(item => item.value)
            .ToList();
        
        if (rareItems.Count > 0)
        {
            return rareItems[Random.Range(0, rareItems.Count)];
        }
        
        // 再从Epic中找
        var epicItems = itemsByQuality[Quality.Epic]
            .Where(item => item.value <= maxValue)
            .ToList();
        
        if (epicItems.Count > 0)
        {
            return epicItems[Random.Range(0, epicItems.Count)];
        }
        
        return null;
    }
}
```
融入项目的修改方案
ItemDataManager.cs
```csharp
// 在 ItemDataManager 类中添加以下方法

/// <summary>
/// 根据品质获取物品列表
/// </summary>
public List<ItemData> GetItemsByQuality(Quality quality)
{
    return itemDataList.Where(item => item.quality == quality).ToList();
}

/// <summary>
/// 获取所有物品数据列表
/// </summary>
public List<ItemData> GetAllItems()
{
    return new List<ItemData>(itemDataList);
}
```
InventoryController.cs
```csharp
// 在类顶部添加字段
[Header("物品生成配置")]
[SerializeField] private ContainerGenerationData generationData;
private MarkovItemGenerator itemGenerator;

// 在 Awake 或 Start 中初始化（确保在 ItemDataManager 初始化之后）
void Start()
{
    // ... 现有代码 ...
    
    // 初始化马尔科夫生成器
    if (generationData != null)
    {
        itemGenerator = new MarkovItemGenerator(generationData, ItemDataManager.Instance);
    }
}

/// <summary>
/// 为容器生成物品（使用马尔科夫链）
/// </summary>
public void GenerateItemsForContainer(ItemGrid targetGrid, ContainerType containerType)
{
    if (targetGrid == null || itemGenerator == null) 
    {
        Debug.LogWarning("无法生成物品：targetGrid或itemGenerator为空");
        return;
    }
    
    List<ItemData> itemsToGenerate = itemGenerator.GenerateItems(containerType);
    
    foreach (ItemData itemData in itemsToGenerate)
    {
        InventoryItem inventoryItem = uiPool.GetItemObject();
        if (inventoryItem == null) continue;
        
        inventoryItem.Set(itemData);
        uiPool.MoveItemReturnToGridLayer(inventoryItem, targetGrid);
        InsertItem(inventoryItem, targetGrid);
    }
    
    uiPool.SetGridItemContainerActive(targetGrid, false);
}
```
ContainerInteractable.cs
```csharp
// 修改前的 Start 方法
void Start()
{
    uiPool.CreateContainerInstance(containerID, containerUIPrefab);
    ItemGrid grid = uiPool.GetContainerGrid(containerID);
    InventoryController.Instance.InsertRandomItem(grid);
    InventoryController.Instance.InsertRandomItem(grid);
    InventoryController.Instance.InsertRandomItem(grid);
    InventoryController.Instance.InsertRandomItem(grid);
}

// 修改后的 Start 方法
void Start()
{
    uiPool.CreateContainerInstance(containerID, containerUIPrefab);
    ItemGrid grid = uiPool.GetContainerGrid(containerID);
    
    // 使用马尔科夫链生成物品
    InventoryController.Instance.GenerateItemsForContainer(grid, containerType);
}

// 添加获取容器类型的方法（可选，用于外部访问）
public ContainerType GetContainerType() => containerType;
```


五、使用步骤总结
步骤1：创建脚本文件
步骤2：创建配置资源
Project窗口右键 → Create → Inventory → Container Generation Data
命名为 DefaultGenerationData
放置于 Assets/Resources/GenerationData/ 目录
在Inspector中调整转移矩阵参数（可选）
步骤3：配置 InventoryController
选中场景中的 InventoryController 对象
将 DefaultGenerationData 拖拽到 Generation Data 字段
步骤4：配置容器预制体
打开容器预制体
确保 ContainerInteractable 组件的 Container Type 字段正确设置为 chest 或 precious
步骤5：测试验证
运行游戏，打开不同类型的容器
观察控制台输出的生成日志
验证 chest 容器价值在 (100, 270) 范围
验证 precious 容器价值在 (120, 380) 范围且 Treasure 物品更多

六、调参建议
如果实际效果不满意，可以调整以下参数：

调整目标	修改位置
提高Treasure出现率	增大转移矩阵第4列的值
降低价值波动	减小maxTotalValue - minTotalValue差值
增加物品数量	增大maxItemCount
让品质更连贯	增大转移矩阵对角线的值
让品质更跳跃	减小转移矩阵对角线的值