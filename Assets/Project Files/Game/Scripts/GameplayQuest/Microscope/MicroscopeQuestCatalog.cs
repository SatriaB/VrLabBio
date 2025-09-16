using System;
using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public enum GoalKind
    {
        AssembleSlide, // irisan + air + cover slip
        PlaceSlide,    // letakkan slide di stage
        SetTurret      // set objektif (4/10/40/100)
    }

    [Serializable]
    public class QuestSpec
    {
        public string id;
        public string title;

        public GoalKind kind;

        // Khusus AssembleSlide
        public bool requireSlice = true;
        public bool requireWaterDrop = true;
        public bool requireCoverSlip = true;

        // Khusus SetTurret
        public int objective; // 4, 10, 40, 100
    }

    [CreateAssetMenu(menuName = "FatahDev/Microscope/Quest Catalog")]
    public class MicroscopeQuestCatalog : ScriptableObject
    {
        public List<QuestSpec> steps = new List<QuestSpec>();
    }
}