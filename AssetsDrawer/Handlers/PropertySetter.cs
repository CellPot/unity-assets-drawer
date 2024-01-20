using AssetsDrawer.Data;
using UnityEditor;
using UnityEngine;

namespace AssetsDrawer.Handlers
{
    public class PropertySetter : IObjectPropertySetter
    {
        private readonly DrawerProperties _properties;
        
        private Vector3 _spawnRotation = Vector3.zero;
        private float _spawnScaleMod = 1;

        public PropertySetter(DrawerProperties properties)
        {
            _properties = properties;
        }

        public void UpdateSpawnRotation()
        {
            _spawnRotation = new Vector3(_properties.rotationDelta.x, _properties.rotationDelta.y,
                _properties.rotationDelta.z);
            if (_properties.randomizeRotation)
                _spawnRotation = new Vector3(0, Random.Range(0, 360), 0);
        }

        public void UpdateSpawnScaleMod()
        {
            var scaleMod = _properties.scaleMultiplier;
            if (_properties.randomizeScale)
                scaleMod = Random.Range(_properties.minRandomScale, _properties.maxRandomScale);
            _spawnScaleMod = scaleMod;
        }

        public void SetPreselectedPosition(GameObject gameObject, Vector3 transformPosition)
        {
            gameObject.transform.SetPositionAndRotation(transformPosition, gameObject.transform.rotation);
        }

        public void SetPreselectedRotation(GameObject gameObject)
        {
            gameObject.transform.Rotate(_spawnRotation);
        }

        public void SetPreselectedAlignment(GameObject gameObject, Vector3 upVector)
        {
            if (_properties.alignToNormal)
                gameObject.transform.up = upVector;
            else
                gameObject.transform.eulerAngles = Vector3.zero;
        }

        public void SetPreselectedScale(GameObject gameObject, Vector3 baseScale)
        {
            gameObject.transform.localScale = baseScale * _spawnScaleMod;
        }

        public void SetPreselectedParent(GameObject gameObject)
        {
            if (!_properties.setParent || Selection.activeGameObject == null ||
                !Selection.activeGameObject.activeInHierarchy) return;
            gameObject.transform.SetParent(Selection.activeGameObject.transform, true);
        }
    }

    public interface IObjectPropertySetter
    {
        void UpdateSpawnRotation();
        void UpdateSpawnScaleMod();
        void SetPreselectedPosition(GameObject gameObject, Vector3 transformPosition);
        void SetPreselectedRotation(GameObject gameObject);
        void SetPreselectedAlignment(GameObject gameObject, Vector3 upVector);
        void SetPreselectedScale(GameObject gameObject, Vector3 baseScale);
        void SetPreselectedParent(GameObject gameObject);
    }
}