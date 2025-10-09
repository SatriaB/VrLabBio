using UnityEngine;
using TMPro;

[ExecuteAlways]
public class TMP_BackwardCurve : MonoBehaviour
{
    public TMP_Text text;
    [Range(0.01f, 2f)]
    public float curveStrength = 0.5f;

    void OnEnable()
    {
        if (!text) text = GetComponent<TMP_Text>();
        text.ForceMeshUpdate();
    }

    void Update()
    {
        ApplyCurve();
    }

    void ApplyCurve()
    {
        if (!text) return;

        text.ForceMeshUpdate();
        var textInfo = text.textInfo;

        if (textInfo.characterCount == 0)
            return;

        float boundsMinX = text.bounds.min.x;
        float boundsMaxX = text.bounds.max.x;
        float width = boundsMaxX - boundsMinX;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int matIndex = charInfo.materialReferenceIndex;
            Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                Vector3 orig = verts[vertexIndex + j];
                float normalizedX = (orig.x - boundsMinX) / width; // 0 → 1 across text width
                float offsetY = Mathf.Sin(normalizedX * Mathf.PI) * -curveStrength * width; // NEGATIVE = backward curve
                verts[vertexIndex + j] = orig + new Vector3(0, offsetY, 0);
            }
        }

        // Apply vertex changes
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
