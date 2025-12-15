using UnityEngine;

/// <summary>
/// 容器物品生成配置 - ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ContainerGenerationData", menuName = "Inventory/Container Generation Data")]
public class ContainerGenerationData : ScriptableObject
{
    // 普通容器配置 (Chest)
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
        minTotalValue = 50,
        maxTotalValue = 120,
        maxItemCount = 5,
        maxFillerItems = 2
    };
    
    // 珍贵容器配置 (Precious)
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
        minTotalValue = 80,
        maxTotalValue = 180,
        maxItemCount = 4,
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
}
