using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data.SO_Script;
using GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class DataManager
    {
        public CharacterInfoSO charSO;
    
        private readonly string charPath = "CharacterInfo";
        
        public async UniTask Init()
        {
            try
            {
                charSO = await Addressables.LoadAssetAsync<CharacterInfoSO>(charPath);
                
            }
            catch (Exception e)
            {
                Debug.LogError("데이터 매니저 초기화 오류: " + e);
            }
        }

        public string GetCharacterName(SystemEnum.Character character, SystemEnum.Language language = SystemEnum.Language.KOR)
        {
            var name = "";
            foreach (var info in charSO.CharacterInfos.Where(info => info.name == character))
            {
                name = language switch
                {
                    SystemEnum.Language.KOR => info.name_KOR,
                    SystemEnum.Language.ENG => character.ToString(),
                    _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
                };
                break;
            }
            return name;
        }

        public string GetCharacterInfo(
            SystemEnum.Character character,
            SystemEnum.Language language = SystemEnum.Language.KOR)
        {
            var infoText = "";
            foreach (var info in charSO.CharacterInfos.Where(info => info.name == character))
            {
                infoText = language switch
                {
                    SystemEnum.Language.KOR => info.info,
                    _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
                };
                break;
            }
            return infoText;
        }
    }
}
