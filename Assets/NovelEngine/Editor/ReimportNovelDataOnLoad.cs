//using UnityEditor;
//using UnityEditor.Compilation;
//using UnityEngine;

//[InitializeOnLoad]
//public static class ReimportNovelDataOnLoad
//{
//    static ReimportNovelDataOnLoad()
//    {
//        // 이미 설치됐으면 더 안 함
//        if (EditorPrefs.GetBool("Novel_ReimportOnLoad_Done", false)) return;

//        CompilationPipeline.compilationFinished += _ =>
//        {
//            // 여기서 필요한 에셋만 정확히 지정하거나 라벨/폴더로 찾으세요.
//            ReimportByType("NovelCharacterData");
//            ReimportByType("NovelAudioData");
//            ReimportByType("NovelVariableData");
//            ReimportByType("NovelBackgroundData");
//            ReimportByType("NovelScriptData");

//            EditorPrefs.SetBool("Novel_ReimportOnLoad_Done", true);
//            Debug.Log("[Novel] Reimport on first boot done.");
//        };
//    }

//    static void ReimportByType(string typeName)
//    {
//        var guids = AssetDatabase.FindAssets($"t:{typeName}");
//        foreach (var guid in guids)
//        {
//            var path = AssetDatabase.GUIDToAssetPath(guid);
//            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
//        }
//    }
//}
