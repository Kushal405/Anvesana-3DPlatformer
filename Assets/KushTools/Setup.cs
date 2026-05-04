#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEditor.AssetDatabase;

public static class Setup
{
    [MenuItem("KushTools/Setup/Create Default Folders")]
    public static void CreateDefaultFolders()
    {
        Folders.CreateDefault("_Project", 
            "Animation", 
            "Art", 
            "Materials", 
            "Audio", 
            "Editor", 
            "Prefabs", 
            "Resources", 
            "Scripts", 
            "Settings"
        );
        Refresh();
    }

    [MenuItem("KushTools/Setup/Import My Favorite Assets")]
    public static void ImportMyFavoriteAssets()
    {
        Assets.ImportAsset("DOTween.unitypackage", "DemiGiant/ScriptingAnimation");
    }

    public static class Folders
    {
        public static void CreateDefault(string root, params string[] folders)
        {
            var fullPath = Path.Combine(Application.dataPath, root);
            foreach (var folder in folders)
            {
                var path = Path.Combine(fullPath, folder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
    }

    public static class Assets
    {
        public static void ImportAsset(string asset, string subfolder,
            string folder = "C:\\Users\\mznkr\\AppData\\Roaming\\Unity\\Asset Store-5.x")
        {
            ImportPackage(Path.Combine(folder, subfolder, asset), false);
        }
    }
}
#endif