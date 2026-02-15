using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using novel;
public class NovelScriptProvider : SettingsProvider
{
    SerializedObject novelScript;
    private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Script);

    public NovelScriptProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

    public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
    {
        novelScript = NovelEditorUtils.GetSerializedSettings<NovelScriptData>(path);
    }

    public override void OnGUI(string searchContext)
    {
        if (novelScript == null)
            novelScript = NovelEditorUtils.GetSerializedSettings<NovelScriptData>(path);

        novelScript.Update();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Script List");
        var scriptProp = novelScript.FindProperty("scriptList").FindPropertyRelative("pairs");

        EditorGUI.indentLevel++;


        for (int i = 0; i < scriptProp.arraySize; i++)
        {
            var element = scriptProp.GetArrayElementAtIndex(i);
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
            scriptProp.arraySize++;
            var elem = scriptProp.GetArrayElementAtIndex(scriptProp.arraySize - 1);
            elem.FindPropertyRelative("_key").stringValue = "";
            elem.FindPropertyRelative("value").objectReferenceValue = null;
        }
        if (GUILayout.Button("-", GUILayout.Width(18)))
        {
            scriptProp.DeleteArrayElementAtIndex(scriptProp.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            novelScript.ApplyModifiedProperties();                  // Undo 지원 버전 권장
            EditorUtility.SetDirty(novelScript.targetObject);       // Dirty 마킹
            AssetDatabase.SaveAssets();                               // 디스크에 즉시 flush
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new NovelScriptProvider("Project/Novel/Script", SettingsScope.Project)
        {
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "Script" })
        };
    }
}
