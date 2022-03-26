using Controllers;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private SearchController _searchController;

    void Start()
    {
        _dragAndDropController.Init();
        _searchController.Init();
        _actionBlockController.Init();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
