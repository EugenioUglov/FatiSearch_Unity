using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;

public class DirectoryManager : MonoBehaviour
{
    List<string> _logs = new List<string>();


    public bool OpenDirectory(string diretory)
    {
        bool isOpened = false;
        
        try
        {
            Process.Start(diretory);
            isOpened = true;

            _logs.Add("Directory opened: " + diretory);
        }
        catch (Exception exception)
        {
            _logs.Add("Not possible to open directory: " + diretory);
        }

        return isOpened;
    }
    
    public bool OpenDirectoryAsAdministrator(string diretory)
    {
        bool isOpened = false;
        
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = diretory;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.Start();
            // Process.Start(diretory);
            isOpened = true;

            _logs.Add("Directory opened: " + diretory);
        }
        catch (Exception exception)
        {
            _logs.Add("Not possible to open directory: " + diretory);
        }

        return isOpened;
    }

    public bool GoToFileLocation(string path)
    {
        bool isSelected = false;

        if (File.Exists(path))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + path));
            isSelected = true;

            _logs.Add("GoToFileLocation: " + path);
        }
        else {
            _logs.Add("Not possible open file location: " + path);
        }

        return isSelected;
    }

    public string[] GetFileDirectoriesFromFolderWithSubfolders(string folderDirectory)
    {
        string[] allfiles = Directory.GetFiles(folderDirectory, "*.*", SearchOption.AllDirectories);
        // List<string> fileDirectories = new List<string>(); 
        string[] fileDirectories = new string[allfiles.Length];


        int iFile = 0;

        foreach (var file in allfiles){
            FileInfo info = new FileInfo(file);
            // Do something with the Folder or just add them to a list via nameoflist.add();
            fileDirectories[iFile] = info.FullName;

            iFile++;
        }

        _logs.Add("Directory opened: " + folderDirectory);


        return fileDirectories;
    }
    
    public IEnumerator GetFilesFromDirectoryAsync(string directory, Action<string> onGetFile = null, Action<string[]> onEnd = null)
    {
        List<string> files = new List<string>();

        foreach(var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)) 
        {
            yield return null;
            
            files.Add(file);

            onGetFile?.Invoke(file);
        }
        

        onEnd?.Invoke(files.ToArray());
    }

    public void CreateDirectoryIfDoesNotExist(string directory)
    {
        if (!Directory.Exists(directory)) 
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void MoveFiles(DirectoryInfo source, DirectoryInfo target) 
    {
        Directory.CreateDirectory(target.FullName);

        foreach (var file in source.GetFiles())
        {
            string targetPath = Path.Combine(target.FullName, file.Name);

            if (File.Exists(targetPath))
            {
                print("File already exists: " + targetPath);
                continue;
            }
            else 
            {
                print("Move file " + targetPath);
                file.MoveTo(targetPath);
            }
        }

        foreach (var sourceSubdirectory in source.GetDirectories())
        {
            var targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
            MoveFiles(sourceSubdirectory, targetSubdirectory);
        }
    }

   
    // !!! Not working well. Infinite loading.
    public void CopyFiles(DirectoryInfo source, DirectoryInfo target) 
    {
        Directory.CreateDirectory(target.FullName);

        foreach (var file in source.GetFiles())
        {
            // Thread.Sleep(50);
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        foreach (var sourceSubdirectory in source.GetDirectories())
        {
            // Thread.Sleep(50);
            var targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
            CopyFiles(sourceSubdirectory, targetSubdirectory);
        }
    }
   
    public void RemoveFiles(DirectoryInfo directoryInfo)
    {
        foreach (FileInfo file in directoryInfo.EnumerateFiles())
        {
            file.Delete(); 
        }
    }
        
    /*
        Remove empty folders with subfolders.
    */
    public void RemoveEmptyFolders(string startLocation)
    {    
        foreach (var directory in Directory.GetDirectories(startLocation))
        {
            RemoveEmptyFolders(directory);
            if (Directory.GetFiles(directory).Length == 0 && 
                Directory.GetDirectories(directory).Length == 0)
            {
                Directory.Delete(directory, false);
            }
        }
    }

    public void RemoveFolders(DirectoryInfo directoryInfo)   
    {
        foreach (DirectoryInfo dir in directoryInfo.EnumerateDirectories())
        {
            dir.Delete(true); 
        }
    }
}
