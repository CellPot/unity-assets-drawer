using AssetsDrawer.Data;
using UnityEditor;
using UnityEngine;

namespace AssetsDrawer.Handlers
{
    public class HandlesPointerDrawer : IPointerDrawer
    {
        private readonly DrawerProperties _properties;
        private readonly DrawerSettings _settings;

        public HandlesPointerDrawer(DrawerProperties properties, DrawerSettings settings)
        {
            _properties = properties;
            _settings = settings;
        }

        public void DrawPointer(Vector3 center, Vector3 up, DrawerMode mode, bool isOperable)
        {
            if (_settings.handlesThickness <= float.Epsilon) return;

            Handles.color = GetPointerColor(mode, isOperable);
            Handles.DrawWireDisc(center, up, _properties.circleRadius,
                _settings.handlesThickness);
            if (_settings.showHandlesNormal)
            {
                var lineDelta = new Vector3(up.x, up.y, up.z);
                Handles.DrawLine(center, center + lineDelta, _settings.handlesThickness);
            }
        }

        private Color GetPointerColor(DrawerMode mode, bool operable)
        {
            var color = _settings.handlesColor;
            if (mode is DrawerMode.EraseBrushReady or DrawerMode.EraseBrushDown)
                color = _settings.erasureHandlesColor;
            else if (!operable)
                color = _settings.idleHandlesColor;
            return color;
        }
    }

    public interface IPointerDrawer
    {
        void DrawPointer(Vector3 center, Vector3 up, DrawerMode mode, bool isOperable);
    }
}