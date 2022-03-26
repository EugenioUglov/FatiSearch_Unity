using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionBlockView : MonoBehaviour
{
    [SerializeField] private GameObject _actionBlockPrefab;
    [SerializeField] private GameObject _scrollViewContent;
    [SerializeField] private GameObject _searchPage;
    [SerializeField] private GameObject _executionErrorPanel;
    [SerializeField] private TextMeshProUGUI _centralLogText;
    [SerializeField] private TextMeshProUGUI _foundResultsText;
    [SerializeField] private GameObject _foundResultsGameObject;
    
    public Action CallbackStartLoadingActionBlocksToShow;
    public Action<string> CallBackActionBlockShowed;
    
    private List<GameObject> actionBlocksPrefabsShowed = new List<GameObject>();
    
    
    public void ShowActionBlocks(HashSet<ActionBlockModel.ActionBlock> actionBlocks)
    {
        // print("ShowActionBlocks");
        _searchPage.SetActive(false);
        if (CallbackStartLoadingActionBlocksToShow != null) CallbackStartLoadingActionBlocksToShow();

        ClearActionBlocks();

        List<string> imagePaths = new List<string>();
        
        foreach (var actionBlock in actionBlocks)
        {
            GameObject actionBlockPrefabShowed = Instantiate(_actionBlockPrefab, 
                _scrollViewContent.transform, false) as GameObject;
      
            actionBlocksPrefabsShowed.Add(actionBlockPrefabShowed);
            actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().SetTitle(actionBlock.Title);
            imagePaths.Add(actionBlock.ImagePath);
            
            if (Directory.Exists(actionBlock.Content) == false && File.Exists(actionBlock.Content) == false && IsURLValid(actionBlock.Content) == false)
            {
                actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().SetTitleColorRed();
            }
        }

        OnActionBlocksDownloaded();
        
        // Set images async.
        StartCoroutine(SetSprite(actionBlocksPrefabsShowed.ToArray(), imagePaths.ToArray()));
    }
    
    private Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (System.IO.File.Exists(path))
        {            
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 700, 340), new Vector2(0.5f, 0.0f), 1.0f);

            // You should return the sprite here!
            return sprite;
        }

        return null;
    }
    
    private IEnumerator SetSprite(GameObject[] actionBlocksPrefabsShowed, string[] imagePaths) 
    {
        for (var i = 0; i < actionBlocksPrefabsShowed.Length; i++)
        {
            string imagePath = imagePaths[i];
            
            if (string.IsNullOrEmpty(imagePath))
            {
                continue;
            }
            
            GameObject actionBlockPrefab = actionBlocksPrefabsShowed[i];
            
            WWW www = new WWW (imagePath);
       
            while(!www.isDone)
                yield return null;
        
            //GameObject image = GameObject.Find ("RawImage");
            //image.GetComponent<RawImage>().texture = www.texture;
            Image imageComponent = actionBlockPrefab.GetComponent<ActionBlockEntity>().Image.GetComponent<Image>();
            /*
            var spriteFromFile = Sprite.Create(www.texture, new Rect(0.0f, 0.0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            imageComponent.sprite = spriteFromFile;
            */
            try
            {
                var data = System.IO.File.ReadAllBytes(imagePath);
                var texture = new Texture2D(1, 1);
                texture.LoadImage(data);
                Sprite spriteFromFile = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.0f), 1.0f);
                imageComponent.sprite = spriteFromFile;

                float heightOfImageComponentActionBlock = imageComponent.rectTransform.sizeDelta.y;
                float heightOfDownloadedImage = texture.height;
                float newHeightImageForActionBlock = texture.height / (texture.width / imageComponent.rectTransform.sizeDelta.x);
                if (newHeightImageForActionBlock > heightOfImageComponentActionBlock)
                {
                    newHeightImageForActionBlock = heightOfImageComponentActionBlock;
                }
                
                imageComponent.rectTransform.sizeDelta = new Vector2(imageComponent.rectTransform.sizeDelta.x,
                    newHeightImageForActionBlock);
                // Set white color for image component to right visibility of image.
                imageComponent.color = new Color32(255, 255, 255, 255);
            }
            catch (Exception exception)
            {
                print(exception);
                print("Error! Not possible set image for Action-Block " + 
                                                    actionBlockPrefab.GetComponent<ActionBlockEntity>().GetTitle());
            }
        }
    }
    
    public void ClearActionBlocks()
    {
        foreach (var actionBlockButton in actionBlocksPrefabsShowed)
        {
            // Delete old searched Action-Blocks.
            
            Destroy(actionBlockButton);
        }
        
        actionBlocksPrefabsShowed.Clear();
    }

    public void ShowExecutionError()
    {
        _executionErrorPanel.SetActive(true);
    }
    
    public void HideExecutionError()
    {
        _executionErrorPanel.SetActive(false);
    }

    private void OnActionBlocksDownloaded()
    {
        _foundResultsText.text = "Found " + actionBlocksPrefabsShowed.Count + " results";
 
        
        _searchPage.SetActive(true);

        if (CallBackActionBlockShowed != null) CallBackActionBlockShowed(actionBlocksPrefabsShowed.Count.ToString());
    }
    
    private bool IsURLValid(string url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }
}
