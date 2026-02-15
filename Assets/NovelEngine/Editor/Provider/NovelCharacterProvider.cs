using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;


namespace novel
{
    public class NovelCharacterProvider : SettingsProvider
    {
        SerializedObject novelChar;
        private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Character);

        private bool showCharacter = false;
        //private string currentCharacter = "";
        private NovelCharacterSO currentCharacterData = null;

        Texture icon;

        public NovelCharacterProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);
            novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);
            icon = EditorGUIUtility.FindTexture("d_UnityEditor.AnimationWindow");
        }

        public override void OnGUI(string searchContext)
        {
            if (novelChar == null)
                novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);

            novelChar.Update();
            EditorGUI.BeginChangeCheck();

            if (!showCharacter)
            {
                EditorGUILayout.LabelField("Character List");
                var charProp = novelChar.FindProperty("charDict").FindPropertyRelative("pairs");
                EditorGUI.indentLevel++;
                for (int i = 0; i < charProp.arraySize; i++)
                {
                    var element = charProp.GetArrayElementAtIndex(i);
                    var keyProp = element.FindPropertyRelative("_key");
                    var valueProp = element.FindPropertyRelative("value");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(5);



                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(keyProp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (valueProp.objectReferenceValue != null)
                        {
                            var charSO = valueProp.objectReferenceValue as NovelCharacterSO;

                            if (charSO != null && charSO.characterName != keyProp.stringValue)
                            {
                                Undo.RecordObject(charSO, "Rename Character");
                                charSO.characterName = keyProp.stringValue;



                                EditorUtility.SetDirty(charSO);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }

                    if (GUILayout.Button(new GUIContent(icon), GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        // 클릭시 해당 캐릭터 상세보기
                        if (valueProp != null)
                        {
                            currentCharacterData = valueProp.objectReferenceValue as NovelCharacterSO;

                            if (currentCharacterData == null)
                            {
                                string assetPath = $"Assets/NovelEngine/Addressable/CharacterData/{keyProp.stringValue}.asset";
                                var existingAsset = AssetDatabase.LoadAssetAtPath<NovelCharacterSO>(assetPath);
                                if (existingAsset != null)
                                {
                                    currentCharacterData = existingAsset;
                                    valueProp.objectReferenceValue = currentCharacterData;
                                    novelChar.ApplyModifiedPropertiesWithoutUndo();
                                    showCharacter = true;
                                    //currentCharacter = keyProp.stringValue;
                                    return;
                                }
                                else
                                {
                                    // 새로운 캐릭터 생성
                                    var newChar = ScriptableObject.CreateInstance<NovelCharacterSO>();
                                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                                    AssetDatabase.CreateAsset(newChar, assetPath);
                                    AssetDatabase.SaveAssets();

                                    currentCharacterData = newChar;
                                    valueProp.objectReferenceValue = currentCharacterData;
                                    novelChar.ApplyModifiedPropertiesWithoutUndo();
                                    showCharacter = true;
                                    //currentCharacter = keyProp.stringValue;
                                    return;
                                }
                            }
                            else
                            {
                                showCharacter = true;
                            }

                        }
                        else
                        {
                            Debug.LogError("valueProp is null");
                        }


                            
                    }
                    GUILayout.Space(5);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);

                if (GUILayout.Button("Add New Character"))
                {
                    string newCharName = "NewCharacter";
                    int suffix = 1;

                    bool keyExists;
                    do
                    {
                        keyExists = false;
                        for (int i = 0; i < charProp.arraySize; i++)
                        {
                            var element = charProp.GetArrayElementAtIndex(i);
                            var keyProp = element.FindPropertyRelative("_key");

                            if (keyProp.stringValue == newCharName)
                            {
                                keyExists = true;
                                newCharName = $"NewCharacter{suffix}";
                                suffix++;
                                break;
                            }
                        }
                        if (keyExists)
                        {
                            newCharName = $"NewCharacter{suffix}";
                        }
                    }while (keyExists);

                    charProp.arraySize++;
                    var newElement = charProp.GetArrayElementAtIndex(charProp.arraySize - 1);
                    newElement.FindPropertyRelative("_key").stringValue = newCharName;

                    string assetPath = $"Assets/NovelEngine/Addressable/CharacterData/{newCharName}.asset";
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    var newCharSO = ScriptableObject.CreateInstance<NovelCharacterSO>();

                    // 캐릭터 이름 초기화
                    newCharSO.characterName = newCharName;

                    AssetDatabase.CreateAsset(newCharSO, assetPath);
                    AssetDatabase.SaveAssets();

                    newElement.FindPropertyRelative("value").objectReferenceValue = newCharSO;

                    novelChar.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(novelChar.targetObject);
                    AssetDatabase.SaveAssets();
                }

                GUILayout.Space(5);

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Back To Character List", GUILayout.Height(25)))
                {
                    showCharacter = false;
                    //currentCharacter = "";
                    currentCharacterData = null;
                    return;
                }

                // 캐릭터 이름
                GUIStyle boldLargeLabel = new GUIStyle(EditorStyles.label);
                boldLargeLabel.fontSize = 20;                 // 글자 크기
                boldLargeLabel.fontStyle = FontStyle.Bold;    // 볼드체
                boldLargeLabel.normal.textColor = Color.white; // 글자색 (옵션)
                GUILayout.Space(10);
                EditorGUILayout.LabelField(currentCharacterData.characterName, boldLargeLabel);
                GUILayout.Space(10);
                var beforeName = currentCharacterData.novelName;
                currentCharacterData.novelName = EditorGUILayout.TextField("Display Name", currentCharacterData.novelName);
                if (beforeName != currentCharacterData.novelName) EditorUtility.SetDirty(currentCharacterData);

                var beforeBody = currentCharacterData.body;
                currentCharacterData.body = (Sprite)EditorGUILayout.ObjectField("Body Sprite", currentCharacterData.body, typeof(Sprite), false);
                if (beforeBody != currentCharacterData.body) EditorUtility.SetDirty(currentCharacterData);

                var beforeHead = currentCharacterData.headOffset;
                currentCharacterData.headOffset = EditorGUILayout.Vector2Field("Head Offset", currentCharacterData.headOffset);
                if (beforeHead != currentCharacterData.headOffset) EditorUtility.SetDirty(currentCharacterData);

                var beforeFace = currentCharacterData.faceDict;
                currentCharacterData.faceDict = SerializableDictDrawer.DrawSerializableDict(currentCharacterData, currentCharacterData.faceDict, "Face Expressions");
                if (beforeFace != currentCharacterData.faceDict) EditorUtility.SetDirty(currentCharacterData);

                // 캐릭터 삭제
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                Vector2 buttonSize = buttonStyle.CalcSize(new GUIContent("Delete Character"));
                if (GUILayout.Button("Delete Character", buttonStyle, GUILayout.Width(buttonSize.x), GUILayout.Height(25)))
                {
                    var charDictProp = novelChar.FindProperty("charDict").FindPropertyRelative("pairs");

                    for (int i = 0; i < charDictProp.arraySize; i++)
                    {
                        var element = charDictProp.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("_key");
                        var valueProp = element.FindPropertyRelative("value");

                        if (valueProp.objectReferenceValue == currentCharacterData)
                        {
                            // 에셋 삭제
                            var charSO = valueProp.objectReferenceValue as NovelCharacterSO;    // 현재 참조된 ScriptableObject

                            if (charSO != null)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(charSO);
                                if (string.IsNullOrEmpty(assetPath))
                                {
                                    Debug.LogError("Failed to get asset path for deletion.");
                                    return;
                                }
                                else
                                {
                                    AssetDatabase.DeleteAsset(assetPath);
                                    AssetDatabase.SaveAssets();
                                }
                            }

                            // 딕셔너리에서 삭제
                            valueProp.objectReferenceValue = null;  // 참조 해제
                            charDictProp.DeleteArrayElementAtIndex(i);  // 요소 삭제

                            novelChar.ApplyModifiedPropertiesWithoutUndo();
                            break;
                        }
                    }
                    showCharacter = false;
                    //currentCharacter = "";
                    currentCharacterData = null;

                }
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                novelChar.ApplyModifiedProperties();                  // Undo 지원 버전 권장
                EditorUtility.SetDirty(novelChar.targetObject);       // Dirty 마킹
                AssetDatabase.SaveAssets();                               // 디스크에 즉시 flush                             // 보통 불필요
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelCharacterProvider("Project/Novel/Character", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Character" })
            };
        }
    }

}
