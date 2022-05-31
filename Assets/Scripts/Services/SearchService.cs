using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Views;

public class SearchService : MonoBehaviour
{
    [SerializeField] private SearchView _view;
    
    public void FocusInputField()
    {
        _view.FocusInputField();
    }
}
