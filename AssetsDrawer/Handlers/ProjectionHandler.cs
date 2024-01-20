using AssetsDrawer.Data;
using UnityEngine;

namespace AssetsDrawer.Handlers
{
    public class ProjectionHandler : IProjectionHandler
    {
        private readonly DrawerProperties _properties;
        private readonly IObjectPropertySetter _propertySetter;
        private readonly IPointerHitProvider _hitProvider;

        private int _projectionIndex = -1;
        private GameObject _projectionObject;

        public ProjectionHandler(DrawerProperties properties, IObjectPropertySetter propertySetter, IPointerHitProvider hitProvider)
        {
            _properties = properties;
            _propertySetter = propertySetter;
            _hitProvider = hitProvider;
        }
        
        public void UpdateProjection(int spawnIndex)
        {
            if (spawnIndex == -1 || _properties.assetsPalette.Count <= 0 || _properties.assetsPalette?[spawnIndex] == null)
            {
                DestroyProjection();
                _projectionIndex = -1;
                return;
            }

            if (spawnIndex != _projectionIndex)
            {
                DestroyProjection();
                _projectionIndex = spawnIndex;
            }

            if (_projectionObject == null)
                _projectionObject = DrawerUtility.InstantiatePrefab(_properties.assetsPalette[spawnIndex]);

            _propertySetter.SetPreselectedAlignment(_projectionObject, _hitProvider.PointerHit.normal);
            _propertySetter.SetPreselectedRotation(_projectionObject);
            _propertySetter.SetPreselectedScale(_projectionObject,
                _properties.assetsPalette[spawnIndex].transform.localScale);
            _propertySetter.SetPreselectedPosition(_projectionObject,
                _hitProvider.PointerHit.point + _properties.positionDelta);
            _projectionObject.DisableColliders();
            _projectionObject.hideFlags = HideFlags.HideAndDontSave;
        }

        public void DestroyProjection()
        {
            if (_projectionObject == null) return;
            
            Object.DestroyImmediate(_projectionObject);
        }
    }

    public interface IProjectionHandler
    {
        void UpdateProjection(int spawnIndex);
        void DestroyProjection();
    }
}