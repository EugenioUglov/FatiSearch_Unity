using Controllers;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;


public class SettingsData {
    public string IsDirectoryInTitle { get; set; }
    public string Theme { get; set; }
}

public class UserSettings : MonoBehaviour
{
    private string _settingsFilePath = @"Admin\Settings.json";


    public SettingsData GetSettings()
    {
        FileController fileController = new FileController();
        SettingsData settingsFromFile = new SettingsData();
        string settingsJSONFromFile = fileController.GetContentFromFile(_settingsFilePath);

        if (string.IsNullOrEmpty(settingsJSONFromFile) == false)
        {
            try
            {
                settingsFromFile =
                    JsonConvert.DeserializeObject<SettingsData>(settingsJSONFromFile);
            }
            catch (Exception exception)
            {
                print("Warning! File Settings.json not found");
                print(exception);
            }
        }

        return settingsFromFile;
    }

    public void ApplySettings() 
    {
        SettingsData settingsData = GetSettings();
        if (settingsData.Theme == "light")
        {
            Camera.main.backgroundColor = new Color32(180, 180, 180, 225);
        }
        else if (settingsData.Theme == "dark")
        {
            Camera.main.backgroundColor = new Color32(70, 70, 70, 0);
        }
        else if (settingsData.Theme == "darkest")
        {
            Camera.main.backgroundColor = new Color32(48, 48, 48, 0);
        }
    }
}
