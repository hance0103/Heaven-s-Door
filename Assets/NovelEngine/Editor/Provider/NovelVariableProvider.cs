using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace novel
{
    class NovelVariableProvider : SettingsProvider
    {
        private SerializedObject novelSettings;
        private readonly string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Variable);
        private List<Type> parameterTypes;
        private string[] typeNames;
        private readonly bool showParameter = false;

        private NovelVariableProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelVariableData>(path);
            if (parameterTypes == null)
            {
                Type baseType = typeof(NovelParameter);
                parameterTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                    .OrderBy(t =>t.GetCustomAttribute<OrderAttribute>()?.Index ?? int.MaxValue)
                    .ThenBy(t => t.Name)
                    .ToList();

            }
            typeNames = parameterTypes.Select(GetAlias).ToArray();
            EditorGUIUtility.FindTexture("d_UnityEditor.AnimationWindow");
        }
        private static string GetAlias(Type type)
        {
            var attr = type.GetCustomAttribute<NovelParameterAliasAttribute>();
            return attr != null ? attr.Alias : type.Name;
        }
        public override void OnGUI(string searchContext)
        {
            novelSettings ??= NovelEditorUtils.GetSerializedSettings<NovelVariableData>(path);


            novelSettings.Update();

            EditorGUI.BeginChangeCheck();


            // 변수 리스트만 표시
            if (!showParameter)
            {
                EditorGUILayout.LabelField("Variable List");

                var dictProp = novelSettings.FindProperty("novelVariableDict");
                if (dictProp == null)
                {
                    EditorGUILayout.HelpBox("필드 'novelVariableDict'를 찾을 수 없습니다.", MessageType.Error);
                    return;
                }

                var pairsProp = dictProp.FindPropertyRelative("pairs");
                if (pairsProp == null || !pairsProp.isArray)
                {
                    EditorGUILayout.HelpBox("'novelVariableDict.pairs' 배열을 찾을 수 없습니다.", MessageType.Error);
                    return;
                }

                for (int i = 0; i < pairsProp.arraySize; i++)
                {
                    var element = pairsProp.GetArrayElementAtIndex(i);
                    if (element == null) continue;

                    var keyProp = element.FindPropertyRelative("_key");
                    var valueProp = element.FindPropertyRelative("value");
                    if (keyProp == null || valueProp == null) continue;

                    var paramProp = valueProp.FindPropertyRelative("parameter");
                    if (paramProp == null)
                    {
                        // NovelVariable에 parameter가 없으면 구조가 다른 것. 에러 표기
                        EditorGUILayout.HelpBox($"[{i}] value.parameter 경로를 찾을 수 없습니다.", MessageType.Error);
                        continue;
                    }

                    int currentParameterIndex = 0;
                    if (paramProp.managedReferenceValue is NovelParameter currentParam)
                    {
                        int index = parameterTypes.FindIndex(t => t == currentParam.GetType());
                        if (index >= 0) currentParameterIndex = index;
                    }
                    else
                    {
                        paramProp.managedReferenceValue = Activator.CreateInstance(parameterTypes[0]) as NovelParameter;
                        currentParameterIndex = 0;
                    }


                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Key", GUILayout.Width(40));
                        keyProp.stringValue = EditorGUILayout.TextField(keyProp.stringValue);

                        EditorGUILayout.LabelField("Type", GUILayout.Width(40));
                        int selected = EditorGUILayout.Popup(currentParameterIndex, typeNames, GUILayout.Width(180));
                        if (selected != currentParameterIndex)
                        {
                            // SerializeReference 교체는 parameter에만!
                            var newType = parameterTypes[selected];
                            paramProp.managedReferenceValue =
                                Activator.CreateInstance(newType) as NovelParameter;
                        }
                    }


                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(paramProp, includeChildren: true);
                    EditorGUI.indentLevel--;

                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(18)))
                    {
                        pairsProp.arraySize++;
                        var newElem = pairsProp.GetArrayElementAtIndex(pairsProp.arraySize - 1);
                        var newKey = newElem.FindPropertyRelative("_key");
                        var newValue = newElem.FindPropertyRelative("value");
                        var newParam = newValue?.FindPropertyRelative("parameter");

                        if (newKey != null) newKey.stringValue = string.Empty;

                        if (newParam is { managedReferenceValue: null })
                        {
                            newParam.managedReferenceValue =
                                Activator.CreateInstance(parameterTypes[0]) as NovelParameter;
                        }

                    }
                    if (GUILayout.Button("-", GUILayout.Width(18)))
                    {
                        if (pairsProp.arraySize > 0)
                        {
                            pairsProp.DeleteArrayElementAtIndex(pairsProp.arraySize - 1);
                            novelSettings.ApplyModifiedProperties();
                        }
                            
                    }
                }
            }
            // 세부 파라미터 표시
            else
            {

            }

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
            return new NovelVariableProvider("Project/Novel/Variable")
            {
                keywords = new HashSet<string>(new[] { "Number" })
            };
        }
    }

}
