using System.Collections.Generic;
using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Text.RegularExpressions;

public class TagManager : MonoBehaviour
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


    /// <summary>
    /// Get tags trimmed, with no multi spaces, split camel case, no special symbols.
    /// </summary>
    /// <param name="tagsSeparatedByComma"></param>
    /// <returns></returns>
    public List<string> GetNormalizedTags(List<string> tagsSeparatedByComma)
    {
        List<string> normalizedTags = new List<string>();

        
        foreach (string tag in tagsSeparatedByComma)
        {
            string tagWithoutSpecialSymbolsAndWithSplitedCamelCase = GetStringWithoutSpecialSymbolsAndWithSplitedCamelCase(tag);
            string tagWithNormalizedSpaces = GetStringWithNormalizedSpaces(tagWithoutSpecialSymbolsAndWithSplitedCamelCase);

            
            if (string.IsNullOrEmpty(tag) == false)
            {
                normalizedTags.Add(tag);
            }

            
            if (tagsSeparatedByComma.Contains(tagWithoutSpecialSymbolsAndWithSplitedCamelCase) == false)
            {
                normalizedTags.Add(tagWithoutSpecialSymbolsAndWithSplitedCamelCase);
            }
        }


        string GetStringWithoutSpecialSymbolsAndWithSplitedCamelCase(string stringToNormalize)
        {
            StringManager stringManager = new StringManager();

            string stringWithoutSpecialSymbolsAndWithSplitedCamelCase = "";

            string stringWithoutSpecialSymbols = stringManager.GetTextWithoutSpecialSymbols(stringToNormalize);
            string[] words = stringWithoutSpecialSymbols.Split(' ');


            foreach (string word in words)
            {
                if (IsAllUpper(word))
                {
                    stringWithoutSpecialSymbolsAndWithSplitedCamelCase += word + " ";

                    continue;
                }
                
                stringWithoutSpecialSymbolsAndWithSplitedCamelCase += stringManager.SplitCamelCase(word) + " ";
            }


            bool IsAllUpper(string input)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    {
                        return false;
                    }
                }

                return true;
            }


            return stringWithoutSpecialSymbolsAndWithSplitedCamelCase;
        }

        string GetStringWithNormalizedSpaces(string strToNormalize)
        {
            // Delete empty spaces from sides of tags.
            string tagTrimmed = strToNormalize.Trim();

            // Repalce multiple spaces to one space.
            string tagTrimmedWithoutMultipleSpaces = Regex.Replace(tagTrimmed, @"\s+", " ");


            return tagTrimmedWithoutMultipleSpaces;
        }

        return normalizedTags;
    }
}
