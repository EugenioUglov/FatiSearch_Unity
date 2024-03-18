using UnityEngine;

namespace Controllers 
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private PageService _pageService;
        [SerializeField] private SearchService _searchService;
        
        
        void Update()
        {
            if (_pageService.PageState == PageService.PageStateEnum.SearchPage)
            {
                // _searchService.FocusInputField();
            }

            if (Input.anyKeyDown)
            {
                KeyClickedEvent keyClickedEvent = new KeyClickedEvent();
                keyClickedEvent.KeyCodeEntered = KeyCode.None;
                EventAggregator.Invoke<KeyClickedEvent>(keyClickedEvent);
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                KeyClickedEvent keyClickedEvent = new KeyClickedEvent();
                keyClickedEvent.KeyCodeEntered = KeyCode.Return;
                EventAggregator.Invoke<KeyClickedEvent>(keyClickedEvent);
            }

            // if (Input.GetKeyDown(KeyCode.Return))
            // {
            //     KeyDownEvent keyDowndEvent = new KeyDownEvent();
            //     keyDowndEvent.KeyCodeEntered = KeyCode.Return;
            //     EventAggregator.Invoke<KeyDownEvent>(keyDowndEvent);
            // }
        }
    }
}