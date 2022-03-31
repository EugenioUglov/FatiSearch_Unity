using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using Screen = UnityEngine.Screen;

public class BottomMessageView : MonoBehaviour
{
    [SerializeField] private GameObject _bottomMessagePanel;
    [SerializeField] private TextMeshProUGUI _textBottomAlertComponent;
    [SerializeField] private Transform _visiblePointTransform;
    [SerializeField] private Transform _unvisiblePointTransform;

    private Transform _bottomMessagePanelTransform;
    private RectTransform _bottomMessagePanelRectTransform;

    private void Awake()
    {
        _bottomMessagePanelTransform = _bottomMessagePanel.transform;
        _bottomMessagePanelRectTransform = _bottomMessagePanel.GetComponent<RectTransform>();
    }

    public void SetText(string newText)
    {
        // Set color yellow.
        _textBottomAlertComponent.GetComponent<TextMeshProUGUI>().color = new Color32(255, 128, 0, 255);
        _textBottomAlertComponent.text = newText;
    }
    
    public void Show()
    {
        Vector2 anchoredPosition = _bottomMessagePanelRectTransform.anchoredPosition;

        _bottomMessagePanel.SetActive(true);

        var position = _bottomMessagePanelTransform.position;
        
        Vector3 startPosition = new Vector3(Screen.width / 2, Screen.height, 0);
        Vector3 endPosition = new Vector3(Screen.width / 2, Screen.height -  30, 0);
        
        Vector2 startAnchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y);
        Vector2 endAnchoredPosition = new Vector2(anchoredPosition.x, 30);

        StartCoroutine(MoveBottomMessage(startPosition: startAnchoredPosition, endPosition: endAnchoredPosition));
    }
    
    public void Hide()
    {
        var position = _bottomMessagePanelTransform.position;
        Vector2 anchoredPosition = _bottomMessagePanelRectTransform.anchoredPosition;
        
        Vector3 startPosition = new Vector3(Screen.width / 2, Screen.height -  30, 0);
        Vector3 endPosition = new Vector3(Screen.width / 2, Screen.height, 0);
        
        Vector2 startAnchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y);
        Vector2 endAnchoredPosition = new Vector2(anchoredPosition.x, -30);
        
        StartCoroutine(MoveBottomMessage(startAnchoredPosition, endAnchoredPosition, callbackEnd: OnEndMove));

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

        callbackEnd?.Invoke();
    }
    
    private IEnumerator MoveBottomMessage(Vector2 startPosition, Vector2 endPosition, int speed = 100, Action callbackEnd = null)
    {
        _bottomMessagePanelRectTransform.anchoredPosition = startPosition;
        
        while (_bottomMessagePanelRectTransform.anchoredPosition != endPosition)
        {           
            _bottomMessagePanelRectTransform.anchoredPosition = Vector3.MoveTowards(
                _bottomMessagePanelRectTransform.anchoredPosition,
                endPosition,
                Time.deltaTime * speed);

            yield return null;
        }

        callbackEnd?.Invoke();
    }
}
