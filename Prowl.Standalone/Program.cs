﻿using Prowl.Runtime;
using Prowl.Runtime.SceneManagement;

namespace Prowl.Standalone;

internal class Program {

    public static DirectoryInfo Data => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameData"));

    public static int Main(string[] args) {

        StandaloneApplication standaloneApplication = new();
        standaloneApplication.Initialize();
        Application.AssetProvider = new StandaloneAssetProvider();

        FileInfo StartingScene = new FileInfo(Path.Combine(Data.FullName, "level.prowl"));
        if (File.Exists(StartingScene.FullName))
        {
            SerializedProperty tag = BinaryTagConverter.ReadFromFile(StartingScene);
            Scene scene = Serializer.Deserialize<Scene>(tag);
            SceneManager.LoadScene(scene);
        }

        Window.Start();

        return 0;
    }
    
}
