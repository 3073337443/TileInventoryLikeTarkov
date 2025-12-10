using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class ABtest : MonoBehaviour
{
    public Image heal;
    public Image eye;

    // Start is called before the first frame update
    void Start()
    {
        LoadSpriteAssets(eye, "sprite", "eye");
        StartCoroutine(LoadSpriteAssetsCoroutine(heal, "sprite", "heal"));
    }
    void LoadSpriteAssets(Image image, string ABName, string resName) 
    {
        AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + ABName);
        Sprite obj = ab.LoadAsset<Sprite>(resName);
        image.sprite = obj;
        ab.Unload(false);

    }
    IEnumerator LoadSpriteAssetsCoroutine(Image image, string ABName, string resName) 
    {
        image.gameObject.SetActive(false);
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + ABName);
        yield return abcr;
        AssetBundleRequest abr = abcr.assetBundle.LoadAssetAsync<Sprite>(resName);
        yield return abr;
        image.sprite = abr.asset as Sprite;
        yield return new WaitForSeconds(1f);
        image.gameObject.SetActive(true);
        abcr.assetBundle.Unload(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
