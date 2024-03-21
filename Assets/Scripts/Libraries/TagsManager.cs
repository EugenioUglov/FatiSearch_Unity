using System.Collections.Generic;
using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Threading.Tasks;
using System;
using System.Threading;

public class TagsManager : MonoBehaviour
{
    public async Task GetSyngularizedTagsAsync(string[] tags, Action<List<string>> onSingularizedTagsReady, Action onCancel = null, Action onFinish = null, Action<string> onError = null, CancellationTokenSource cancellationTokenSource = null)
    {
        NounNumber nounNumber = new NounNumber();
        List<string> singularizedTags = new List<string>();

        // Start the file move operation in a separate task
        Task copyTask = Task.Run(() =>
        {
            foreach (string tag in tags)
            {
                string[] wordsOfTag = tag.Split(' ');

                foreach (string wordOfTag in wordsOfTag)
                {
                    if (cancellationTokenSource != null &&  cancellationTokenSource.Token.IsCancellationRequested) 
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                            onCancel?.Invoke();
                        });
                        
                        return; 
                    }

                    string trimmedWordOfTag = wordOfTag.Trim();
                
                    try
                    {
                        string singularizedTag = nounNumber.GetSingularWord(trimmedWordOfTag);

                        if (trimmedWordOfTag != singularizedTag)
                        {
                            singularizedTags.Add(singularizedTag);
                        }
                    }
                    catch(Exception ex)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                            onError?.Invoke("Singularize API error: " + ex.Message);
                        });
                    }
                }
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() => { onSingularizedTagsReady?.Invoke(singularizedTags); });
        });

        await copyTask;

        UnityMainThreadDispatcher.Instance().Enqueue(() => { onFinish?.Invoke(); });
    }
}
