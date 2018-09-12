#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#  define PRE_UNITY_5
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.DynamicWaterSystem.EditorExtensions {
    public static class EditorGUILayoutExtensions {
        private const float IndentationWidth = 9f;

        //FixedWidthLabel class. Extends IDisposable, so that it can be used with the "using" keyword.
        private class FixedWidthLabel : IDisposable {
            private readonly ZeroIndent _indentReset; // Helper class to reset and restore indentation

            public FixedWidthLabel(GUIContent label) {
                float indentation = IndentationWidth * EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentation);
                float width = Mathf.Max(EditorGUIUtility.labelWidth - indentation, GUI.skin.label.CalcSize(label).x);
                GUILayout.Label(label, GUILayout.Width(width));

                _indentReset = new ZeroIndent();
            }

            public FixedWidthLabel(string label)
                : this(new GUIContent(label)) {
            }

            public void Dispose() {
                _indentReset.Dispose();
                EditorGUILayout.EndHorizontal();
            }
        }

        private class ZeroIndent : IDisposable //helper class to clear indentation
        {
            private readonly int _originalIndent; //the original indentation value before we change the GUI state

            public ZeroIndent() {
                _originalIndent = EditorGUI.indentLevel; //save original indentation
                EditorGUI.indentLevel = 0; //clear indentation
            }

            public void Dispose() {
                EditorGUI.indentLevel = _originalIndent; //restore original indentation
            }
        }

        public static bool ToggleFixedWidth(GUIContent label, bool value) {
            using (new FixedWidthLabel(label)) {
                Rect controlRect =
                    EditorGUILayoutInternal.GetControlRect(
                        false,
                        16f,
                        EditorStyles.toggle,
                        null
                        );

                value =
                    GUI.Toggle(
                        controlRect,
                        value,
                        ""
                        );

                return value;
            }
        }

        public static bool ToggleFixedWidth(string label, bool value) {
            return ToggleFixedWidth(new GUIContent(label), value);
        }

        public static int IntSliderFixedWidth(GUIContent label, int value, int leftValue, int rightValue) {
            using (new FixedWidthLabel(label)) {
                value =
                    EditorGUI.IntSlider(
                        EditorGUILayoutInternal.GetControlRect(
                            false,
                            16f,
                            EditorStyles.toggle,
                            null
                            ),
                        "",
                        value,
                        leftValue,
                        rightValue
                        );

                return value;
            }
        }

        public static int IntSliderFixedWidth(string label, int value, int leftValue, int rightValue) {
            return IntSliderFixedWidth(new GUIContent(label), value, leftValue, rightValue);
        }

        private static List<string> layers;
        private static List<int> layerNumbers;
        private static string[] layerNames;
        private static long lastUpdateTick;

        public static LayerMask LayerMaskField(GUIContent content, LayerMask selected, bool showSpecial) {
            if (layers == null ||
                (DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout)) {
                lastUpdateTick = DateTime.Now.Ticks;
                if (layers == null) {
                    layers = new List<string>();
                    layerNumbers = new List<int>();
                    layerNames = new string[4];
                }
                else {
                    layers.Clear();
                    layerNumbers.Clear();
                }

                int emptyLayers = 0;
                for (int i = 0; i < 32; i++) {
                    string layerName = LayerMask.LayerToName(i);

                    if (layerName != "") {
                        for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                        layerNumbers.Add(i);
                        layers.Add(layerName);
                    }
                    else {
                        emptyLayers++;
                    }
                }

                if (layerNames.Length != layers.Count) {
                    layerNames = new string[layers.Count];
                }
                for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];
            }

            selected.value = EditorGUILayout.MaskField(content, selected.value, layerNames);

            return selected;
        }
    }

    public static class EditorGUIInternal {
        public const float kNumberW = 40f;
    }

    public static class EditorStylesInternal {
        public static GUIStyle helpBox {
            get {
                Type type = typeof (EditorStyles);
                PropertyInfo info = type.GetProperty("helpBox", BindingFlags.Static | BindingFlags.NonPublic);
                if (info != null) {
                    object value = info.GetValue(type, null);
                    return (GUIStyle) value;
                }

                return EditorStyles.label;
            }
        }
    }

    public static class EditorGUILayoutInternal {
        public const float kLabelFloatMinW = 80f + EditorGUIInternal.kNumberW + 5f;
        public const float kLabelFloatMaxW = 80f + EditorGUIInternal.kNumberW + 5f;
        public const float kPlatformTabWidth = 30f;

        public static Rect GetControlRect(bool hasLabel, float height, GUIStyle style, params GUILayoutOption[] options) {
            Rect rect = 
                GUILayoutUtility.GetRect(
                    !hasLabel ? EditorGUIInternal.kNumberW : kLabelFloatMinW,
                    kLabelFloatMaxW, 
                    height, 
                    height, 
                    style, 
                    options
                );

            return rect;
        }
    }

    public static class TagManager {
        private const string kTagManagerPath = "ProjectSettings/TagManager.asset";
        private static SerializedObject _tagManager;
        private static SerializedProperty _tags;
#if !PRE_UNITY_5
        private static SerializedProperty _layers;
#else
        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
#endif

        static TagManager() {
            UpdateManager();
        }

        public static bool IsTagExists(string tag) {
            UpdateManager();

            IEnumerator it = _tags.GetEnumerator();

            while (it.MoveNext()) {
                SerializedProperty prop = it.Current as SerializedProperty;
                if (prop == null || prop.type != "string")
                    continue;

                if (prop.stringValue == tag)
                    return true;
            }

            return false;
        }

        public static void AddTag(string tag) {
            UpdateManager();

            if (IsTagExists(tag))
                return;

            int newIndex = _tags.arraySize > 0 ? _tags.arraySize - 1 : 0;
            _tags.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newTag = _tags.GetArrayElementAtIndex(newIndex);
            newTag.stringValue = tag.Trim();

            SaveManager();
        }

        public static string GetLayer(int layerNumber) {
            UpdateManager();

            SerializedProperty layer = GetLayerProperty(layerNumber);
            return layer != null ? layer.stringValue : null;
        }

        public static bool SetLayer(int layerNumber, string layerName) {
            UpdateManager();

            SerializedProperty layer = GetLayerProperty(layerNumber);
            if (layer == null)
                return false;

            layer.stringValue = layerName.Trim();
            SaveManager();

            return true;
        }

        public static bool IsLayerExists(string layerName) {
            for (int i = 7; i <= 31; i++) {
                if (GetLayer(i) == layerName) {
                    return true;
                }
            }

            return false;
        }

        public static int GetFreeLayer(LayerSearchDirection searchDirection) {
            if (searchDirection == LayerSearchDirection.FirstToLast) {
                for (int i = 7; i <= 31; i++) {
                    if (GetLayer(i) == "") {
                        return i;
                    }
                }
            } else {
                for (int i = 31; i >= 7; i--) {
                    if (GetLayer(i) == "") {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static void AddLayer(string layerName,
            LayerSearchDirection searchDirection = LayerSearchDirection.LastToFirst) {
            if (IsLayerExists(layerName))
                return;

            int freeLayer = GetFreeLayer(searchDirection);
            SetLayer(freeLayer, layerName);
        }

        private static void UpdateManager() {
            _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(kTagManagerPath)[0]);
            _tags = _tagManager.FindProperty("tags");
#if !PRE_UNITY_5
            _layers = _tagManager.FindProperty("layers");
#endif
        }

        private static void SaveManager() {
            _tagManager.UpdateIfDirtyOrScript();
            _tagManager.ApplyModifiedProperties();
        }

        private static SerializedProperty GetLayerProperty(int layerNumber) {
#if !PRE_UNITY_5
            return _layers.GetArrayElementAtIndex(layerNumber);
#else
            return _tagManager.FindProperty("User Layer " + layerNumber.ToString(_invariantCulture));
#endif
        }

        public enum LayerSearchDirection {
            LastToFirst,
            FirstToLast,
        }
    }

    public static class TextureImporterHelper {
        public static void SetTextureIsReadable(Texture2D texture, bool readable) {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null) {
                textureImporter.isReadable = readable;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        public static bool GetTextureIsReadable(Texture2D texture) {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            return textureImporter == null || textureImporter.isReadable;
        }
    }

    public static class AssetDatabaseHelper {
        private class Folders {
            public string Source { get; private set; }
            public string Target { get; private set; }

            public Folders(string source, string target) {
                Source = source;
                Target = target;
            }
        }

        private static void MoveDirectoryEx(string source, string target) {
            Stack<Folders> stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0) {
                Folders folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (string file in Directory.GetFiles(folders.Source, "*.*")) {
                    string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }

                foreach (string folder in Directory.GetDirectories(folders.Source)) {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
            Directory.Delete(source, true);
        }

        public static void MoveAssetDirectory(string sourceDir, string destDir) {
            destDir = Path.GetFullPath(destDir);
            sourceDir = Path.GetFullPath(sourceDir);

            DirectoryInfo sourceInfo = new DirectoryInfo(sourceDir);
            DirectoryInfo destInfo = new DirectoryInfo(destDir);

            if (Directory.Exists(destDir) && Directory.GetFiles(destDir).Length == 0)
                Directory.Delete(destDir);

            Directory.CreateDirectory(destInfo.Parent.ToString());

            MoveDirectoryEx(sourceDir, destDir);

            string slash = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

            string sourceMeta = sourceInfo.Parent + slash + sourceInfo.Name + ".meta";
            string destMeta = destInfo.Parent + slash + destInfo.Name + ".meta";

            if (File.Exists(destMeta))
                File.Delete(destMeta);

            File.Move(sourceMeta, destMeta);
        }
    }
}