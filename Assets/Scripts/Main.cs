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
        
        UserSettings settings = new UserSettings();
        SettingsData settingsData = settings.GetSettings();

        if (settingsData.Theme == "light")
        {
            Camera.main.backgroundColor = new Color32(180, 180, 180, 225);
        }
        else if (settingsData.Theme == "dark")
        {
            Camera.main.backgroundColor = new Color32(70, 70, 70, 0);
        }
        else if (settingsData.Theme == "darkest")
        {
            Camera.main.backgroundColor = new Color32(48, 48, 48, 0);
        }
        _dragAndDropController.Init();
        _searchController.Init();
        _actionBlockController.Init();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
