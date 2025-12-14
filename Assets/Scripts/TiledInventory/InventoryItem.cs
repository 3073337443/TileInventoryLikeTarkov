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
    public void InitSearchState()
    {
        if (isSearched)
        {
            // 已搜索完成：都隐藏
            HideCoverUI();
            HideSearchUI();
        }
        else
        {
            // 未搜索：显示 Cover，隐藏 Search
            ShowCoverUI();
            HideSearchUI();
        }
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
        this.isSearched = false;  // 重置搜索状态

        itemSprite.sprite = Resources.Load<Sprite>(itemData.spritePath); // 加载物品图片，路径为itemData中的spritePath。
        backgroundSpirite.color = GetQualityColor(itemData.quality);

        Vector2 size = new Vector2();
        size.x = itemData.width * ItemGrid.GetSimpleTileWidth(); // 使物品在tile的正中央显示
        size.y = itemData.height * ItemGrid.GetSimpleTileHeight(); // 使物品在tile的正中央显示
        GetComponent<RectTransform>().sizeDelta = size;

        InitSearchState();
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
            // 已搜索完成 隐藏遮盖和搜索UI
            HideCoverUI();
            HideSearchUI();
            if(onSearchComplete != null)
            {
                onSearchComplete?.Invoke();
            }
            return;
        }

        this.onSearchComplete = onSearchComplete;

        // 正在搜索 显示遮盖和搜索UI
        ShowCoverUI();
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
            case Quality.Epic: loops = 2; break;
            case Quality.Legendary: loops = 3; break;
            case Quality.Treasure: loops = 4; break;
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
    /// <summary>
    /// 搜索完成
    /// </summary>
    private void OnSearchComplete()
    {
        isSearched = true;

        // 搜索完成 隐藏遮盖和搜索UI
        HideCoverUI();
        HideSearchUI();

        if(this.onSearchComplete != null)
        {
            this.onSearchComplete?.Invoke();
            this.onSearchComplete = null;
        }
    }
    /// <summary>
    /// 停止搜索并重置进度
    /// </summary>
    public void StopAndResetSearch()
    {
        if( isSearched ) return;
        if(searchTween != null && searchTween.IsActive())
        {
            // 停止动画
            searchTween.Kill();
            searchTween = null;
        }
        // 重置角度
        curAngle = 0f;
        if(searchImage != null)
        {
            searchImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        onSearchComplete = null;

        ShowCoverUI();
        HideSearchUI();
    }
    private void HideCoverUI()
    {
        if(itemUnSearched != null)
        {
            itemUnSearched.SetActive(false);
        }

    }
    private void ShowCoverUI()
    {
        if(itemUnSearched != null)
        {
            itemUnSearched.SetActive(true);
        }
    }
    private void HideSearchUI()
    {
        if(searchImage != null)
        {
            searchImage.gameObject.SetActive(false);
        }
    }
    private void ShowSearchUI()
    {
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
