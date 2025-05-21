using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class HDRPFloat : MonoBehaviour
{
    public WaterSurface waterSurface; // Assign in Inspector
    public float floatHeight = 0.5f;
    public float bounceDamp = 0.1f;
    public float buoyancyForce = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (waterSurface == null || rb == null)
            return;

        WaterSearchParameters searchParams = new WaterSearchParameters();
        searchParams.targetPositionWS = transform.position;

        WaterSearchResult result;
        if (waterSurface.ProjectPointOnWaterSurface(searchParams, out result))
        {
            float waterHeight = result.projectedPositionWS.y;

            float depth = waterHeight - transform.position.y + floatHeight;

            if (depth > 0f)
            {
                Vector3 uplift = Vector3.up * (depth * buoyancyForce);
                rb.AddForce(uplift - rb.linearVelocity * bounceDamp, ForceMode.Acceleration);
            }
        }
    }
}
