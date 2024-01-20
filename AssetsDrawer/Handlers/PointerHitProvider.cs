using UnityEditor;
using UnityEngine;

namespace AssetsDrawer.Handlers
{
    public class PointerHitProvider : IPointerHitProvider
    {
        private RaycastHit _pointerHit;
        public RaycastHit PointerHit => _pointerHit;

        public bool TryUpdatePointerHit(Vector2 pointerPosition, int layerMask)
        {
            var worldSpaceRay = HandleUtility.GUIPointToWorldRay(pointerPosition);
            var raycastResult =
                DrawerUtility.PerformInfiniteRaycast(worldSpaceRay.origin, worldSpaceRay.direction, layerMask,
                    out var hitInfo);
            if (raycastResult)
                _pointerHit = hitInfo;

            return raycastResult;
        }
    }

    public interface IPointerHitProvider
    {
        RaycastHit PointerHit { get; }
        public bool TryUpdatePointerHit(Vector2 pointerPosition, int layerMask);
    }
}