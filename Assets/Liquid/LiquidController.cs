using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class LiquidController : MonoBehaviour
{
    public Material mat; // assign or will use MeshRenderer.material
    public Color liquidColor = new Color(0f, 0.5f, 1f, 0.6f);
    Color currentColor = new Color(1, 1, 1);
    [Range(0f, 1f)] public float fill = 0.5f;
    [Range(0f, 1f)] public float currentFill = 0.5f;
    public float power = 1f;
    public float wobbleSpeed = 2f;
    public float wobbleStrength = 0.05f;
    public bool useWorldUp = true;     // true -> liquid stays level with gravity (world up)
    public bool invertTilt = false;    // flip tilt normal if it looks reversed
    public bool autoDetectCapsule = true; // try to auto-set _Radius/_HalfHeight from mesh

    MeshRenderer rend;
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        if (mat == null) mat = rend.material;

        if (autoDetectCapsule)
            AutoDetectCapsuleParams();
    }

    void AutoDetectCapsuleParams()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        // mesh bounds are in local space. We estimate radius and halfHeight in local-space units.
        var b = mf.sharedMesh.bounds;
        // extents.x/z represent radius-ish; extents.y is half of the full height (including caps)
        float approxRadius = Mathf.Max(b.extents.x, b.extents.z);
        float approxHalfTotal = b.extents.y;               // this is approx half of total mesh height
        float approxHalfHeight = Mathf.Max(0f, approxHalfTotal - approxRadius);

        // account for object scale
        Vector3 ls = transform.lossyScale;
        float radiusScaled = approxRadius * Mathf.Max(ls.x, ls.z);
        float halfHeightScaled = approxHalfHeight * ls.y;

        mat.SetFloat("_Radius", radiusScaled);
        mat.SetFloat("_HalfHeight", halfHeightScaled);
    }

    void Update()
    {
        if (mat == null) return;

        // fill / color / wobble
        if (Mathf.Abs(currentFill - fill) > 0.001f)
            currentFill = Mathf.MoveTowards(currentFill, fill, Time.deltaTime * power);
        mat.SetFloat("_Fill", Mathf.Clamp01(currentFill));

        currentColor = Color.Lerp(currentColor, liquidColor, Time.deltaTime * power);
        mat.SetColor("_Color", currentColor);

        mat.SetFloat("_WobbleSpeed", wobbleSpeed);
        mat.SetFloat("_WobbleStrength", wobbleStrength);

        // Compute tilt normal in OBJECT (local) space:
        // If we want liquid to stay level to gravity (world-up), convert Vector3.up into object-local:
        Vector3 tiltLocal;
        if (useWorldUp)
        {
            // world-up expressed in the object's local coordinate system:
            tiltLocal = transform.InverseTransformDirection(Vector3.up);
        }
        else
        {
            // Make surface follow object's local up => local-space normal is (0,1,0)
            tiltLocal = Vector3.up;
        }

        if (invertTilt) tiltLocal = -tiltLocal;

        mat.SetVector("_TiltNormal", new Vector4(tiltLocal.x, tiltLocal.y, tiltLocal.z, 0f));
    }

    public void changeFill(float _fill)
    {
        fill = _fill;
        Debug.Log("change fill : " + fill + " to " + _fill);
    }

    public void changeColor(string ColorCode)
    {
        Color parsedColor;
        if (ColorUtility.TryParseHtmlString(ColorCode, out parsedColor))
        {
            liquidColor = parsedColor; // apply to material
        }
        else
        {
            Debug.LogError("Invalid color code: " + ColorCode);
        }
    }
}
