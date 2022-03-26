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

    private Transform _bottomMessagePanelTransform;

    private void Awake()
    {
        _bottomMessagePanelTransform = _bottomMessagePanel.transform;
    }

    public void SetText(string newText)
    {
        // Set color yellow.
        _textBottomAlertComponent.GetComponent<TextMeshProUGUI>().color = new Color32(255, 128, 0, 255);
        _textBottomAlertComponent.text = newText;
    }
    
    public void Show()
    {
        _bottomMessagePanel.SetActive(true);

        var position = _bottomMessagePanelTransform.position;
        
        Vector3 startPosition = new Vector3(position.x, -25, position.z);
        Vector3 endPosition = new Vector3(position.x, 23, position.z);

        StartCoroutine(MoveBottomMessage(startPosition: startPosition, endPosition: endPosition, callbackEnd: OnEnd));

        void OnEnd()
        {
            print("end move");
        }
    }
    
    public void Hide()
    {
        var position = _bottomMessagePanelTransform.position;
        
        Vector3 startPosition = new Vector3(position.x, 23, position.z);
        Vector3 endPosition = new Vector3(position.x, -25, position.z);

        StartCoroutine(MoveBottomMessage(startPosition, endPosition, callbackEnd: OnEndMove));

        void OnEndMove()
        {
            _bottomMessagePanel.SetActive(false);
        }
    }
    
    private IEnumerator MoveBottomMessage(Vector3 startPosition, Vector3 endPosition, int speed = 100, Action callbackEnd = null)
    {
        _bottomMessagePanelTransform.position = startPosition;
        
        while (_bottomMessagePanelTransform.position != endPosition)
        {           
            _bottomMessagePanelTransform.position = Vector3.MoveTowards(
                _bottomMessagePanelTransform.position,
                endPosition,
                Time.deltaTime * speed);

            yield return null;
        }

        if (callbackEnd != null) callbackEnd();
    }
}
