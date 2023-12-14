using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        // print(_textBottomAlertComponent.rectTransform.sizeDelta.y);
        print(_bottomMessagePanelRectTransform.sizeDelta.y);
        _bottomMessagePanel.SetActive(true);
        print(_textBottomAlertComponent.GetComponent<TextMeshProUGUI>().text);
        RefreshHeight(onFinish: ()=> {
            // float widthFactor = Screen.width/720;
            //  float heightFactor = Screen.height/769;
            _bottomMessagePanel.SetActive(true);
            
            // _bottomMessagePanelRectTransform.anchoredPosition = new Vector3(0,  _textBottomAlertComponent.rectTransform.sizeDelta.y * -2, 0);
            print("sizeDelta.y : " + _textBottomAlertComponent.rectTransform.sizeDelta.y );
            
            print("height: " + Screen.height);
            return;
            Vector2 anchoredPosition = _bottomMessagePanelRectTransform.anchoredPosition;
            var position = _bottomMessagePanelTransform.position;
            
            Vector3 startPosition = new Vector3(Screen.width / 2, Screen.height, 0);
            Vector3 endPosition = new Vector3(Screen.width / 2, Screen.height -  30, 0);
            
            Vector2 startAnchoredPosition = new Vector2(anchoredPosition.x, Screen.height);
            // Vector2 endAnchoredPosition = new Vector2(anchoredPosition.x, 30);
            // Vector2 endAnchoredPosition = new Vector2(anchoredPosition.x, _textBottomAlertComponent.rectTransform.sizeDelta.y);
            Vector2 endAnchoredPosition = new Vector2(anchoredPosition.x, Screen.height / 2);

            
            StartCoroutine(MoveBottomMessage(startPosition: startAnchoredPosition, endPosition: endAnchoredPosition));
            
        });

        

        

        void RefreshHeight(Action onFinish = null)
        {
            _bottomMessagePanel.GetComponent<VerticalLayoutGroup>().enabled = false;
            _bottomMessagePanel.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            
            StartCoroutine(RefreshHeightAfterPause(onFinish));

            IEnumerator RefreshHeightAfterPause(Action onFinish = null) 
            {
                yield return new WaitForEndOfFrame();

                if (_bottomMessagePanel != null)
                {
                    _bottomMessagePanel.GetComponent<VerticalLayoutGroup>().enabled = true;
                    _bottomMessagePanel.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
                }

                onFinish?.Invoke();
            }
        }
    }
    
    public void Hide()
    {
        // !!!
        _bottomMessagePanel.SetActive(false);
        return;

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
