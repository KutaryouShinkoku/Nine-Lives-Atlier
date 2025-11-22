using UnityEngine;

namespace WanFramework.Data
{
    public class DataTableRawAsset : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        private byte[] data;
        public void SetData(byte[] d) => data = d;
        public byte[] GetData() => data;
        
        [SerializeField]
        [HideInInspector]
        private string typeName;
        public void SetTypeName(string n) => typeName = n;
        public string GetTypeName() => typeName;
    }

}