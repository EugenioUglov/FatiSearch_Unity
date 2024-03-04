using TMPro;
using UnityEngine;

public class CommandView : MonoBehaviour
{
    [SerializeField] private GameObject _inputFieldGO;
    private TMP_InputField _inputFieldTMP;


    private void Awake()
    {
        _inputFieldTMP = _inputFieldGO.GetComponent<TMP_InputField>();

        _inputFieldTMP.onSelect.AddListener(delegate
        {
            print("Command input field Selected");
        });
        
        _inputFieldTMP.onDeselect.AddListener(delegate
        {
            print("Command input field Deselected");
        });
    }

    public string GetTextFromInputField()
    {
        string text = _inputFieldTMP.text;
            
        return text;
    }

    public void ClearInputField()
    {
        _inputFieldTMP.text = "";
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
