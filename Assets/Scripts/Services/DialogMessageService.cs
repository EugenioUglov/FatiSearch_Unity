using UnityEngine;
using System.Windows.Forms;
using System.Collections;
using System;

public class DialogMessageService : MonoBehaviour
{
    [SerializeField] private MessageFullscreenService _messageFullscreenService;

    public void ShowMessage(string text, Action<DialogResult>? onClickButton = null)
    {
        ShowPanelWaitingForResponseOnDialogBox();

        StartCoroutine(ShowDialogueNoFreeze());

        IEnumerator ShowDialogueNoFreeze()
        {
            yield return null;

            DialogResult dialogResult = MessageBox.Show(text);

            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes || dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
            {
                _messageFullscreenService.Hide();
                onClickButton?.Invoke(dialogResult);
            }
        }
    }

    public void ShowMessage(string text, string caption, Action<DialogResult> onClickButton = null)
    {
        ShowPanelWaitingForResponseOnDialogBox();

        StartCoroutine(ShowDialogNoFreeze());

        IEnumerator ShowDialogNoFreeze()
        {
            yield return null;

            DialogResult dialogResult = MessageBox.Show(text, caption);

            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes || dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
            {
                _messageFullscreenService.Hide();
                onClickButton?.Invoke(dialogResult);
            }
        }
    }
    
    public void ShowMessage(string text, string caption, MessageBoxButtons buttons, Action<DialogResult>? onClickButton = null)
    {
        ShowPanelWaitingForResponseOnDialogBox();

        StartCoroutine(ShowDialogueNoFreeze());

        IEnumerator ShowDialogueNoFreeze()
        {
            yield return null;
            DialogResult dialogResult = MessageBox.Show(text, caption, buttons);

            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes || dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
            {
                _messageFullscreenService.Hide();
                onClickButton?.Invoke(dialogResult);
            }
        }
    }

    public void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, Action<DialogResult> onClickButton = null)
    {
        ShowPanelWaitingForResponseOnDialogBox();

        StartCoroutine(ShowDialogueNoFreeze());

        IEnumerator ShowDialogueNoFreeze()
        {
            yield return null;
            DialogResult dialogResult = MessageBox.Show(text, caption, buttons, icon);

            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes || dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
            {
                _messageFullscreenService.Hide();
                onClickButton?.Invoke(dialogResult);
            }
        }
    }


    private void ShowPanelWaitingForResponseOnDialogBox()
    { 
        _messageFullscreenService.Show(
            text:"You need to responde to a dialog box window.",
            title: MessageFullscreenService.Title.WaitingForResponse
        );
    }
}
