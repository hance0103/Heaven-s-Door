using System;
using System.Collections.Generic;
using GamePlay;
using UnityEngine;

namespace Data.SO_Script
{
    
    [CreateAssetMenu(fileName = "CharacterInfoSO", menuName = "Scriptable Objects/CharacterInfoSO")]
    public class CharacterInfoSO : ScriptableObject
    {
        [SerializeField] private List<CharacterInfo> characterInfos = new List<CharacterInfo>();
        public List<CharacterInfo> CharacterInfos => characterInfos;
    }
    
    [Serializable]
    public class CharacterInfo
    {
        public SystemEnum.Character name;
        public string name_KOR;
        public string info;

        public SystemEnum.Judge judge;
        public float percentage;
    }

    [Serializable]
    public class CharacterSaveEntry
    {
        public SystemEnum.Character character;
        public SystemEnum.Judge judge;
        public float percentage;
    }
    
    [Serializable]
    public class CharacterSaveData
    {
        public List<CharacterSaveEntry> entries = new();
    }
}