using System;
using AssetsDrawer.Data;
using AssetsDrawer.Handlers;
using UnityEditor;
using UnityEngine;

namespace AssetsDrawer
{
    [Serializable]
    public class AssetsDrawerHandler
    {
        private const string SearchPattern = "*.prefab";

        private DrawerProperties _properties;
        private DrawerSettings _settings;

        private IPointerDrawer _pointerDrawer;
        private IPointerHitProvider _hitProvider;
        private IPathProvider _pathProvider;
        private IAssetsSpawner _spawner;
        private IObjectPropertySetter _propertySetter;
        private IProjectionHandler _projectionHandler;

        private DrawerMode _drawerMode = DrawerMode.Default;
        private RaycastHit _cachedPointerHit;

        public IObjectPropertySetter PropertySetter => _propertySetter;
        public IProjectionHandler ProjectionHandler => _projectionHandler;

        public void Initialize(DrawerProperties properties, DrawerSettings settings)
        {
            _properties = properties;
            _settings = settings;
            _hitProvider = new PointerHitProvider();
            _pathProvider = new AssetsPathProvider();
            _propertySetter = new PropertySetter(_properties);
            _pointerDrawer = new HandlesPointerDrawer(_properties, _settings);
            _spawner = new AssetsSpawner(_properties, _propertySetter);
            _projectionHandler = new ProjectionHandler(_properties, _propertySetter, _hitProvider);

            if (_properties.contentSelection != null)
            {
                _spawner.UpdateSpawnIndex();
                _projectionHandler.UpdateProjection(_spawner.PreselectedSpawnIndex);
            }
        }

        public void DeInitialize()
        {
            (_pathProvider as IDisposable)?.Dispose();
        }

        public void HandleMousePosition(Vector2 mousePosition)
        {
            var updateResult = _hitProvider.TryUpdatePointerHit(mousePosition, _properties.surfaceLayerMask);
            if (updateResult)
            {
                if (ShouldShowProjection())
                    _projectionHandler.UpdateProjection(_spawner.PreselectedSpawnIndex);
                else
                    _projectionHandler.DestroyProjection();

                _pointerDrawer.DrawPointer(_hitProvider.PointerHit.point, _hitProvider.PointerHit.normal, _drawerMode,
                    _spawner.PreselectedSpawnIndex != -1);
            }

            bool ShouldShowProjection() =>
                _settings.showProjection && _properties.spawnAmountPerClick == 1 &&
                _drawerMode is not DrawerMode.EraseBrushReady and not DrawerMode.EraseBrushDown;
        }

        public void HandleSceneViewInputs(Event currentEvent)
        {
            _drawerMode = DrawerUtility.GetDrawerMode(currentEvent);

            if (_drawerMode is DrawerMode.EraseBrushDown)
            {
                HandleEraseAction();
            }
            else if (_drawerMode is DrawerMode.PaintBrushPushDown)
            {
                HandleDrawAction();
            }
            else if (_drawerMode is DrawerMode.PaintBrushDrag)
            {
                if (IsFarEnough(_hitProvider.PointerHit.point, _cachedPointerHit.point))
                    HandleDrawAction();
            }
            else if (_drawerMode is DrawerMode.RotationLeft)
            {
                HandleRotationAction(-1);
            }
            else if (_drawerMode is DrawerMode.RotationRight)
            {
                HandleRotationAction(1);
            }

            bool IsFarEnough(Vector3 position1, Vector3 position2) =>
                Vector3.Distance(position1, position2) >= _properties.spread;
        }

        public void ProjectSelectedOntoSurface()
        {
            Undo.RecordObjects(Selection.transforms, "Project transforms");
            DrawerUtility.ProjectTransforms(Selection.transforms, _properties.surfaceLayerMask,
                _properties.alignProjected);
        }

        public void UpdateSelectionState(int index, bool state)
        {
            if (_properties.contentSelection[index] == state) return;

            _properties.contentSelection[index] = state;
            _spawner.UpdateSpawnIndex();
            _projectionHandler.UpdateProjection(_spawner.PreselectedSpawnIndex);
        }

        public void UpdateLogic()
        {
            var isPathUpdated = _pathProvider.UpdatePath(_properties.pathToPalette);
            if (isPathUpdated)
                UpdateAssetsPalette();
        }

        private void UpdateAssetsPalette()
        {
            var newAssets = DrawerUtility.GetAssetFiles(_pathProvider.AssetsPath, SearchPattern);
            if (newAssets == null) return;

            var isSameList = newAssets.Compare(_properties.assetsPalette);

            if (isSameList) return;

            _properties.assetsPalette.Clear();
            _properties.assetsPalette = newAssets;
            _properties.contentSelection = new bool[_properties.assetsPalette.Count];
            _spawner.UpdateSpawnIndex();
            _projectionHandler.UpdateProjection(_spawner.PreselectedSpawnIndex);
        }

        private void HandleEraseAction()
        {
            if (_properties.assetsLayerMask.value != 0)
                DrawerUtility.DestroyObjectsAt(_hitProvider.PointerHit.point, _properties.circleRadius,
                    _properties.assetsLayerMask);
            else
                DrawerUtility.DestroyObjectsAt(_hitProvider.PointerHit.point, _properties.circleRadius,
                    _properties.assetsTag);
        }

        private void HandleDrawAction()
        {
            if (_hitProvider.PointerHit.collider == null) return;
            HandleSpawnIteration();
            _cachedPointerHit = _hitProvider.PointerHit;
        }

        private void HandleRotationAction(float angleMod)
        {
            _properties.rotationDelta = Vector3.MoveTowards(_properties.rotationDelta,
                new Vector3(_properties.rotationDelta.x,
                    _properties.rotationDelta.y + _settings.rotationMaxSpeed * angleMod, _properties.rotationDelta.z),
                _settings.rotationMaxSpeed);
            _propertySetter.UpdateSpawnRotation();
        }

        private void HandleSpawnIteration()
        {
            var currentHit = _hitProvider.PointerHit;
            if (_properties.spawnAmountPerClick > 1)
            {
                for (var i = 0; i < _properties.spawnAmountPerClick; i++)
                {
                    if (currentHit.collider == null)
                        continue;

                    var raycastResult = PerformRaycastAtRandomCirclePoint(out currentHit);
                    if (raycastResult)
                    {
                        if (_properties.avoidOverlapping &&
                            currentHit.point.HasCollision(currentHit.normal, _properties.assetsLayerMask,
                                _properties.assetsTag))
                            continue;

                        TrySpawnAt(currentHit.point, currentHit.normal);
                    }
                }
            }
            else
                TrySpawnAt(currentHit.point, currentHit.normal);

            _projectionHandler.UpdateProjection(_spawner.PreselectedSpawnIndex);

            GameObject TrySpawnAt(Vector3 spawnPoint, Vector3 upVector)
            {
                if (_properties.avoidOverlapping &&
                    spawnPoint.HasCollision(upVector, _properties.assetsLayerMask, _properties.assetsTag))
                    return null;

                return _spawner.SpawnPreselectedObjectAt(spawnPoint, upVector);
            }
        }

        private bool PerformRaycastAtRandomCirclePoint(out RaycastHit hit)
        {
            var randomVector = DrawerUtility.GetRandomVectorIn(_properties.circleRadius);
            var projection = Vector3.ProjectOnPlane(randomVector, _hitProvider.PointerHit.normal);
            var randomPos = new Vector3(_hitProvider.PointerHit.point.x + projection.x,
                _hitProvider.PointerHit.point.y + projection.y,
                _hitProvider.PointerHit.point.z + projection.z);
            var origin = randomPos + _hitProvider.PointerHit.normal * DrawerUtility.RaycastMargin;
            var raycastResult =
                DrawerUtility.PerformInfiniteRaycast(origin, -_hitProvider.PointerHit.normal,
                    _properties.surfaceLayerMask,
                    out hit);

            return raycastResult;
        }
    }
}