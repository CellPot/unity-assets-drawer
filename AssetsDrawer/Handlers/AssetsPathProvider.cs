using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace AssetsDrawer.Handlers
{
    public class AssetsPathProvider : IPathProvider, IDisposable
    {
        private string _assetsPath;
        private bool _isPathCompositionModified;

        public AssetsPathProvider()
        {
            PathProviderPostprocessor.OnProjectAssetsCompositionModified += UpdateAssetsComposition;
        }

        public string AssetsPath => _assetsPath;

        public bool UpdatePath(string newPath)
        {
            newPath = newPath.Trim();

            if (string.IsNullOrEmpty(newPath) || !IsExistingPath())
                newPath = GetCurrentProjectWindowPath();

            if (IsPathNotModified())
                return false;

            _isPathCompositionModified = false;
            _assetsPath = newPath;
            return true;

            bool IsExistingPath() =>
                Directory.Exists(newPath);

            bool IsPathNotModified() =>
                _assetsPath == newPath && !_isPathCompositionModified;
        }

        public void Dispose()
        {
            PathProviderPostprocessor.OnProjectAssetsCompositionModified -= UpdateAssetsComposition;
        }

        private void UpdateAssetsComposition(string path)
        {
            if (_assetsPath != path) return;

            _isPathCompositionModified = true;
        }

        private static string GetCurrentProjectWindowPath()
        {
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            var getActiveFolderPath =
                projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            var obj = getActiveFolderPath?.Invoke(null, Array.Empty<object>());
            var pathToCurrentFolder = obj?.ToString();
            return pathToCurrentFolder;
        }

        private class PathProviderPostprocessor : AssetPostprocessor
        {
            public static event Action<string> OnProjectAssetsCompositionModified;

            public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                var pathsArray = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths)
                    .ToArray();
                foreach (var path in pathsArray)
                {
                    var indexOfName = path.LastIndexOf('/');
                    var directoryPath = path[..indexOfName];
                    OnProjectAssetsCompositionModified?.Invoke(directoryPath);
                }
            }
        }
    }

    public interface IPathProvider
    {
        public string AssetsPath { get; }
        bool UpdatePath(string newPath);
    }
}