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

    public bool ShowInExplorer(string path)
    {
        bool isSelected = false;

        if (File.Exists(path) || Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + path));
            isSelected = true;
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

    /// <summary>
    /// Copy file with all folders where the file contained.
    /// </summary>
    /// <param name="fileToCopy"></param>
    /// <param name="targetDirectory"></param>
    /// <returns></returns>
    public string CopyFileKeepingFolders(string fileToCopy, string targetDirectory)
    {
        string directoryOfFileToCopy = Path.GetDirectoryName(fileToCopy);
        string fileNameToCopy = Path.GetFileName(fileToCopy);

        string directoryOfFileToCopyWithoutDriveOrNetworkShare = directoryOfFileToCopy.Substring(Path.GetPathRoot(directoryOfFileToCopy).Length);

        // If the last symbol in directory is not slash then add it.
        if (targetDirectory[targetDirectory.Length - 1] != '\\')
        {
            targetDirectory += @"\";
        }

        targetDirectory += directoryOfFileToCopyWithoutDriveOrNetworkShare + @"\";

        string targetFilePath = targetDirectory + fileNameToCopy;

        if (File.Exists(targetFilePath))
        {
            print("File already exists: " + targetFilePath);
            return "";
        }
        else 
        {

        }

        CreateDirectoryIfNotExist(targetDirectory);
        File.Copy(fileToCopy, targetFilePath);

        void CreateDirectoryIfNotExist(string directory)
        {
        try
        {
            // Determine whether the directory exists.
            if (Directory.Exists(directory))
            {

            }
            else 
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(directory);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(directory));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());
        }
        finally {}
        }

        return targetFilePath;
    }

    // !!! Not working well. Infinite loading.
    // public void CopyFiles(DirectoryInfo source, DirectoryInfo target) 
    // {
    //     Directory.CreateDirectory(target.FullName);

    //     foreach (var file in source.GetFiles())
    //     {
    //         // Thread.Sleep(50);
    //         file.CopyTo(Path.Combine(target.FullName, file.Name), true);
    //     }

    //     foreach (var sourceSubdirectory in source.GetDirectories())
    //     {
    //         // Thread.Sleep(50);
    //         var targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
    //         CopyFiles(sourceSubdirectory, targetSubdirectory);
    //     }
    // }
   
    public void RemoveFiles(DirectoryInfo directoryInfo)
    {
        foreach (FileInfo file in directoryInfo.EnumerateFiles())
        {
            file.Delete(); 
        }
    }
        
    /// <summary>
    /// Copy folder by path (sourcePath) to another directory (targetPath). If current folder already exists in destination directory then throw exception.
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    /// <returns>Target path of copied folder</returns>
    public string CopyFolder(string sourcePath, string targetPath)
    {
        DirectoryInfo pathInfo = new DirectoryInfo(sourcePath);
        string folderName = pathInfo.Name;
        string targetPathWithSourceFolderName = targetPath + @"\" + folderName;

        // get the file attributes for file or directory
        FileAttributes attr = File.GetAttributes(sourcePath);

        if (Directory.Exists(targetPathWithSourceFolderName))
        {
            throw new Exception("Current folder already exists");
        }

        if ((attr & FileAttributes.Directory) == FileAttributes.Directory == false)
        {
            throw new Exception("Source path is not a folder");
        }

        CopyFolderRecoursively(sourcePath, targetPathWithSourceFolderName);

        void CopyFolderRecoursively(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                DirectoryInfo createdDirectory = Directory.CreateDirectory(destFolder);
            }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolderRecoursively(folder, dest);
            }
        }

        return targetPathWithSourceFolderName;
    }

    /// <summary>
    /// Delete file or folder by path.
    /// </summary>
    public void DeleteByPath(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else if (Directory.Exists(path))
        { 
            Directory.Delete(path, true);
        }
        else {
            throw new Exception("Path is not found");
        }
    }

    /// <summary>
    /// Remove empty folders with subfolders.
    /// </summary>
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