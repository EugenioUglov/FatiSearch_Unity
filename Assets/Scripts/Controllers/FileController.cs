using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FileController : MonoBehaviour
{  
    public string GetContentFromFile(string path)
    {
        using (StreamReader streamReader = new StreamReader(path))
        {
            string content = streamReader.ReadToEnd();
            
            return content;
        }
    }
    
    public void Save(string path, string name, string extension, string content)
    {
        path = path + name + "." + extension;
        //string Msg = name + ";" + telephone + ";" + address;
  
        // Save File to .txt
        FileStream fParameter = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter m_WriterParameter = new StreamWriter(fParameter);
        m_WriterParameter.BaseStream.Seek(0, SeekOrigin.End);
        m_WriterParameter.Write(content);
        m_WriterParameter.Flush();
        m_WriterParameter.Close();
    }
    
    public void Save(string path, string content)
    {
        //string Msg = name + ";" + telephone + ";" + address;
  
        // Save File to .txt
        FileStream fParameter = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter m_WriterParameter = new StreamWriter(fParameter);
        m_WriterParameter.BaseStream.Seek(0, SeekOrigin.End);
        m_WriterParameter.Write(content);
        m_WriterParameter.Flush();
        m_WriterParameter.Close();
    }

    public FileInfo[] GetFilesFromFolder()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo (Application.persistentDataPath);
        FileInfo[] fileInfo = directoryInfo.GetFiles ("*.*", SearchOption.AllDirectories);
        
        /* SET TO DROPDOWN */
        GameObject.Find ("File Selection").GetComponent<Dropdown> ().options.Clear ();
        foreach (FileInfo file in fileInfo) {
            Dropdown.OptionData optionData = new Dropdown.OptionData (file.Name);
            GameObject.Find ("File Selection").GetComponent<Dropdown> ().options.Add (optionData);
            GameObject.Find ("File Selection").GetComponent<Dropdown> ().value = 1;
        }

        return fileInfo;
    }
    
    /*
    public string GetContentFromFileByPath(string path)
    {
        string[] lines = System.IO.File.ReadAllLines(path);

        string contentOfFile = "";
        
        // Display the file contents by using a foreach loop.
       
        
        foreach (string line in lines)
        {
            contentOfFile += line;
        }

        return contentOfFile;
    }

    
     // !!!
     // EditorUtility error ob building: The name 'EditorUtility' does not exist in the current context
    public string ChooseFile()
    {
        string path = EditorUtility.OpenFilePanel("Choose file with Action-Blocks", "", "");
        return GetContentFromFileByPath(path);
    }
    */
    

}
