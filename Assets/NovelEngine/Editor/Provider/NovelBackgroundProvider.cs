using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace novel
{
    public class NovelBackgroundProvider : SettingsProvider
    {
        private SerializedObject novelSettings;

        private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Background);
        public NovelBackgroundProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelBackgroundData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelSettings == null)
                novelSettings = NovelEditorUtils.GetSerializedSettings<NovelBackgroundData>(path);

            novelSettings.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Background List");
            var bgProp = novelSettings.FindProperty("novelBackgroundDict").FindPropertyRelative("pairs");

            EditorGUI.indentLevel++;


            for (int i = 0; i < bgProp.arraySize; i++)
            {
                var element = bgProp.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative("_key");
                var valueProp = element.FindPropertyRelative("value");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(keyProp, GUIContent.none);
                EditorGUILayout.PropertyField(valueProp, GUIContent.none);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(18)))
            {
                bgProp.arraySize++;
                var elem = bgProp.GetArrayElementAtIndex(bgProp.arraySize - 1);
                elem.FindPropertyRelative("_key").stringValue = ""; // 초기화(권장)
                elem.FindPropertyRelative("value").objectReferenceValue = null;
            }
            if (GUILayout.Button("-", GUILayout.Width(18)))
            {
                bgProp.DeleteArrayElementAtIndex(bgProp.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                novelSettings.ApplyModifiedProperties();                  // Undo 지원 버전 권장
                EditorUtility.SetDirty(novelSettings.targetObject);       // Dirty 마킹
                AssetDatabase.SaveAssets();                               // 디스크에 즉시 flush
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelBackgroundProvider("Project/Novel/Backgrounds", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
            };
        }
    }

}
