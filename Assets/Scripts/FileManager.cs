using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;

public class FileManager : MonoBehaviour
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
}
