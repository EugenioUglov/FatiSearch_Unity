using System;
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

    public string SplitCamelCase(string input)
    {
        if (IsAllUpper(input))
        {
            return input;
        }

        bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }


        return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
    }
}
