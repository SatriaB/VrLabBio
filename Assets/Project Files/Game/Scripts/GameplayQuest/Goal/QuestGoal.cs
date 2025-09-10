// Assets/VRBio/Quest/Runtime/Goals/Core/QuestGoal.cs

using System;
using UnityEngine;

namespace FatahDev
{
    public enum GoalState
    {
        Inactive,
        Active,
        Completed,
        Failed
    }


    public abstract class QuestGoal
    {
        public string Id { get; private set; }
        public GoalState State { get; private set; } = GoalState.Inactive;
        protected GoalParams Parameters { get; private set; }

        public void Begin(string id, GoalParams parameters)
        {
            Id = id;
            Parameters = parameters ?? GoalParams.Empty;

            // >>> INI WAJIB ADA <<<
            State = GoalState.Active;

            OnBegin();
        }

        public void Cancel()
        {
            if (State == GoalState.Active) OnCancel();
            State = GoalState.Inactive;
        }

        protected void Complete()
        {
            if (State != GoalState.Active) return;
            State = GoalState.Completed;
            OnComplete();
        }

        protected void Fail(string reason = null)
        {
            if (State != GoalState.Active) return;
            State = GoalState.Failed;
            OnFailed(reason);
        }

        protected abstract void OnBegin();

        protected virtual void OnCancel()
        {
        }

        protected virtual void OnComplete()
        {
        }

        protected virtual void OnFailed(string reason)
        {
        }
    }


    public sealed class GoalParams
    {
        readonly System.Collections.Generic.Dictionary<string, object> parameterMap;

        public static readonly GoalParams Empty =
            new(new System.Collections.Generic.Dictionary<string, object>());

        public GoalParams(System.Collections.Generic.Dictionary<string, object> map)
        {
            parameterMap = map;
        }

        public int GetInt(string key, int defaultValue = 0) =>
            parameterMap != null && parameterMap.TryGetValue(key, out var value)
                ? Convert.ToInt32(value)
                : defaultValue;

        public float GetFloat(string key, float defaultValue = 0f) =>
            parameterMap != null && parameterMap.TryGetValue(key, out var value)
                ? Convert.ToSingle(value)
                : defaultValue;

        public bool GetBool(string key, bool defaultValue = false) =>
            parameterMap != null && parameterMap.TryGetValue(key, out var value)
                ? Convert.ToBoolean(value)
                : defaultValue;

        public string GetString(string key, string defaultValue = null) =>
            parameterMap != null && parameterMap.TryGetValue(key, out var value) ? value?.ToString() : defaultValue;
    }
}