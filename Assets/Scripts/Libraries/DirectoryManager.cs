using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;

public class DirectoryManager : MonoBehaviour
{
    public bool OpenDirectory(string directory)
    {
        bool isOpened = false;

        directory = directory.Replace(@"\\", @"\");
        
        try
        {
            Process.Start(directory);
            isOpened = true;
        }
        catch (Exception exception)
        {
            print(exception.Message);
        }

        return isOpened;
    }
    
    public bool OpenDirectoryAsAdministrator(string directory)
    {
        bool isOpened = false;

        directory = directory.Replace(@"\\", @"\");
        
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = directory;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.Start();
            // Process.Start(diretory);
            isOpened = true;
        }
        catch (Exception exception)
        {
            print(exception.Message);
        }

        return isOpened;
    }

    public bool ShowInExplorer(string path)
    {
        bool isSelected = false;

        path = path.Replace(@"\\", @"\");

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

        foreach (var file in allfiles)
        {
            FileInfo info = new FileInfo(file);
            // Do something with the Folder or just add them to a list via nameoflist.add();
            fileDirectories[iFile] = info.FullName;

            iFile++;
        }

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
    public void CopyFileKeepingFoldersAsync(string fileToCopy, string targetDirectory, Action<string> onDone, Action<string> onFail)
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
            // print("File already exists: " + targetFilePath);
            onFail("File already exists: " + targetFilePath);
            return;
        }
        else 
        {

        }

        CreateDirectoryIfNotExist(targetDirectory);

        Task task = Task.Run(async () => await CopyFileAsync(
            fileToCopy, 
            targetFilePath, 
            onDone: (message) => {
                UnityMainThreadDispatcher.Instance().Enqueue(() => { onDone(targetFilePath); });
            }
        ));

        async Task CopyFileAsync(string fileToCopy, string targetFilePath, Action<string> onDone)
        {
            // Create a CancellationTokenSource to facilitate cancellation
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            
            // Cancel the file move operation
            // cancellationTokenSource.Cancel();

            // Start the file move operation in a separate task
            Task moveTask = Task.Run(() =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested) return;
                
                try
                {
                    File.Copy(fileToCopy, targetFilePath);
                    onDone("File moved successfully.");
                }
                catch (Exception ex)
                {
                    onDone($"Error occurred: {ex.Message}");
                }
            });

            print("after task run");

            await moveTask;

            print("File move process stopped.");
        }

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

        // return targetFilePath;
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
    public void CopyFolderAsync(string sourcePath, string targetPath, Action<string> onDone)
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

            Task task = Task.Run(async () => await CopyFolderFilesAsync(
                sourceFolder, 
                destFolder, 
                onDone: (messages) => {
                    foreach (string message in messages)
                    {
                        print(message);
                    }

                    string[] folders = Directory.GetDirectories(sourceFolder);
                
                    foreach (string folder in folders)
                    {
                        string name = Path.GetFileName(folder);
                        string dest = Path.Combine(destFolder, name);
                        CopyFolderRecoursively(folder, dest);
                    }

                    UnityMainThreadDispatcher.Instance().Enqueue(() => { onDone(targetPathWithSourceFolderName); });
                }
            ));
        }

        async Task CopyFolderFilesAsync(string sourceFolder, string destFolder, Action<List<string>> onDone)
        {
            List<string> messages = new List<string>();

            // Create a CancellationTokenSource to facilitate cancellation
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            
            // Cancel the file move operation
            // cancellationTokenSource.Cancel();

            // Start the file move operation in a separate task
            Task moveTask = Task.Run(() =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested) return;
                
                string[] files = Directory.GetFiles(sourceFolder);

                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    
                    try
                    {
                        File.Copy(file, dest);
                        messages.Add($"File \"{file}\" moved successfully.");
                    }
                    catch (Exception ex)
                    {
                        messages.Add($"Error occurred for file \"{file}\": {ex.Message}");
                    }
                }
            });

            await moveTask;

            onDone(messages);
            
            print("File move process stopped.");
        }

        // return targetPathWithSourceFolderName;
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