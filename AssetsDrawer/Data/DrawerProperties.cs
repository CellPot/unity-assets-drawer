using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetsDrawer.Data
{
    [Serializable]
    public class DrawerProperties
    {
        public LayerMask surfaceLayerMask = ~0;
        public LayerMask assetsLayerMask = 0;
        public string assetsTag = DrawerUtility.UntaggedTag;

        public float spread = 1;
        public float circleRadius = 2;
        public bool randomizeScale;
        public float scaleMultiplier = 1;
        public float minRandomScale = 1;
        public float maxRandomScale = 1;
        public bool alignToNormal = true;
        public bool randomizeRotation;
        public int spawnAmountPerClick = 1;
        public bool avoidOverlapping = true;
        public bool setParent;
        public Vector3 positionDelta = Vector3.zero;
        public Vector3 rotationDelta = Vector3.zero;
        public bool alignProjected = true;
        public string pathToPalette = string.Empty;

        public List<GameObject> assetsPalette = new();
        public bool[] contentSelection;
    }
}