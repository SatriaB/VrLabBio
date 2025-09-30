namespace FatahDev
{
    internal static class QuestSignals
    {
        public const string QUEST_GOAL_COMPLETED = "QUEST_GOAL_COMPLETED";
        
        public const string PINSET_SAMPLE_PICKED     = "PINSET_SAMPLE_PICKED";
        public const string SAMPLE_PLACED_ON_SLIDE   = "SAMPLE_PLACED_ON_SLIDE";
        public const string WATER_DROPPED_ON_SLIDE   = "WATER_DROPPED_ON_SLIDE";
        public const string SLIDE_INSERTED           = "SLIDE_INSERTED";

        public const string MICROSCOPE_ON            = "MICROSCOPE_ON";

        public const string OBJECTIVE_SET_4X         = "OBJECTIVE_SET_4X";
        public const string OBJECTIVE_SET_10X        = "OBJECTIVE_SET_10X";
        public const string OBJECTIVE_SET_40X        = "OBJECTIVE_SET_40X";
        public const string OBJECTIVE_SET_100X       = "OBJECTIVE_SET_100X";
        
        public const string CALIPER_SPECIMEN_PLACED  = "CALIPER_SPECIMEN_PLACED";
        public const string CALIPER_CONTACT_OK       = "CALIPER_CONTACT_OK";
        public const string CALIPER_MEASURE_CAPTURED = "CALIPER_MEASURE_CAPTURED";
        
        public const string MICROMETER_SPECIMEN_PLACED   = "MICROMETER_SPECIMEN_PLACED";   // benda sudah di antara anvil–spindle
        public const string MICROMETER_MEASURE_CAPTURED  = "MICROMETER_MEASURE_CAPTURED";  // simpan pembacaan (submit)
        
        public const string BALANCE_CONTAINER_PLACED= "BALANCE_CONTAINER_PLACED";// boat/kertas timbang diletakkan
        public const string BALANCE_SAMPLE_PLACED   = "BALANCE_SAMPLE_PLACED";   // sampel diletakkan
        public const string BALANCE_STABLE_READING  = "BALANCE_STABLE_READING";  // indikator stabil (ikon segitiga/“STABLE”)
        public const string BALANCE_CAPTURED        = "BALANCE_CAPTURED";        // hasil dicapture/submit
        public const string BALANCE_CLEANED         = "BALANCE_CLEANED";         // piringan dibersihkan (opsional)
    }
}