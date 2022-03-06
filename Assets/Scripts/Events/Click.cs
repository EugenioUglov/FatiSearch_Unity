using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
 
public class Click : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent PointerUp;
    
    public void OnPointerDown (PointerEventData eventData) {
        
    }
 
    public void OnPointerUp (PointerEventData eventData) {
        PointerUp?.Invoke();
    }
}