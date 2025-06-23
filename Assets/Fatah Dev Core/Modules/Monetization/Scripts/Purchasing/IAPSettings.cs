using UnityEngine;

namespace FatahDev
{
    public class IAPSettings : ScriptableObject
    {
        [SerializeField, Hide] IAPItem[] storeItems;
        public IAPItem[] StoreItems => storeItems;
    }
}