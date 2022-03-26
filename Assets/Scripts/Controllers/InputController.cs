using UnityEngine;

namespace Controllers 
{
    public class InputController : MonoBehaviour
    {
        void Update()
        {
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
        }
    }
}