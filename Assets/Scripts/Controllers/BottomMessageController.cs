using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomMessageController : MonoBehaviour
{
    [SerializeField] private BottomMessageView _view;

    private Queue<string> _queueOfMessages = new Queue<string>();
    private bool _isActive = false;


    public void Show(string text)
    {
        if (_isActive)
        {
            _queueOfMessages.Enqueue(text);
        } 
        else
        {
            _view.SetText(text);
            _view.Show();
            _isActive = true;
            StartCoroutine(ShowNextMessage(3));
        }
    }

    public void Hide()
    {
        _view.Hide();
        _isActive = false;
    }
    
    private IEnumerator ShowNextMessage(float secondsToHold)
    {
        yield return new WaitForSeconds(secondsToHold);
        Hide();
        
        if (_queueOfMessages.Count != 0)
        {
            // Show next message.
            string messageFromQueue = _queueOfMessages.Dequeue();
            yield return new WaitForSeconds(1);
            Show(messageFromQueue);
        }
    }
}
