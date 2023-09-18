using System.Text.RegularExpressions;


public class StringManager
{
    public string GetTextWithoutSpecialSymbols(string textToCheck)
    {
        return Regex.Replace(textToCheck, @"[^0-9a-zA-Z]", " ");
        
        /*
        // 2 Way
        StringBuilder sb = new StringBuilder();
        
        foreach (char c in str) {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                sb.Append(c);
            }
        }
        
        return sb.ToString();
        */
    }
}
