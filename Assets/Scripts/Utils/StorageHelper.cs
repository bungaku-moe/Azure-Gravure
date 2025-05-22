using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using UnityEngine;

namespace Kiraio.Azure.Utils
{
    public static class StorageHelper
    {
        /// <summary>
        ///     Get current application/project root directory.
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
        ///     Open File Dialog.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="allowMultiple"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static async UniTask<string[]> OpenFileDialogAsync(
            string windowTitle = "Load File",
            bool allowMultiple = false,
            string[] filters = null
        )
        {
            var tcs = new UniTaskCompletionSource<string[]>();
            FileBrowser.SetFilters(true, filters);
            FileBrowser.SetDefaultFilter(filters?[0]);
            FileBrowser.ShowLoadDialog(
                paths => tcs.TrySetResult(paths),
                () => tcs.TrySetResult(Array.Empty<string>()),
                FileBrowser.PickMode.Files,
                allowMultiple,
                GetApplicationPath(),
                null,
                windowTitle
            );
            return await tcs.Task;
        }

        /// <summary>
        ///     Open Directory Dialog.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="allowMultiple"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static async UniTask<string[]> OpenDirectoryDialogAsync(
            string windowTitle = "Load Directory",
            bool allowMultiple = false,
            string[] filters = null
        )
        {
            var tcs = new UniTaskCompletionSource<string[]>();
            FileBrowser.SetFilters(true, filters);
            FileBrowser.SetDefaultFilter(filters?[0]);
            FileBrowser.ShowLoadDialog(
                paths => tcs.TrySetResult(paths),
                () => tcs.TrySetResult(Array.Empty<string>()),
                FileBrowser.PickMode.Folders,
                allowMultiple,
                GetApplicationPath(),
                null,
                windowTitle
            );
            return await tcs.Task;
        }

        /// <summary>
        ///     Normalize the path by removing invalid characters.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePath(string path)
        {
            var sanitizedPath = string.Concat(path.Where(c => !Path.GetInvalidPathChars().Contains(c)));
            return Path.GetFullPath(sanitizedPath);
        }
    }
}
