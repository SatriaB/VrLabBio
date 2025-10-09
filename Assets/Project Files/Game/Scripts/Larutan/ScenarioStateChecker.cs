using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScenarioStateChecker : MonoBehaviour
{
    [SerializeField] private List<StateBoolCheck> boolState = new List<StateBoolCheck>();
    [SerializeField] private List<CustomIntCheck> integerState = new List<CustomIntCheck>();

    //Run when condition is fulfilled
    [SerializeField] private UnityEvent OnFulfilled = new UnityEvent();

    //Run when condition is not fulfilled
    [SerializeField] private UnityEvent OnUnFulfilled = new UnityEvent();

    //Run everytime check is called and condition is not fulfilled
    [SerializeField] private UnityEvent OnNotFulfilled = new UnityEvent();

    //Run everytime check is called and not even 1 condition is fulfilled
    [SerializeField] private UnityEvent OnNothingFulfilled = new UnityEvent();

    [SerializeField] private bool isAllFulfilled;

    #region IntegerState
    public void IntegerStateAdd(int index)
    {
        if (!gameObject.activeInHierarchy || !enabled) return;
        if (integerState[index].isFulfilled && integerState[index].stayComplete) return;
        integerState[index].count++;
        IntegerStateCheck(integerState[index]);
    }

    public void IntegerStateSubtract(int index)
    {
        if (!gameObject.activeInHierarchy || !enabled) return;
        if (integerState[index].isFulfilled && integerState[index].stayComplete) return;
        integerState[index].count--;
        IntegerStateCheck(integerState[index]);
    }

    private void IntegerStateCheck(CustomIntCheck _integerState)
    {
        if (_integerState.stayComplete && _integerState.isFulfilled) return;

        if (_integerState.count >= _integerState.targetCount) _integerState.isFulfilled = true;
        else _integerState.isFulfilled = false;
        CheckAll();
    }
    #endregion

    #region BoolState
    public void BoolStateTrue(string id)
    {
        if (!gameObject.activeInHierarchy || !enabled) return;
        foreach (var _bool in boolState)
        {
            if (_bool.checkName == id)
            {
                if (_bool.isFulfilled && _bool.stayComplete) return;
                _bool.isFulfilled = true;
                CheckAll();
                return;
            }
        }
    }
    public void BoolStateFalse(string id)
    {
        if (!gameObject.activeInHierarchy || !enabled) return;
        foreach (var _bool in boolState)
        {
            if (_bool.checkName == id)
            {
                if (_bool.isFulfilled && _bool.stayComplete) return;
                _bool.isFulfilled = false;
                CheckAll();
                return;
            }
        }
    }

    #endregion

    private void CheckAll()
    {
        if (!gameObject.activeInHierarchy || !enabled) return;

        bool somethingFulfilled = false;
        bool allFulfilled = true;

        foreach (var _boolState in boolState)
        {
            if (_boolState.isFulfilled) somethingFulfilled = true;
            else allFulfilled = false;

            if (somethingFulfilled && !allFulfilled) break;
        }

        foreach (var _integerState in integerState)
        {
            if (_integerState.isFulfilled) somethingFulfilled = true;
            else allFulfilled = false;

            if (somethingFulfilled && !allFulfilled) break;
        }

        if (!somethingFulfilled)
            OnNothingFulfilled.Invoke();

        if (allFulfilled)
            OnFulfilled.Invoke();
        else
        {
            if (isAllFulfilled)
                OnUnFulfilled.Invoke();

            OnNotFulfilled.Invoke();
        }

        isAllFulfilled = allFulfilled;
    }

    #region Class_and_Enum

    [System.Serializable]
    private class StateBoolCheck
    {
        public string checkName = "";

        [Tooltip("Kalo dah complete gabisa dibikin incomplete")]
        public bool stayComplete = true;
        public bool isFulfilled = false;
    }

    [System.Serializable]
    private class CustomIntCheck
    {
        public int count;
        public int targetCount;

        [Tooltip("Kalo dah complete gabisa dibikin incomplete")]
        public bool stayComplete = true;
        public bool isFulfilled = false;
    }
    private enum StateCheckerSectionEnum
    {
        COMPLETE, INCOMPLETE
    }

    private enum StateCheckerItemEnum
    {
        OCCUPYING, UNOCCUPYING
    }

    private enum StateCheckerSocketEnum
    {
        OCCUPIED, UNOCCUPIED, TOOL_LOCKED, TOOL_UNLOCKED
    }
    #endregion

}
