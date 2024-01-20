using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using AssetsDrawer.Data;

namespace AssetsDrawer
{
    public class AssetsDrawerWindow : EditorWindow
    {
        [SerializeField] private DrawerProperties properties = new();
        [SerializeField] private DrawerSettings settings = new();

        private AssetsDrawerHandler _drawerHandler = new();

        private SerializedObject _serializedObject;
        private bool _drawerActiveState;
        private Vector2 _paletteScrollPosition = Vector2.zero;
        private Vector2 _mainScrollPosition = Vector2.zero;

        [MenuItem("Tools/Assets Drawer")]
        private static void ShowWindow()
        {
            GetWindow(typeof(AssetsDrawerWindow), false, "Assets Drawer");
        }

        private void OnEnable()
        {
            _drawerHandler.Initialize(properties, settings);
        }

        private void OnDisable()
        {
            _drawerHandler.DeInitialize();
        }


        private void OnFocus()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            _drawerHandler.ProjectionHandler?.DestroyProjection();
        }

        private void Update()
        {
            _drawerHandler.UpdateLogic();
            if (!_drawerActiveState || !settings.showProjection)
                _drawerHandler.ProjectionHandler?.DestroyProjection();
        }

        private void OnLostFocus()
        {
            _drawerHandler.ProjectionHandler?.DestroyProjection();
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _drawerHandler.ProjectionHandler?.DestroyProjection();
        }

        private void OnGUI()
        {
            DrawEditorGUI();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_drawerActiveState)
            {
                _drawerHandler?.HandleMousePosition(Event.current.mousePosition);
                _drawerHandler?.HandleSceneViewInputs(Event.current);
                BlockObjectSelection();
                Repaint();
                sceneView.Repaint();
            }
        }

        private void BlockObjectSelection()
        {
            var controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(controlID);
        }

        private void DrawEditorGUI()
        {
            _serializedObject = new SerializedObject(this);

            DrawActivateButtonGUI();
            _mainScrollPosition = GUILayout.BeginScrollView(_mainScrollPosition, false, false);
            DrawAssetsMasksGUI();
            DrawSpacingControlsGUI();
            DrawScaleControlsGUI();
            DrawSpawnControlsGUI();
            DrawTransformControlsGUI();
            DrawEditorSettingsGUI();
            EditorGUILayout.Space();
            DrawShortKeysHelpBoxGUI();
            DrawSelectedObjectsControlsGUI();
            EditorGUILayout.Space();
            DrawPalettePropertiesGUI();
            DrawPaletteGUI();
            GUILayout.EndScrollView();

            _serializedObject.ApplyModifiedProperties();
        }

        private void DrawActivateButtonGUI()
        {
            EditorGUILayout.HelpBox("Press to activate/deactivate draw mode", MessageType.Info);
            _drawerActiveState = GUILayout.Toggle(_drawerActiveState, new GUIContent("Draw", "Activates draw mode"),
                "Button", GUILayout.Height(30f), GUILayout.Width(200f));
        }

        private void DrawAssetsMasksGUI()
        {
            EditorGUILayout.PropertyField(
                _serializedObject.FindProperty(nameof(properties))
                    .FindPropertyRelative(nameof(properties.surfaceLayerMask)),
                new GUIContent("Surface layers", "Layers that the brush will draw on"));
            EditorGUILayout.PropertyField(
                _serializedObject.FindProperty(nameof(properties))
                    .FindPropertyRelative(nameof(properties.assetsLayerMask)),
                new GUIContent("Assets layers", "Layers of assets that can be erased"));
            properties.assetsTag = EditorGUILayout.TagField(new GUIContent("Assets tag",
                    "Tag of assets that can be erased when no assets layer is set" +
                    "\nTip: Useful if assets that need to be erased don't have any colliders, therefore layer mask can't be used" +
                    "\nWarning: Pretty expensive invocation"),
                properties.assetsTag);
        }

        private void DrawSpacingControlsGUI()
        {
            properties.spread = EditorGUILayout.FloatField(
                new GUIContent("Draw spread", "Min distance from the previous spawn point while dragging"),
                properties.spread);
            properties.circleRadius = EditorGUILayout.Slider(
                new GUIContent("Brush radius", "Effective radius of the brush"),
                properties.circleRadius,
                0.1f, 100f);
        }

        private void DrawScaleControlsGUI()
        {
            var cachedRandScale = properties.randomizeScale;
            properties.randomizeScale = EditorGUILayout.ToggleLeft(
                new GUIContent("Randomize scale",
                    "Allows to randomize the scale within a given range"),
                properties.randomizeScale);
            if (cachedRandScale != properties.randomizeScale)
                _drawerHandler.PropertySetter.UpdateSpawnScaleMod();

            if (properties.randomizeScale)
            {
                var cachedMin = properties.minRandomScale;
                var cachedMax = properties.maxRandomScale;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                properties.minRandomScale =
                    EditorGUILayout.FloatField(properties.minRandomScale, GUILayout.MaxWidth(100));
                EditorGUILayout.MinMaxSlider(ref properties.minRandomScale, ref properties.maxRandomScale, 0.0001f,
                    100f);
                properties.maxRandomScale =
                    EditorGUILayout.FloatField(properties.maxRandomScale, GUILayout.MaxWidth(100));
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
                if (Math.Abs(cachedMin - properties.minRandomScale) > 0.0001f ||
                    Math.Abs(cachedMax - properties.maxRandomScale) > 0.0001f)
                    _drawerHandler.PropertySetter.UpdateSpawnScaleMod();
            }
            else
            {
                properties.scaleMultiplier =
                    EditorGUILayout.Slider(
                        new GUIContent("Scale multiplier", "Multiplies original scale of the object"),
                        properties.scaleMultiplier, 0.1f, 100f);
                _drawerHandler.PropertySetter.UpdateSpawnScaleMod();
            }
        }

        private void DrawSpawnControlsGUI()
        {
            properties.spawnAmountPerClick =
                EditorGUILayout.IntField(
                    new GUIContent("Amount to spawn",
                        "Max amount of created objects at one draw iteration" +
                        "\nThose created on top of each other are removed if possible"),
                    properties.spawnAmountPerClick);

            properties.avoidOverlapping =
                EditorGUILayout.ToggleLeft(
                    new GUIContent("Avoid overlapping",
                        "Attempts to avoid overlapping spawned objects based on layers and tag"),
                    properties.avoidOverlapping);

            properties.alignToNormal =
                EditorGUILayout.ToggleLeft(
                    new GUIContent("Align to normal",
                        "Aligns new object's rotation to normal vector at pointer's hit point"),
                    properties.alignToNormal);
        }

        private void DrawTransformControlsGUI()
        {
            var cachedRandomizeRotation = properties.randomizeRotation;
            properties.randomizeRotation =
                EditorGUILayout.ToggleLeft(
                    new GUIContent("Randomize rotation", "Randomizes new object's rotation"),
                    properties.randomizeRotation);
            if (cachedRandomizeRotation != properties.randomizeRotation)
                _drawerHandler.PropertySetter.UpdateSpawnRotation();

            if (!properties.randomizeRotation)
            {
                var cachedRotationDelta = properties.rotationDelta;
                var newDelta = properties.rotationDelta;
                if (settings.normalizeRotationDelta)
                    newDelta = new Vector3(properties.rotationDelta.x % 360, properties.rotationDelta.y % 360,
                        properties.rotationDelta.z % 360);

                properties.rotationDelta = EditorGUILayout.Vector3Field("Rotation delta", newDelta);
                if (cachedRotationDelta != properties.rotationDelta)
                    _drawerHandler.PropertySetter.UpdateSpawnRotation();
            }

            properties.positionDelta = EditorGUILayout.Vector3Field("Position delta", properties.positionDelta);
            properties.setParent = EditorGUILayout.ToggleLeft(
                new GUIContent("Set parent object",
                    "Sets selected object in Hierarchy Window as parent for new objects if there is one"),
                properties.setParent);
        }

        private void DrawEditorSettingsGUI()
        {
            settings.foldGroup1State = EditorGUILayout.Foldout(settings.foldGroup1State, "Drawer's settings", true);
            if (settings.foldGroup1State)
            {
                EditorGUI.indentLevel++;
                settings.rotationMaxSpeed =
                    EditorGUILayout.Slider(new GUIContent("Rotation speed", "Max rotation speed"),
                        settings.rotationMaxSpeed, 0.01f, 180f);
                settings.handlesThickness =
                    EditorGUILayout.Slider(new GUIContent("Handles thickness", "Visual thickness of handles in scene"),
                        settings.handlesThickness, 0f, 10f);
                settings.showProjection = EditorGUILayout.ToggleLeft(
                    new GUIContent("Show projection", "Shows projection of the next object to spawn"),
                    settings.showProjection);
                settings.showHandlesNormal = EditorGUILayout.ToggleLeft(
                    new GUIContent("Show normal", "Shows normal vector of the surface at pointer's hit point"),
                    settings.showHandlesNormal);
                settings.normalizeRotationDelta = EditorGUILayout.ToggleLeft(
                    new GUIContent("Normalize rotation delta", "Normalizes rotation delta in the range [0:360]"),
                    settings.normalizeRotationDelta);
                settings.handlesColor = EditorGUILayout.ColorField("Draw handles color", settings.handlesColor);
                settings.erasureHandlesColor =
                    EditorGUILayout.ColorField("Erasure handles color", settings.erasureHandlesColor);
                settings.idleHandlesColor = EditorGUILayout.ColorField("Idle handles color", settings.idleHandlesColor);
                settings.maxElementsInLine = EditorGUILayout.IntSlider(new GUIContent("Max toggles in line",
                    "Max toggles in palette's line"), settings.maxElementsInLine, 1, 10);

                EditorGUILayout.LabelField("Toggle size");
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                settings.minToggleSize = EditorGUILayout.FloatField(settings.minToggleSize, GUILayout.MaxWidth(100));
                EditorGUILayout.MinMaxSlider(ref settings.minToggleSize, ref settings.maxToggleSize, 5, 300);
                settings.maxToggleSize = EditorGUILayout.FloatField(settings.maxToggleSize, GUILayout.MaxWidth(100));
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawShortKeysHelpBoxGUI()
        {
            EditorGUILayout.HelpBox("Erasure brush: Shift+LMB\n" +
                                    "Change rotation: Ctrl+X/Ctrl+C\n" +
                                    "Undo/Redo changes: Ctrl+Z/Ctrl+Y", MessageType.Info);
        }

        private void DrawPalettePropertiesGUI()
        {
            properties.pathToPalette =
                EditorGUILayout.TextField(
                    new GUIContent("Persistent assets path",
                        "Sets a persistent path for assets. Current Project Window's directory is used if string is empty or doesn't exist" +
                        "\nExample: Assets/Resources/Palette" +
                        "\nTip: You can use 'Copy Path' in the context menu of the Project Window and then just paste it here"),
                    properties.pathToPalette);
        }

        private void DrawSelectedObjectsControlsGUI()
        {
            properties.alignProjected = EditorGUILayout.ToggleLeft(
                new GUIContent("Align when projected",
                    "Aligns objects rotation to the normal vector if set to true when they are projected onto the surface"),
                properties.alignProjected);

            if (GUILayout.Button(new GUIContent("Project selected onto the surface",
                    "Moves objects selected in the Hierarchy onto the surface")))
            {
                _drawerHandler.ProjectSelectedOntoSurface();
            }
        }

        private void DrawPaletteGUI()
        {
            var contents = new GUIContent[properties.assetsPalette.Count];
            for (var i = 0; i < contents.Length; i++)
                contents[i] = new GUIContent(AssetPreview.GetAssetPreview(properties.assetsPalette[i]),
                    properties.assetsPalette[i].name);

            EditorGUILayout.BeginVertical(GUILayout.MinHeight(150), GUILayout.MaxHeight(600));
            _paletteScrollPosition = GUILayout.BeginScrollView(_paletteScrollPosition, false, false);

            for (var i = 0; i < contents.Length; i++)
            {
                if (IsFirstInLine(i))
                    EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(settings.minToggleSize),
                    GUILayout.MaxHeight(settings.maxToggleSize),
                    GUILayout.MinWidth(settings.minToggleSize), GUILayout.MaxWidth(settings.maxToggleSize));
                var state = GUILayout.Toggle(properties.contentSelection[i], contents[i],
                    GUILayout.MinHeight(settings.minToggleSize),
                    GUILayout.MaxHeight(settings.maxToggleSize),
                    GUILayout.MinWidth(settings.minToggleSize), GUILayout.MaxWidth(settings.maxToggleSize));
                EditorGUILayout.LabelField(contents[i].tooltip, GUILayout.MinWidth(settings.minToggleSize),
                    GUILayout.MaxWidth(settings.maxToggleSize));
                EditorGUILayout.EndVertical();

                if (IsLastInLine(i) || DrawerUtility.IsLastIndex(i, contents.Length))
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }

                _drawerHandler.UpdateSelectionState(i, state);
            }

            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            bool IsFirstInLine(int index) =>
                index % settings.maxElementsInLine == 0;

            bool IsLastInLine(int index) =>
                index % settings.maxElementsInLine == settings.maxElementsInLine - 1;
        }
    }
}
#endif