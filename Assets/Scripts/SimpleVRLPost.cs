using System.Collections;
using UnityEngine;

namespace FatahDev
{
    public class SimpleVRLPost : MonoBehaviour
    {
        public string result;
        public WorkStepGroupId workStepGroupId = WorkStepGroupId.Titration;

        public void fillResult(string newResult)
        {
            result = newResult;
        }

        public void Complete(int stepId)
        {
            StartCoroutine(cor(stepId));
        }

        IEnumerator cor(int stepId)
        {
            yield return 0;
            VRLWorks.CompleteStep(workStepGroupId, stepId, true, result);
        }
    }
}
