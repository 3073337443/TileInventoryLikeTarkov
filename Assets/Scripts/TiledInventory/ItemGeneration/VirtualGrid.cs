using UnityEngine;

/// <summary>
/// 虚拟网格 - 用于模拟物品放置，检查空间是否足够
/// </summary>
public class VirtualGrid
{
    private bool[,] occupied;
    private int width;
    private int height;
    
    public int Width => width;
    public int Height => height;
    
    public VirtualGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        occupied = new bool[width, height];
    }
    
    /// <summary>
    /// 检查物品是否能放入网格（不实际放置）
    /// </summary>
    public bool CanPlace(int itemWidth, int itemHeight)
    {
        for (int y = 0; y <= height - itemHeight; y++)
        {
            for (int x = 0; x <= width - itemWidth; x++)
            {
                if (CanPlaceAt(x, y, itemWidth, itemHeight))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// 尝试放置物品，成功返回true并标记占用
    /// </summary>
    public bool TryPlace(int itemWidth, int itemHeight)
    {
        for (int y = 0; y <= height - itemHeight; y++)
        {
            for (int x = 0; x <= width - itemWidth; x++)
            {
                if (CanPlaceAt(x, y, itemWidth, itemHeight))
                {
                    PlaceAt(x, y, itemWidth, itemHeight);
                    return true;
                }
            }
        }
        return false;
    }
    
    private bool CanPlaceAt(int posX, int posY, int itemWidth, int itemHeight)
    {
        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                if (occupied[posX + x, posY + y])
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    private void PlaceAt(int posX, int posY, int itemWidth, int itemHeight)
    {
        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                occupied[posX + x, posY + y] = true;
            }
        }
    }
}