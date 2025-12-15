using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationConfig
{
    [Header("转移矩阵 (4x4: Rare, Epic, Legendary, Treasure)")]
    [Tooltip("行=当前状态，列=下一状态的概率")]
    public float[] transitionMatrixFlat = new float[16]; 
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
    /// 默认构造函数 - 初始化默认值
    /// </summary>
    public GenerationConfig()
    {
        // 初始化数组长度
        transitionMatrixFlat = new float[16];
        initialDistribution = new float[4];
        
        // 默认值
        minTotalValue = 100;
        maxTotalValue = 270;
        maxItemCount = 5;
        maxFillerItems = 2;
        
        // 默认转移矩阵（均匀分布）
        for (int i = 0; i < 16; i++)
        {
            transitionMatrixFlat[i] = 0.25f;
        }
        
        // 默认初始分布（均匀）
        for (int i = 0; i < 4; i++)
        {
            initialDistribution[i] = 0.25f;
        }
    }
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

}
