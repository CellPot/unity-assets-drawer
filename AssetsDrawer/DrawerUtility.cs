using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AssetsDrawer
{
    public static class DrawerUtility
    {
        public const string UntaggedTag = "Untagged";
        public const int RaycastMargin = 5;
        private static readonly Collider[] OverlappingColliders = new Collider[5];

        public static List<GameObject> GetAssetFiles(string path, string searchPattern)
        {
            var assets = new List<GameObject>();
            var files = Directory.GetFiles(path, searchPattern);
            foreach (var file in files)
                assets.Add(AssetDatabase.LoadAssetAtPath(file, typeof(GameObject)) as GameObject);

            return assets;
        }

        public static void ProjectTransforms(IEnumerable<Transform> transforms, LayerMask surfaceMask, bool align)
        {
            foreach (var transform in transforms)
                ProjectTransform(transform, Vector3.up, surfaceMask, align);
        }

        public static bool IsLastIndex(int index, int length)
        {
            return index == length - 1;
        }

        public static void DestroyObjectsAt(Vector3 deletionPosition, float radius, string tag)
        {
            if (tag == string.Empty) return;

            var objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            foreach (var gameObject in objectsWithTag)
            {
                var distance = Vector3.Distance(deletionPosition, gameObject.transform.position);

                if (gameObject == null || !(distance <= radius)) continue;
                
                TryDestroyObject(gameObject);
            }
        }

        public static void DestroyObjectsAt(Vector3 deletionPosition, float radius, LayerMask mask)
        {
            if (mask.value == 0) return;
            for (var i = 0; i < OverlappingColliders.Length; i++) 
                OverlappingColliders[i] = null;

            Physics.OverlapSphereNonAlloc(deletionPosition, radius, OverlappingColliders, mask);
            foreach (var collider in OverlappingColliders)
            {
                if (collider == null) continue;

                TryDestroyObject(collider.gameObject);
            }
        }

        private static void TryDestroyObject(GameObject objectToDestroy)
        {
            if (IsDestroyable(objectToDestroy))
                Undo.DestroyObjectImmediate(objectToDestroy);
            else
                Debug.LogWarning($"Can't destroy the object {objectToDestroy.name}. Check if it's a part of a prefab");
            
            bool IsDestroyable(GameObject gameObject) => 
                 !PrefabUtility.IsPartOfAnyPrefab(gameObject) || PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject);
        }

        public static DrawerMode GetDrawerMode(Event guiEvent)
        {
            var currentMode = DrawerMode.Default;
            var isDrawSuitable = !guiEvent.alt && guiEvent.button == 0;
            if (guiEvent.shift)
            {
                if (isDrawSuitable && guiEvent.type is EventType.MouseDown or EventType.MouseDrag)
                    currentMode = DrawerMode.EraseBrushDown;
                else
                    currentMode = DrawerMode.EraseBrushReady;
            }
            else if (!guiEvent.shift && isDrawSuitable && guiEvent.type == EventType.MouseDown)
                currentMode = DrawerMode.PaintBrushPushDown;
            else if (!guiEvent.shift && isDrawSuitable && guiEvent.type == EventType.MouseDrag)
                currentMode = DrawerMode.PaintBrushDrag;
            else if (guiEvent.control && guiEvent.keyCode == KeyCode.C && guiEvent.type == EventType.KeyDown)
                currentMode = DrawerMode.RotationLeft;
            else if (guiEvent.control && guiEvent.keyCode == KeyCode.X && guiEvent.type == EventType.KeyDown)
                currentMode = DrawerMode.RotationRight;

            return currentMode;
        }

        public static GameObject InstantiatePrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            return instance;
        }

        public static List<int> GetIndexesOfState(this bool[] array, bool state)
        {
            var selectedIndexes = new List<int>();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == state)
                    selectedIndexes.Add(i);
            }

            return selectedIndexes;
        }

        public static bool Compare(this List<GameObject> list1, List<GameObject> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            return !list1.Where((go, i) => go != list2[i]).Any();
        }

        public static void DisableColliders(this GameObject gameObject)
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
                collider.enabled = false;
        }

        public static bool HasCollision(this Vector3 point, Vector3 upVector, LayerMask mask, string tag = "")
        {
            if (Physics.Raycast(point + upVector * RaycastMargin, -upVector, out var hit))
            {
                if (tag != string.Empty && tag != UntaggedTag && hit.collider.CompareTag(tag))
                    return true;
                if (mask.ContainsLayer(hit.collider.gameObject.layer))
                    return true;
            }

            return false;
        }

        public static Vector3 GetRandomVectorIn(float radius) =>
            Random.insideUnitSphere * radius;

        public static void ProjectTransform(Transform transform, Vector3 upVector, LayerMask surfaceMask, bool align)
        {
            if (!transform) return;

            var raycastResult = Physics.Raycast(transform.position + upVector * RaycastMargin, -upVector, out var hit,
                Mathf.Infinity, surfaceMask);
            if (!raycastResult) return;

            if (align)
                transform.AlignTransform(hit.normal);
            transform.position = hit.point;
        }

        public static void AlignTransform(this Transform transform, Vector3 up)
        {
            var forwardProjection = Vector3.ProjectOnPlane(transform.forward, up);
            transform.rotation = Quaternion.LookRotation(forwardProjection, up);
        }

        public static bool ContainsLayer(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public static bool PerformInfiniteRaycast(Vector3 origin, Vector3 direction, int layerMask,
            out RaycastHit hitInfo)
        {
            return Physics.Raycast(origin, direction, out hitInfo, Mathf.Infinity, layerMask);
        }
    }
}