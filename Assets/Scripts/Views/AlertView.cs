using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlertView : MonoBehaviour
{
    [SerializeField] private GameObject _alertPanel;
    [SerializeField] private TextMeshProUGUI _textComponent;


    public void SetText(string newText)
    {
        _textComponent.text = newText;
    }
    
    public void Show()
    {
        _alertPanel.SetActive(true);
    }
    
    public void Hide()
    {
        _alertPanel.SetActive(false);
    }
}
