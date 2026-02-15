using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NovelEditorWindow
{
    [MenuItem("Novel/Variable")]
    public static void OpenVariableMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Variable");
    }

    [MenuItem("Novel/Audio")]
    public static void OpenAudioMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Audio");
    }

    [MenuItem("Novel/Background")]
    public static void OpenBackgroundMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Background");
    }

    [MenuItem("Novel/Character")]
    public static void OpenCharacterMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Character");
    }
    [MenuItem("Novel/Script")]
    public static void OpenScriptMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Script");
    }
    [MenuItem("Novel/Text Player")]
    public static void OpenTextPlayerMenu()
    {
        //SettingsService.OpenProjectSettings("Project/Novel/Text Player");
    }

}
