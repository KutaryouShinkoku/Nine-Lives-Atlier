using WanFramework.UI.DataComponent;
using Game.Data;

namespace Game.Model.InGameSubModel
{
    /// <summary>
    /// Buff状态
    /// </summary>
    public partial class BuffModel : DataModelBase
    {
        [AutoNotify]
        public BuffIds id;
        [AutoNotify]
        public int stack;
    }
}