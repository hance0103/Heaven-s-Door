#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
public enum NovelDataType
{
    Audio,
    Background,
    Character,
    Variable,
    Script
}
public static class NovelEditorUtils
{
    public const string NovelRoot = "Assets/NovelEngine/Addressable";
    public const string NovelResourcePath = NovelRoot + "/NovelResourceData";


    public static T GetOrCreateSettings<T> (string path) where T : ScriptableObject
    {
        var settings = AssetDatabase.LoadAssetAtPath<T>(path);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<T>();


            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            // 어드레서블로 설정 일단 해줄필요는 없는데 나중에 필요할시 수정할것
            //AddToAddressables(path, address ?? Path.GetFileNameWithoutExtension(path));
        }
        return settings;
    }
    public static SerializedObject GetSerializedSettings<T>(string path) where T : ScriptableObject
    {
        return new SerializedObject(GetOrCreateSettings<T>(path));
    }

    public static T LoadSettingsOrNull<T>(string path) where T : ScriptableObject
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static string GetNovelResourceDataPath(NovelDataType type)
    {
        string path = NovelResourcePath + "/Novel" + type.ToString() + "Data.asset";
        return path;
    }


    /// <summary>
    /// 에셋을 Addressables에 등록합니다.
    /// - groupName 지정 시 해당 그룹에, 없으면 기본 그룹에 등록
    /// - labelName 지정 시 해당 라벨을 부여(없으면 생성), 미지정이면 라벨 변경 없음
    /// - address 미지정이면 기존 주소 유지
    /// </summary>
    public static void AddToAddressables(string assetPath, string address = null, string groupName = null, string labelName = null)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("AddressableAssetSettings not found.");
            return;
        }

        var group = string.IsNullOrEmpty(groupName) ? settings.DefaultGroup : settings.FindGroup(groupName);
        if (group == null)
        {
            Debug.LogError($"Addressables group not found: '{groupName}'. (기본 그룹도 비어 있음)");
            return;
        }

        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError($"Invalid asset path: {assetPath}");
            return;
        }

        var entry = settings.CreateOrMoveEntry(guid, group);
        if (!string.IsNullOrEmpty(address))
            entry.address = address;

        if (!string.IsNullOrEmpty(labelName))
        {
            var labels = settings.GetLabels();
            if (!labels.Contains(labelName))
                settings.AddLabel(labelName);

            entry.SetLabel(labelName, true, true);
        }

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}
#endif