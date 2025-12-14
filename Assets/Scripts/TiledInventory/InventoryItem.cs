using System;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物品
/// </summary>
public class InventoryItem : MonoBehaviour
{
    private Action onSearchComplete; // 搜索完成回调
    [SerializeField] private UnityEngine.UI.Image itemSprite;
    [SerializeField] private UnityEngine.UI.Image backgroundSpirite;
    [SerializeField] public bool isSearched = false;

    [SerializeField] private GameObject itemUnSearched;
    [SerializeField] private UnityEngine.UI.Image searchImage;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private float dragAlpha = 0.5f;
    private CanvasGroup canvasGroup;

    public ItemData itemData;
    private Tween searchTween;
    
    public int Width
    {
        get
        {
            return rotated ? itemData.height : itemData.width;
        }
    }
    public int Height
    {
        get
        {
            return rotated ? itemData.width : itemData.height;
        }
    }

    public int onGridPosX;
    public int onGridPosY;
    public bool rotated = false;
    private float curAngle = 0f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    internal void Rotated()
    {
        rotated = !rotated;
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.rotation = Quaternion.Euler(0, 0, rotated ? 90 : 0);
    }
    /// <summary>
    /// 设置物品数据
    /// </summary>
    /// <param name="itemData"></param>
    internal void Set(ItemData itemData)
    {
        this.itemData = itemData;
        itemSprite.sprite = Resources.Load<Sprite>(itemData.spritePath); // 加载物品图片，路径为itemData中的spritePath。
        backgroundSpirite.color = GetQualityColor(itemData.quality);

        Vector2 size = new Vector2();
        size.x = itemData.width * ItemGrid.GetSimpleTileWidth(); // 使物品在tile的正中央显示
        size.y = itemData.height * ItemGrid.GetSimpleTileHeight(); // 使物品在tile的正中央显示
        GetComponent<RectTransform>().sizeDelta = size;
    }
    /// <summary>
    /// 根据物品品质返回对应颜色作为背景色
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    private Color GetQualityColor(Quality quality)
    {
        switch(quality)
        {
            case Quality.Rare: return new Color(0f/255f, 30f/255f, 100f/255f);
            case Quality.Epic: return new Color(55f/255f, 30f/255f, 100f/255f);
            case Quality.Legendary: return new Color(150f/255f, 95f/255f, 0f/255f);
            case Quality.Treasure: return new Color(80f/255f, 10f/255f, 0f/255f);
            default: return Color.white;
        }
    }
    /// <summary>
    /// 设置拖拽透明度
    /// </summary>
    /// <param name="isDragging"></param>
    public void SetDragTransparency(bool isDragging)
    {
        if(canvasGroup == null) return;
        canvasGroup.alpha = isDragging ? dragAlpha : 1f;
    }
    #region 搜索
    public void StartSearch(Action onSearchComplete = null)
    {
        if(isSearched) 
        {
            HideSearchUI();
            if(onSearchComplete != null)
            {
                onSearchComplete?.Invoke();
            }
            return;
        }

        this.onSearchComplete = onSearchComplete;
        ShowSearchUI();
        RotateImage();
    }
    /// <summary>
    /// 图片绕中心旋转
    /// </summary>
    private void RotateImage()
    {

        int loops;
        switch(itemData.quality)
        {
            case Quality.Rare: loops = 1; break;
            case Quality.Epic: loops = 3; break;
            case Quality.Legendary: loops = 4; break;
            case Quality.Treasure: loops = 5; break;
            default: loops = 1; break;
        }
        // 旋转图片
        searchTween = DOTween.To(
            () => curAngle,
            x => {
                curAngle = x;
                UpdatePosition();
            },
            360f,
            360f / rotationSpeed
        )
        .SetEase(Ease.Linear)
        .SetLoops(loops, LoopType.Restart)
        .OnComplete(OnSearchComplete);
    }

    private void OnSearchComplete()
    {
        isSearched = true;
        HideSearchUI();

        if(this.onSearchComplete != null)
        {
            this.onSearchComplete?.Invoke();
            this.onSearchComplete = null;
        }
    }
    private void HideSearchUI()
    {
        if(itemUnSearched != null)
        {
            itemUnSearched.SetActive(false);
        }
        if(searchImage != null)
        {
            searchImage.gameObject.SetActive(false);
        }
    }
    private void ShowSearchUI()
    {
        if(itemUnSearched != null)
        {
            itemUnSearched.SetActive(true);
        }
        if(searchImage != null)
        {
            searchImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 更新图片位置
    /// </summary>
    private void UpdatePosition()
    {
        float radius = curAngle * Mathf.PI / 180f;
        searchImage.rectTransform.anchoredPosition = new Vector2(
            Mathf.Cos(radius) * this.radius,
            Mathf.Sin(radius) * this.radius
        );
    }
    /// <summary>
    /// 是否可以被拾取（已完成搜索）
    /// </summary>
    public bool CanPickUp => isSearched;
    #endregion
}
