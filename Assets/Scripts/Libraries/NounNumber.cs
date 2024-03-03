using UnityEngine;
using System.Net;
using System;

/// <summary>
/// Takes data from the API of noun number: Singular or Plural.
/// </summary>
public class NounNumber : MonoBehaviour
{
    public string GetSingularWord(string stringToSingularize)
    {
        string singularWord = "";

        try
        {
            using (var client = new WebClient())
            {
                var result = client.DownloadString(string.Format("https://funny-blue-poncho.cyclic.app/singular?request={0}", stringToSingularize));
                
                ResponseFromJSON resulJSON = JsonUtility.FromJson<ResponseFromJSON>(result);
                singularWord = resulJSON.response;
            }
        }
        catch (Exception ex)
        {
            print("Error: " + ex.Message);
        }

        return singularWord;
    }

    public string GetPluralWord(string stringToPluralize)
    {
        string pluralWord = "";

        try
        {
            using (var client = new WebClient())
            {
                var result = client.DownloadString(string.Format("https://funny-blue-poncho.cyclic.app/plural?request={0}", stringToPluralize));
                
                ResponseFromJSON resulJSON = JsonUtility.FromJson<ResponseFromJSON>(result);
                pluralWord = resulJSON.response;
            } 
        }
        catch (Exception ex)
        {
            print("Error: " + ex.Message);
        }

        return pluralWord;
    }

    
    [Serializable]
    public class ResponseFromJSON
    {
        public string response;
    }
}


// ANOTHER WAY TO FETCH DATA.
// string url="https://yessirapi.000webhostapp.com/index.php";
// StartCoroutine(Test());
// IEnumerator Test() {
//     using (WWW www = new WWW(url))
//     {
//         yield return www;
//         print(www.text);
//     }
// }