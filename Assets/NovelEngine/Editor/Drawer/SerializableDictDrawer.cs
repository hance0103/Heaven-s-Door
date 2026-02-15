using NovelEngine.Scripts;
using UnityEngine;
using UnityEditor;

public static class SerializableDictDrawer
{
    public static SerializableDict<TKey, TValue> DrawSerializableDict<TKey, TValue>(
        UnityEngine.Object hostObject,
        SerializableDict<TKey, TValue> dict,
        string label)
    {
        if (dict == null)
            return new SerializableDict<TKey, TValue>();

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        int removeIndex = -1; // 삭제할 인덱스 기록용

        for (int i = 0; i < dict.pairs.Count; i++)
        {
            var pair = dict.pairs[i];
            EditorGUILayout.BeginHorizontal();

            // 키 처리
            if (typeof(TKey) == typeof(string))
            {
                string oldKey = (string)(object)pair.key;
                EditorGUI.BeginChangeCheck();
                string newKey = EditorGUILayout.DelayedTextField(oldKey);

                if (EditorGUI.EndChangeCheck() && newKey != oldKey)
                {
                    // Undo & Dirty & Save
                    Undo.RecordObject(hostObject, "Change Dict Key");

                    dict.ChangeKey((TKey)(object)oldKey, (TKey)(object)newKey);
                    EditorUtility.SetDirty(hostObject);
                    AssetDatabase.SaveAssets();
                }


            }
            else
            {
                //EditorGUILayout.LabelField(pair.key.ToString());
            }

            // 값 처리
            pair.value = DrawValueField(pair.value);

            // 삭제 버튼
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        // 삭제 처리
        if (removeIndex >= 0)
        {
            dict.pairs.RemoveAt(removeIndex);
        }

        // 새 항목 추가
        if (GUILayout.Button("+ Add New"))
        {
            dict.pairs.Add(new SerializableKeyValuePair<TKey, TValue>());
        }

        EditorGUI.indentLevel--;

        return dict;
    }

    private static TValue DrawValueField<TValue>(TValue value)
    {
        object obj = value;

        if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TValue)))
        {
            obj = EditorGUILayout.ObjectField(value as UnityEngine.Object, typeof(TValue), false);
        }
        else if (typeof(TValue) == typeof(string))
        {
            obj = EditorGUILayout.TextField(value as string);
        }
        else if (typeof(TValue) == typeof(int))
        {
            obj = EditorGUILayout.IntField((int)(object)value);
        }
        else if (typeof(TValue) == typeof(float))
        {
            obj = EditorGUILayout.FloatField((float)(object)value);
        }
        else if (typeof(TValue) == typeof(bool))
        {
            obj = EditorGUILayout.Toggle((bool)(object)value);
        }
        else
        {
            EditorGUILayout.LabelField($"Type {typeof(TValue)} not supported");
        }

        return (TValue)obj;
    }
}
