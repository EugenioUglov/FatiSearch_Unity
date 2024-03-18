using UnityEngine;
using System.IO;

public class FileStreamManager : MonoBehaviour
{
    public string GetContentFromFile(string path)
    {
        using (StreamReader streamReader = new StreamReader(path))
        {
            string content = streamReader.ReadToEnd();
            
            return content;
        }

        // 2 WAY
        // string[] lines = System.IO.File.ReadAllLines(path);

        // string contentOfFile = "";
        
        // foreach (string line in lines)
        // {
        //     contentOfFile += line;
        // }

        // return contentOfFile;
    }
    
    public void Save(string path, string content)
    {
        FileStream fParameter = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter m_WriterParameter = new StreamWriter(fParameter);
        m_WriterParameter.BaseStream.Seek(0, SeekOrigin.End);
        m_WriterParameter.Write(content);
        m_WriterParameter.Flush();
        m_WriterParameter.Close();
    }

    public void Save(string path, string name, string extension, string content)
    {
        path = path + name + "." + extension;
  
        FileStream fParameter = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter m_WriterParameter = new StreamWriter(fParameter);
        m_WriterParameter.BaseStream.Seek(0, SeekOrigin.End);
        m_WriterParameter.Write(content);
        m_WriterParameter.Flush();
        m_WriterParameter.Close();
    }
}
