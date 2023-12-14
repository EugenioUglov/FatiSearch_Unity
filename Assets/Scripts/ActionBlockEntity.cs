using TMPro;
using UnityEngine;

public class ActionBlockEntity : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private GameObject _titleGameObject;
    [SerializeField] private GameObject _fileLocationButtonGameObject;
    public GameObject Image;

    
    public string GetTitle()
    {
        return _titleText.text;
    }

    public void SetTitleColorRed()
    {
        //_titleGameObject.GetComponent<TextMeshProUGUI>().color = new Color(186,89,89,255);
        // Set color red.
        _titleGameObject.GetComponent<TextMeshProUGUI>().color = new Color32(231, 54, 23, 255);
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

    public void OnClickButtonFileLocation()
    {
        ActionBlockFileLocationClickedEvent actionBlockFileLocationClickedEvent = new ActionBlockFileLocationClickedEvent();
        actionBlockFileLocationClickedEvent.Title = _titleText.text;
        EventAggregator.Invoke<ActionBlockFileLocationClickedEvent>(actionBlockFileLocationClickedEvent);
    }
    
    public void HideFileLocationButton()
    {
        _fileLocationButtonGameObject.SetActive(false);
    }
}
