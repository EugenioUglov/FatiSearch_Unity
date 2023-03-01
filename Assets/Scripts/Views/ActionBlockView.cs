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
    [Header("UI elements for settings Action-Block")]
    public GameObject TitleInputField;
    public GameObject ContentInputField;
    public GameObject TagsInputField;
    public GameObject ImagePathInputField;
    
    [SerializeField] private GameObject _actionBlockPrefab;
    [SerializeField] private GameObject _loadingTextPrefab;

    [SerializeField] private GameObject _scrollViewContent;
    [SerializeField] private GameObject _searchPage;
    [SerializeField] private GameObject _executionErrorPanel;
    [SerializeField] private TextMeshProUGUI _centralLogText;
    [SerializeField] private TextMeshProUGUI _foundResultsText;
    [SerializeField] private GameObject _foundResultsGameObject;
    [SerializeField] private GameObject _scrollView;
    [SerializeField] private Scrollbar _scrollbar;
    
    private List<GameObject> _actionBlocksPrefabsShowed = new List<GameObject>();
    
    private float _topScrollbarValue = 1;
    private float _bottomScrollbarValue = 0;
    private GameObject loadingTextPrefabShowed;
    
    
    public void ShowActionBlocks(HashSet<ActionBlockModel.ActionBlock> actionBlocks)
    {
        _searchPage.SetActive(false);

        ClearActionBlocks();

        List<string> imagePaths = new List<string>();
        
        foreach (var actionBlock in actionBlocks)
        {
            GameObject actionBlockPrefabShowed = Instantiate(_actionBlockPrefab, 
                _scrollViewContent.transform, false) as GameObject;
      
            _actionBlocksPrefabsShowed.Add(actionBlockPrefabShowed);
            actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().SetTitle(actionBlock.Title);
            imagePaths.Add(actionBlock.ImagePath);
            
            if (Directory.Exists(actionBlock.Content) == false && File.Exists(actionBlock.Content) == false && IsURLValid(actionBlock.Content) == false)
            {
                actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().SetTitleColorRed();
            }
        }

        OnActionBlocksShowed();
        
        // Set images async.
        StartCoroutine(SetSprite(_actionBlocksPrefabsShowed.ToArray(), imagePaths.ToArray()));
    }

    public void AddActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        BlockScrollCapability();

        GameObject actionBlockPrefabShowed = Instantiate(_actionBlockPrefab, 
            _scrollViewContent.transform, false) as GameObject;

        _scrollbar.value = 0.02f;
        UnblockScrollCapability();

      
        _actionBlocksPrefabsShowed.Add(actionBlockPrefabShowed);
        actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().SetTitle(actionBlock.Title);
            
        if (Directory.Exists(actionBlock.Content) == false && File.Exists(actionBlock.Content) == false && IsURLValid(actionBlock.Content) == false)
        {
            actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().SetTitleColorRed();
            actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().HideFileLocationButton();
        }
        
        // Set images async.
        StartCoroutine(SetSprite(actionBlockPrefabShowed, actionBlock.ImagePath));
    }
    
    public void ClearActionBlocks()
    {
        _scrollbar.value = _topScrollbarValue;
        
        foreach (var actionBlockButton in _actionBlocksPrefabsShowed)
        {
            // Delete old searched Action-Blocks.
            
            Destroy(actionBlockButton);
        }
        
        _actionBlocksPrefabsShowed.Clear();
    }

    public void ShowExecutionError()
    {
        _executionErrorPanel.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // _scrollbar.value -= 0.1f;
        }
    }

    public void BindScrollbarValueChange(Action<float> handler)
    {
        _scrollbar.onValueChanged.AddListener((float val) => handler(val));
    }
    
    public void HideExecutionError()
    {
        _executionErrorPanel.SetActive(false);
    }
    
    public void SetDefaultSettingsFields()
    {
        TitleInputField.GetComponent<TMP_InputField>().text = "";
        ContentInputField.GetComponent<TMP_InputField>().text = "";
        TagsInputField.GetComponent<TMP_InputField>().text = "";
        ImagePathInputField.GetComponent<TMP_InputField>().text = "";
    }

    public string ShowCountTextFoundActionBlocks(int count)
    {
        _foundResultsText.text = "Found " + count + " results";
        
        return _foundResultsText.text;
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
    
    private IEnumerator SetSprite(GameObject actionBlockPrefabShowed, string imagePath) 
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            yield break;
        }
        
        
        WWW www = new WWW (imagePath);
   
        while(!www.isDone)
            yield return null;
    
        //GameObject image = GameObject.Find ("RawImage");
        //image.GetComponent<RawImage>().texture = www.texture;
        /*
        var spriteFromFile = Sprite.Create(www.texture, new Rect(0.0f, 0.0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        imageComponent.sprite = spriteFromFile;
        */
        try
        {
            Image imageComponent = actionBlockPrefabShowed.GetComponent<ActionBlockEntity>().Image.GetComponent<Image>();
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
            print("Warning! Not possible set image for Action-Block");
        }
    }

    private void OnActionBlocksShowed()
    {
        _searchPage.SetActive(true);
    }
    
    private bool IsURLValid(string url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }
    
    public void BlockScrollCapability()
    {
        _scrollbar.interactable = false;
        _scrollView.GetComponent<ScrollRect>().vertical = false;
    }

    public void UnblockScrollCapability()
    {
        _scrollbar.interactable = true;
        _scrollView.GetComponent<ScrollRect>().vertical = true;
    }

    public void ScrollToTop()
    {
        _scrollbar.value = 1f;
    }

    public void AddLoadingText()
    {
       loadingTextPrefabShowed = Instantiate(_loadingTextPrefab, 
            _scrollViewContent.transform, false) as GameObject;

        _scrollbar.value = 1;
    }

    public void DestroyLoadingText()
    {
        Destroy(loadingTextPrefabShowed);
    }
}

