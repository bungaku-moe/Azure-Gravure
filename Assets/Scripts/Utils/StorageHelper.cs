using System.IO;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;

namespace Kiraio.Azure.Utils
{
    public static class StorageHelper
    {
        /// <summary>
        /// Get current application/project root directory.
        /// </summary>
        /// <returns>Application/project root full path.</returns>
        public static string GetApplicationPath()
        {
#if UNITY_ANDROID || UNITY_WEBGl && !UNITY_EDITOR
            return $"file:///{Directory.GetParent(Application.persistentDataPath)!.ToString()}";
#elif UNITY_STANDALONE_OSX
            return $"file://{Directory.GetParent(Application.dataPath)!.ToString()}";
#else
            return Directory.GetParent(Application.dataPath)!.ToString();
#endif
        }

        /// <summary>
        /// Open file dialog and fill the <paramref name="inputField"/>.
        /// </summary>
        /// <param name="inputField"></param>
        /// <param name="windowTitle"></param>
        public static string[] OpenFileDialog(
            TMP_InputField inputField,
            string windowTitle = "Load File",
            bool allowMultiple = false
        )
        {
            string[] selectedPaths = new string[0];
            FileBrowser.ShowLoadDialog(
                (paths) =>
                {
                    selectedPaths = paths;
                    inputField.text = string.Join(", ", paths);
                },
                () => { },
                FileBrowser.PickMode.Files,
                allowMultiple,
                GetApplicationPath(),
                null,
                windowTitle
            );
            return selectedPaths;
        }

        /// <summary>
        /// Open directory dialog and fill the <paramref name="inputField"/>.
        /// </summary>
        /// <param name="inputField"></param>
        public static string[] OpenDirectoryDialog(
            TMP_InputField inputField,
            string windowTitle = "Load Directory",
            bool allowMultiple = false
        )
        {
            string[] directories = new string[0];
            FileBrowser.ShowLoadDialog(
                (paths) =>
                {
                    directories = paths;
                    inputField.text = string.Join(", ", paths);
                },
                () => { },
                FileBrowser.PickMode.Folders,
                allowMultiple,
                GetApplicationPath(),
                null,
                windowTitle
            );
            return directories;
        }
    }
}
