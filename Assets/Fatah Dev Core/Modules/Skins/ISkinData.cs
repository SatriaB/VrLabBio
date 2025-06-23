using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public interface ISkinData
    {
        string ID { get; }
        int Hash { get; }
        bool IsUnlocked { get; }
        AbstractSkinDatabase SkinsProvider { get; }

        void Init(AbstractSkinDatabase provider);
        void Unlock();
        
    }
}