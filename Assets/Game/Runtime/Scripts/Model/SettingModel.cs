using System.Collections.Generic;
using Game.Data;
using Game.Localization;
using WanFramework.UI.DataComponent;

namespace Game.Model
{
    [ShowInDataModelEditor]
    public partial class SettingModel : DataModelBase
    {
        /// <summary>
        /// 运行过的教学
        /// </summary>
        [AutoNotify]
        public DataCollection<TutorialIds> TutorialIds { get; } = new();

        [AutoNotify]
        private Language _language;
        
        [AutoNotify]
        private float _volume = 1.0f;

        [AutoNotify]
        private float _cameraShakeSensitivity = 1.0f;

        [AutoNotify]
        private bool _fastUse = true;
        
        public override void Reset()
        {
            base.Reset();
            TutorialIds.Clear();
            Language = LocalizeSystem.GetSystemLanguage();
            _volume = 1.0f;
        }
    }
}