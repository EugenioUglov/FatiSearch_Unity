using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BottomMessageView : MonoBehaviour
{
    [SerializeField] private GameObject _bottomMessagePanel;
    [SerializeField] private TextMeshProUGUI _textBottomAlertComponent;
    
    
    public void SetText(string newText)
    {
        // Set color yellow.
        _textBottomAlertComponent.GetComponent<TextMeshProUGUI>().color = new Color32(255, 128, 0, 255);
        _textBottomAlertComponent.text = newText;
    }
    
    public void Show()
    {
        _bottomMessagePanel.SetActive(true);

       // StartCoroutine(MoveBottomMessage());
    }

    private IEnumerator MoveBottomMessage()
    {
        Vector3 newPosition = new Vector3(
            _bottomMessagePanel.transform.position.x,
            _bottomMessagePanel.transform.position.y + 50, 
            _bottomMessagePanel.transform.position.z);
        
      
        _bottomMessagePanel.transform.localPosition = Vector3.MoveTowards(
            new Vector3(_bottomMessagePanel.transform.localPosition.x, -25, _bottomMessagePanel.transform.localPosition.z), 
            new Vector3(
                _bottomMessagePanel.transform.localPosition.x, 
                50, 
                _bottomMessagePanel.transform.localPosition.z),
            Time.deltaTime * 1);
        
        print(newPosition);
        
        yield return null;
    }

    public void Hide()
    {
        _bottomMessagePanel.SetActive(false);
    }
}
