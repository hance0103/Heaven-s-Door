using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelCharacterSO : ScriptableObject
{
    public string characterName;
    public Sprite body;
    public NovelEngine.Scripts.SerializableDict<string, Sprite> faceDict = new();
    public Vector2 headOffset;
    public string novelName;

    public Sprite GetHead(string head)
    {
        Sprite sprite = faceDict.GetValue(head);
        if (sprite == null)
        {
            Debug.LogError($"{head} 표정이 존재하지 않음");
            return null;
        }

        return sprite;
    }
}