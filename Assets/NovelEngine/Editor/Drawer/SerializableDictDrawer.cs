using System.Collections.Generic;
using NovelEngine.Scripts;
using UnityEngine;
using UnityEditor;

public static class SerializableDictDrawer
{
    public static void DrawSerializableDict<TKey, TValue>(
        UnityEngine.Object hostObject,
        SerializableDict<TKey, TValue> dict,
        string label)
    {
        if (dict == null) return;

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        for (int i = 0; i < dict.pairs.Count; i++)
        {
            var pair = dict.pairs[i];

            EditorGUILayout.BeginHorizontal();

            if (typeof(TKey) == typeof(string))
            {
                string oldKey = (string)(object)pair.key;

                EditorGUI.BeginChangeCheck();
                string newKey = EditorGUILayout.DelayedTextField(oldKey);
                if (EditorGUI.EndChangeCheck() && newKey != oldKey)
                {
                    if (!string.IsNullOrEmpty(newKey) && !dict.ContainsKey((TKey)(object)newKey))
                    {
                        Undo.RecordObject(hostObject, "Change Dict Key");
                        TryChangeKey(dict, (TKey)(object)oldKey, (TKey)(object)newKey);
                        EditorUtility.SetDirty(hostObject);
                        GUIUtility.ExitGUI();
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            var newValue = DrawValueField(pair.value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(hostObject, "Change Dict Value");
                pair.value = newValue;
                EditorUtility.SetDirty(hostObject);
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                Undo.RecordObject(hostObject, "Remove Dict Entry");
                dict.pairs.RemoveAt(i);
                EditorUtility.SetDirty(hostObject);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add New"))
        {
            Undo.RecordObject(hostObject, "Add Dict Entry");
            dict.pairs.Add(new SerializableKeyValuePair<TKey, TValue>());
            EditorUtility.SetDirty(hostObject);
            GUIUtility.ExitGUI();
        }

        EditorGUI.indentLevel--;
    }

    private static void TryChangeKey<TKey, TValue>(SerializableDict<TKey, TValue> dict, TKey oldKey, TKey newKey)
    {
        int index = -1;
        for (int i = 0; i < dict.pairs.Count; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(dict.pairs[i].key, oldKey))
            {
                index = i;
                break;
            }
        }
        if (index < 0) return;

        var val = dict.pairs[index].value;
        dict.pairs.RemoveAt(index);
        dict.pairs.Insert(index, new SerializableKeyValuePair<TKey, TValue>(newKey, val));
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