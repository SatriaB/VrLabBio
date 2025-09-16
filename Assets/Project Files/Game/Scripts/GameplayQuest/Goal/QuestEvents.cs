// Assets/VRBio/Quest/Runtime/QuestEvents.cs
using System;
using System.Collections.Generic;

namespace FatahDev
{
    // Event Bus dengan nama variabel deskriptif
    public static class QuestEvents
    {
        public struct QuestEventData
        {
            public string Name;     // e.g., "turret.set.4"
            public object Payload;  // any payload
            public QuestEventData(string name, object payload = null) { Name = name; Payload = payload; }
        }

        public delegate void QuestEventHandler(QuestEventData eventData);

        static readonly Dictionary<string, List<QuestEventHandler>> handlersByEventName = new();

        public static void Subscribe(string eventName, QuestEventHandler handler)
        {
            if (!handlersByEventName.TryGetValue(eventName, out var handlers))
            {
                handlers = new List<QuestEventHandler>(2);
                handlersByEventName[eventName] = handlers;
            }
            if (!handlers.Contains(handler)) handlers.Add(handler);
        }

        public static void Unsubscribe(string eventName, QuestEventHandler handler)
        {
            if (handlersByEventName.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0) handlersByEventName.Remove(eventName);
            }
        }

        public static void Emit(string eventName, object payload = null)
        {
            if (handlersByEventName.TryGetValue(eventName, out var handlers))
            {
                var listenersSnapshot = handlers.ToArray(); // avoid mutation during invoke
                var eventData = new QuestEventData(eventName, payload);
                foreach (var handler in listenersSnapshot) handler?.Invoke(eventData);
            }
        }
    }

    // Named signals helper
    public static class QuestSignals
    {
        public const string SlidePrepared   = "slide.prepared";
        public const string SlideOnStage    = "slide.on_stage";
        public const string OilApplied      = "oil.applied";
        public const string LensCleaned     = "lens.cleaned";
        public const string ObjectiveSafe   = "objective.safe";

        public const string PowerOn         = "micros.power.on";
        public const string PowerOff        = "micros.power.off";
        public const string MicrosDocked    = "micros.docked";

        public static string TurretSet(int objective)    => $"turret.set.{objective}";
        public static string FocusOk(int objective)      => $"focus.ok.{objective}";
        public static string CaptureSaved(int objective) => $"capture.saved.{objective}";
    }

    // Payload types
    public struct DockedPayload
    {
        public bool twoHandedPickup;
        public bool orientationOk;
    }

    public struct FocusPayload
    {
        public int objective;
        public float quality;    // 0..1
        public bool usedMacroThenMicro;
    }

    public struct CapturePayload
    {
        public int objective;
        public string pathOrId;
    }

    public struct TurretPayload
    {
        public int objective;
    }
}
