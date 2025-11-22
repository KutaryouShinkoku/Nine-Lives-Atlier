using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Localization.Components;
using Game.Model;
using UnityEngine;
using WanFramework.Base;
using WanFramework.Data;
using WanFramework.UI.DataComponent;

namespace Game.Localization
{
    public enum Language
    {
        Cn,
        En
    }
    
    public class LocalizeSystem : SystemBase<LocalizeSystem>
    {
        public Language Current { get; private set; }
        public static Language GetSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return Language.Cn;
                default:
                    return Language.En;
            }
        }
        public override UniTask Init()
        {
            Current = GetSystemLanguage();
            return base.Init();
        }
        public string GetLocalText(LocalizeIds localizeId)
        {
            var entry = DataSystem.Instance.Load<LocalizeTable>().Get(localizeId);
            return Current switch
            {
                Language.En => entry.LangEn,
                Language.Cn => entry.LangCn,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public void SetDisplayLanguage(Language lang)
        {
            Current = lang;
            List<ILocalizeComponent> cache = new();
            foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                obj.GetComponentsInChildren(cache);
                foreach (var component in cache)
                    component.OnLanguageChanged();
            }
        }
        public void SwitchNextLanguage() => DataModel<SettingModel>.Instance.Language = GetNextLanguage();
        public Language GetNextLanguage()
        {
            var langId = (int)Current + 1;
            if (langId >= Enum.GetValues(typeof(Language)).Length)
                langId = 0;
            return (Language)langId;
        }
    }

    public static class LocalizeIdsExtensions
    {
        public static string Local(this LocalizeIds localizeId) => LocalizeSystem.Instance.GetLocalText(localizeId);
    }
}

