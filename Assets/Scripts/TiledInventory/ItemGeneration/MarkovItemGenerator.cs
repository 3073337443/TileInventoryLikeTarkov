using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    /// 为指定容器生成物品（考虑空间约束）
    /// </summary>
    /// <param name="containerType">容器类型</param>
    /// <param name="gridWidth">容器网格宽度</param>
    /// <param name="gridHeight">容器网格高度</param>
    public List<ItemData> GenerateItems(ContainerType containerType, int gridWidth, int gridHeight)
    {
        GenerationConfig config = generationData.GetConfig(containerType);
        List<ItemData> result = new List<ItemData>();
        VirtualGrid virtualGrid = new VirtualGrid(gridWidth, gridHeight);
        int totalValue = 0;
        
        Quality currentQuality = SampleQuality(config.initialDistribution);
        
        int consecutiveFailures = 0;
        int maxConsecutiveFailures = 8; // 连续失败上限，防止死循环
        
        while (result.Count < config.maxItemCount && consecutiveFailures < maxConsecutiveFailures)
        {
            int remainingValue = config.maxTotalValue - totalValue;
            
            // 获取能放入且不超价值的候选物品
            List<ItemData> candidates = GetPlaceableCandidates(currentQuality, remainingValue, virtualGrid);
            
            if (candidates.Count == 0)
            {
                // 当前品质没有合适物品，尝试从所有品质中找
                candidates = GetPlaceableCandidatesFromAllQualities(remainingValue, virtualGrid);
                
                if (candidates.Count == 0)
                {
                    // 空间不足，无法放置任何物品，停止生成
                    Debug.Log("[MarkovGenerator] 空间不足，停止生成");
                    break;
                }
                
                consecutiveFailures++;
            }
            else
            {
                consecutiveFailures = 0;
            }
            
            // 加权选择物品
            ItemData selected = WeightedSelectByRarity(candidates);
            
            // 放置到虚拟网格
            virtualGrid.TryPlace(selected.width, selected.height);
            
            result.Add(selected);
            totalValue += selected.value;
            
            // 马尔科夫转移
            currentQuality = TransitionToNextQuality(currentQuality, config);
        }
        
        // 补偿机制：价值不足时补充小物品
        FillToMinValue(result, ref totalValue, config, virtualGrid);
        
        Debug.Log($"[MarkovGenerator] 容器:{containerType}, 物品数:{result.Count}, 总价值:{totalValue}");
        return result;
    }
    /// <summary>
    /// 获取指定品质中能放入的候选物品
    /// </summary>
    private List<ItemData> GetPlaceableCandidates(Quality quality, int maxValue, VirtualGrid grid)
    {
        return itemsByQuality[quality]
            .Where(item => 
                item.value <= maxValue && 
                grid.CanPlace(item.width, item.height))
            .ToList();
    }

    /// <summary>
    /// 从所有品质中获取能放入的候选物品
    /// </summary>
    private List<ItemData> GetPlaceableCandidatesFromAllQualities(int maxValue, VirtualGrid grid)
    {
        List<ItemData> result = new List<ItemData>();
        
        foreach (var kvp in itemsByQuality)
        {
            var fitting = kvp.Value.Where(item =>
                item.value <= maxValue &&
                grid.CanPlace(item.width, item.height));
            result.AddRange(fitting);
        }
        
        return result;
    }

    /// <summary>
    /// 补偿机制：填充到最小价值
    /// </summary>
    private void FillToMinValue(List<ItemData> result, ref int totalValue, GenerationConfig config, VirtualGrid grid)
    {
        int fillerCount = 0;
        
        while (totalValue < config.minTotalValue && fillerCount < config.maxFillerItems)
        {
            int neededValue = config.minTotalValue - totalValue;
            
            // 只选择小物品作为补偿（面积 <= 4）
            var smallItems = itemsByQuality[Quality.Rare]
                .Concat(itemsByQuality[Quality.Epic])
                .Where(item => 
                    item.value <= neededValue &&
                    item.width * item.height <= 4 &&
                    grid.CanPlace(item.width, item.height))
                .OrderByDescending(item => item.value)
                .ToList();
            
            if (smallItems.Count == 0)
            {
                break; // 没有能放入的小物品
            }
            
            ItemData filler = smallItems[Random.Range(0, Mathf.Min(3, smallItems.Count))];
            grid.TryPlace(filler.width, filler.height);
            result.Add(filler);
            totalValue += filler.value;
            fillerCount++;
        }
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
}