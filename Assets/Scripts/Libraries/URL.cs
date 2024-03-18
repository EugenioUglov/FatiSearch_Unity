using System.Text.RegularExpressions;

public class URL
{
    public bool IsValidUrl(string url)
    {
        string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
        return regex.IsMatch(url);
    }
}
