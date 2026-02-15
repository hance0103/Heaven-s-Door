using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace novel
{

    class NovelAudioProvider : SettingsProvider
    {
        private SerializedObject novelSettings;
        private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Audio);
        public NovelAudioProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelAudioData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelSettings == null)
                novelSettings = NovelEditorUtils.GetSerializedSettings<NovelAudioData>(path);

            novelSettings.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Audio Resources", EditorStyles.boldLabel);


            EditorGUILayout.LabelField("Audio Mixer");
            var mixerProp = novelSettings.FindProperty("mixer");

            EditorGUILayout.PropertyField(mixerProp, new GUIContent("Mixer"), true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("BGM List");
            var bgmProp = novelSettings.FindProperty("bgmDict").FindPropertyRelative("pairs");

            EditorGUI.indentLevel++;

            for (int i = 0; i < bgmProp.arraySize; i++)
            {
                var element = bgmProp.GetArrayElementAtIndex(i);

                
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
                bgmProp.arraySize++;
                var elem = bgmProp.GetArrayElementAtIndex(bgmProp.arraySize - 1);
                elem.FindPropertyRelative("_key").stringValue = ""; // 초기화(권장)
                elem.FindPropertyRelative("value").objectReferenceValue = null;
            }
            if (GUILayout.Button("-", GUILayout.Width(18)))
            {
                bgmProp.DeleteArrayElementAtIndex(bgmProp.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();



            // 여기부터 SFX

            EditorGUILayout.LabelField("SFX List");
            var sfxProp = novelSettings.FindProperty("sfxDict").FindPropertyRelative("pairs");

            EditorGUI.indentLevel++;


            for (int i = 0; i < sfxProp.arraySize; i++)
            {
                var element = sfxProp.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative("key");
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
                sfxProp.arraySize++;
                var elem = sfxProp.GetArrayElementAtIndex(sfxProp.arraySize - 1);
                elem.FindPropertyRelative("key").stringValue = "";
                elem.FindPropertyRelative("value").objectReferenceValue = null;
            }
            if (GUILayout.Button("-", GUILayout.Width(18)))
            {
                sfxProp.DeleteArrayElementAtIndex(sfxProp.arraySize - 1);
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
            return new NovelAudioProvider("Project/Novel/Audio", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Audio" })
            };
        }
    }
}
