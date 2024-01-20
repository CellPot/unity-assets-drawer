using System.Collections.Generic;
using AssetsDrawer.Data;
using UnityEditor;
using UnityEngine;

namespace AssetsDrawer.Handlers
{
    public class AssetsSpawner : IAssetsSpawner
    {
        private readonly DrawerProperties _properties;
        private readonly IObjectPropertySetter _propertySetter;

        private int _preselectedSpawnIndex = -1;

        public AssetsSpawner(DrawerProperties properties, IObjectPropertySetter propertySetter)
        {
            _properties = properties;
            _propertySetter = propertySetter;
        }

        public int PreselectedSpawnIndex => _preselectedSpawnIndex;

        public GameObject SpawnPreselectedObjectAt(Vector3 spawnPoint, Vector3 upVector)
        {
            var selected = GetObjectByIndex(_properties.assetsPalette, _preselectedSpawnIndex);
            var newObject = DrawerUtility.InstantiatePrefab(selected);

            if (newObject == null)
                return null;

            _propertySetter.SetPreselectedAlignment(newObject, upVector);
            _propertySetter.SetPreselectedRotation(newObject);
            _propertySetter.SetPreselectedScale(newObject, newObject.transform.localScale);
            _propertySetter.SetPreselectedPosition(newObject, spawnPoint);
            _propertySetter.SetPreselectedParent(newObject);
            UpdateSpawnIndex();
            _propertySetter.UpdateSpawnRotation();
            _propertySetter.UpdateSpawnScaleMod();
            Physics.SyncTransforms();
            Undo.RegisterCreatedObjectUndo(newObject, $"{newObject.name} instantiation");
            return newObject;
        }

        public void UpdateSpawnIndex()
        {
            var selected = _properties.contentSelection.GetIndexesOfState(state: true);
            _preselectedSpawnIndex = GetNewPrefabIndex(selected, true);
        }

        private int GetNewPrefabIndex(IReadOnlyList<int> indexes, bool excludeRepetition)
        {
            if (indexes.Count == 0)
                return -1;
            if (indexes.Count == 1)
                return indexes[0];

            var randomIndex = indexes[GetRandom(indexes.Count)];
            
            if (excludeRepetition && randomIndex == _preselectedSpawnIndex)
                randomIndex = GetNewPrefabIndex(indexes, true);

            return randomIndex;
        }

        private static GameObject GetObjectByIndex(List<GameObject> objects, int index)
        {
            if (index < 0 || objects.Count <= 0 || objects?[index] == null) return null;

            return objects[index];
        }

        private static int GetRandom(int maxExclusive) => 
            Random.Range(0, maxExclusive);
    }

    public interface IAssetsSpawner
    {
        int PreselectedSpawnIndex { get; }
        GameObject SpawnPreselectedObjectAt(Vector3 spawnPoint, Vector3 upVector);
        void UpdateSpawnIndex();
    }
}