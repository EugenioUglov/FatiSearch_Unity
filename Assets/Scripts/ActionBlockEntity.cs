using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ActionBlockEntity : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private GameObject _titleGameObject;
    public GameObject Image;

  
    
    public string GetTitle()
    {
        return _titleText.text;
    }

    public void SetTitleColorRed()
    {
        //_titleGameObject.GetComponent<TextMeshProUGUI>().color = new Color(186,89,89,255);
        // Set color red.
        _titleGameObject.GetComponent<TextMeshProUGUI>().color = new Color32(255, 38, 0, 255);
    }
    
    public void SetTitle(string title)
    {
        _titleText.text = title;
    }
    
    public void OnClick()
    {
        ActionBlockClickedEvent actionBlockClickedEvent = new ActionBlockClickedEvent();
        actionBlockClickedEvent.Title = _titleText.text;
        EventAggregator.Invoke<ActionBlockClickedEvent>(actionBlockClickedEvent);
    }
    
    public void OnClickButtonSettings()
    {
        ActionBlockSettingsClickedEvent actionBlockSettingsClickedEvent = new ActionBlockSettingsClickedEvent();
        actionBlockSettingsClickedEvent.Title = _titleText.text;
        EventAggregator.Invoke<ActionBlockSettingsClickedEvent>(actionBlockSettingsClickedEvent);
    }
}
