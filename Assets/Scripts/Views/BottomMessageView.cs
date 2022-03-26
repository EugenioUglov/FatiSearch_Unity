using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
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
        
        Vector3 startPosition = new Vector3(_bottomMessagePanel.transform.position.x, -25,
            _bottomMessagePanel.transform.position.z);
        Vector3 endPosition = new Vector3(
            _bottomMessagePanel.transform.position.x, 23, _bottomMessagePanel.transform.position.z);
        int speed = 100;
        
        StartCoroutine(MoveBottomMessage(startPosition: startPosition, endPosition: endPosition, callbackEnd: OnEnd));

        void OnEnd()
        {
            print("end move");
        }
    }

    private IEnumerator MoveBottomMessage(Vector3 startPosition, Vector3 endPosition, int speed = 100, Action callbackEnd = null)
    {
        _bottomMessagePanel.transform.position = startPosition;
        
        while (_bottomMessagePanel.transform.position != endPosition)
        {           
            _bottomMessagePanel.transform.position = Vector3.MoveTowards(
                _bottomMessagePanel.transform.position,
                endPosition,
                Time.deltaTime * speed);

            yield return null;
        }

        if (callbackEnd != null) callbackEnd();
    }

    public void Hide()
    {
        Vector3 startPosition = new Vector3(_bottomMessagePanel.transform.position.x, 23,
            _bottomMessagePanel.transform.position.z);
        Vector3 endPosition = new Vector3(
            _bottomMessagePanel.transform.position.x, -25, _bottomMessagePanel.transform.position.z);
        int speed = 100;
        
        StartCoroutine(MoveBottomMessage(startPosition, endPosition, callbackEnd: OnEndMove));

        void OnEndMove()
        {
            _bottomMessagePanel.SetActive(false);
        }
    }
}
