using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandView : MonoBehaviour
{
    [SerializeField] private GameObject _inputFieldGO;
    private TMP_InputField _inputFieldTMP;


    private void Awake()
    {
        _inputFieldTMP = _inputFieldGO.GetComponent<TMP_InputField>();
    }

    public string GetTextFromInputField()
    {
        string text = _inputFieldTMP.text;
            
        return text;
    }

    public bool IsSelected()
    {
        return _inputFieldTMP.isFocused;
        
    }

    public void OnEnterInputField()
    {
        
    }


    public void SetFocus()
    {
        _inputFieldTMP.ActivateInputField();
        _inputFieldTMP.Select();
    }
}
