// Assets/GameplayQuest/Editor/MicroscopeQuestRunnerEditor.cs
// Inspector = +Build & Start + semua tombol debug (power/turret/slide/focus/capture/oil/shutdown/dock)
// NOTE: letakkan file ini di folder "Editor/"

namespace FatahDev
{
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;
    using FatahDev; // ← penting: pakai QuestEvents/QuestSignals/FocusPayload dari runtime

    [CustomEditor(typeof(MicroscopeQuestRunner))]
    public class MicroscopeQuestRunnerEditor : Editor
    {
        // Focus controls
        int focusObjective = 4;
        float focusQuality = 1.0f;
        bool usedMacroThenMicro = true;

        // Capture controls
        int captureObjective = 4;
        string capturePathOrId = "debug_capture.png";

        // Dock controls
        bool dockTwoHanded = true;
        bool dockOrientationOk = true;

        bool showLiveDebug = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            var runner = (MicroscopeQuestRunner)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                {
                    if (GUILayout.Button("Build & Start Quest", GUILayout.Height(24)))
                        runner.BuildAndStartQuest();

                    if (GUILayout.Button("Advance To Next Goal", GUILayout.Height(24)))
                    {
                        // panggil private AdvanceToNextGoal() via refleksi (debug only)
                        var m = typeof(MicroscopeQuestRunner).GetMethod("AdvanceToNextGoal",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        m?.Invoke(runner, null);
                    }
                }
            }

            EditorGUILayout.Space(10);
            showLiveDebug = EditorGUILayout.Foldout(showLiveDebug, "Live Debug Controls (Editor → emit event)");
            if (showLiveDebug)
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Masuk Play Mode untuk mengaktifkan tombol debug.", MessageType.Info);
                    GUI.enabled = false;
                }
                
                EditorGUILayout.LabelField("Prep Slide (prep_slide)", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Slice done (slide.slice.done)"))
                        QuestEvents.Emit("slide.slice.done");

                    if (GUILayout.Button("Water dropped (slide.water.dropped)"))
                        QuestEvents.Emit("slide.water.dropped");

                    if (GUILayout.Button("Cover applied (slide.cover.applied)"))
                        QuestEvents.Emit("slide.cover.applied");
                }

                if (GUILayout.Button("Slide prepared (slide.prepared)"))
                {
                    QuestEvents.Emit(QuestSignals.SlidePrepared);
                }
                EditorGUILayout.Space(6);

                GUILayout.Label("Phase 1 – Menyiapkan mikroskop", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Power ON")) QuestEvents.Emit(QuestSignals.PowerOn);
                    if (GUILayout.Button("Power OFF")) QuestEvents.Emit(QuestSignals.PowerOff);
                }
                if (GUILayout.Button("Set Revolver 4×"))
                    QuestEvents.Emit(QuestSignals.TurretSet(4), new TurretPayload { objective = 4 });

                EditorGUILayout.Space(4);
                GUILayout.Label("Phase 2 – 4× (4×10)", EditorStyles.boldLabel);
                if (GUILayout.Button("Letakkan preparat (slide.on_stage)"))
                    QuestEvents.Emit(QuestSignals.SlideOnStage);

                // Focus controls
                EditorGUILayout.BeginHorizontal();
                focusObjective = EditorGUILayout.IntPopup("Focus Objective", focusObjective,
                    new[] { "4×", "10×", "40×", "100×" }, new[] { 4, 10, 40, 100 });
                focusQuality = EditorGUILayout.Slider("Quality", focusQuality, 0f, 1f);
                EditorGUILayout.EndHorizontal();
                usedMacroThenMicro = EditorGUILayout.Toggle("Macro→Micro Order", usedMacroThenMicro);

                if (GUILayout.Button($"Fokus OK ({focusObjective}×)"))
                {
                    QuestEvents.Emit(QuestSignals.FocusOk(focusObjective), new FocusPayload
                    {
                        objective = focusObjective,
                        quality = focusQuality,
                        usedMacroThenMicro = usedMacroThenMicro
                    });
                }

                // Capture controls
                EditorGUILayout.BeginHorizontal();
                captureObjective = EditorGUILayout.IntPopup("Capture Objective", captureObjective,
                    new[] { "4×", "10×", "40×", "100×" }, new[] { 4, 10, 40, 100 });
                capturePathOrId = EditorGUILayout.TextField("Path/ID", capturePathOrId);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button($"Capture ({captureObjective}×)"))
                {
                    QuestEvents.Emit(QuestSignals.CaptureSaved(captureObjective), new CapturePayload
                    {
                        objective = captureObjective,
                        pathOrId = capturePathOrId
                    });
                }

                EditorGUILayout.Space(4);
                GUILayout.Label("Phase 3 – 10× (10×10)", EditorStyles.boldLabel);
                if (GUILayout.Button("Set Revolver 10×"))
                    QuestEvents.Emit(QuestSignals.TurretSet(10), new TurretPayload { objective = 10 });

                EditorGUILayout.Space(4);
                GUILayout.Label("Phase 4 – 40× (40×10)", EditorStyles.boldLabel);
                if (GUILayout.Button("Set Revolver 40×"))
                    QuestEvents.Emit(QuestSignals.TurretSet(40), new TurretPayload { objective = 40 });

                EditorGUILayout.Space(4);
                GUILayout.Label("Phase 5 – 100× (100×10)", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Tetes Minyak (oil.applied)"))
                        QuestEvents.Emit(QuestSignals.OilApplied);
                    if (GUILayout.Button("Set Revolver 100×"))
                        QuestEvents.Emit(QuestSignals.TurretSet(100), new TurretPayload { objective = 100 });
                }

                EditorGUILayout.Space(4);
                GUILayout.Label("Phase 6 – Shutdown & Perawatan", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Objective Safe (angkat makro)"))
                        QuestEvents.Emit(QuestSignals.ObjectiveSafe);
                    if (GUILayout.Button("Bersihkan Lensa"))
                        QuestEvents.Emit(QuestSignals.LensCleaned);
                }

                if (GUILayout.Button("Kembali ke 4× (turret.set.4)"))
                    QuestEvents.Emit(QuestSignals.TurretSet(4), new TurretPayload { objective = 4 });

                if (GUILayout.Button("Power OFF"))
                    QuestEvents.Emit(QuestSignals.PowerOff);

                EditorGUILayout.Space(6);
                GUILayout.Label("Dock Payload", EditorStyles.miniBoldLabel);
                dockTwoHanded = EditorGUILayout.Toggle("Two-handed pickup", dockTwoHanded);
                dockOrientationOk = EditorGUILayout.Toggle("Orientation OK", dockOrientationOk);

                if (GUILayout.Button("Dock (micros.docked)"))
                {
                    QuestEvents.Emit(QuestSignals.MicrosDocked, new DockedPayload
                    {
                        twoHandedPickup = dockTwoHanded,
                        orientationOk = dockOrientationOk
                    });
                }

                if (!EditorApplication.isPlaying) GUI.enabled = true;
            }
        }
    }
}
