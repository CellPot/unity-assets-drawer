using System;
using UnityEngine;

namespace AssetsDrawer.Data
{
    [Serializable]
    public class DrawerSettings
    {
        public bool foldGroup1State;
        public float rotationMaxSpeed = 1f;
        public float handlesThickness = 2f;
        public bool showHandlesNormal = true;
        public bool showProjection = true;
        public bool normalizeRotationDelta = true;
        public Color handlesColor = Color.green;
        public Color erasureHandlesColor = Color.red;
        public Color idleHandlesColor = Color.yellow;
        public int maxElementsInLine = 3;
        public float minToggleSize = 50;
        public float maxToggleSize = 150;
    }
}