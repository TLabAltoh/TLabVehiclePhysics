using UnityEngine;

public class TLabVihiclePhysics : MonoBehaviour
{
    [SerializeField] TLabLUT downForceCurve;
    [Header("Rigidbody Center")]
    [SerializeField] Vector3 center;

    public static TLabVihiclePhysics instance;

    private Rigidbody rb;
    private float downForce = 1f;
    private float localVelZ = 0f;

    public float DownForce
    {
        get
        {
            return downForce;
        }
    }

    public float MeterPerSecondInLocal
    {
        get
        {
            return localVelZ;
        }
    }

    public float KilometerPerHourInLocal
    {
        get
        {
            const float msToKmh = 3.6f;
            return localVelZ * msToKmh;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.maxAngularVelocity = Mathf.Infinity;
        rb.inertiaTensor = new Vector3(2886.4406f, 3003.5281f, 1971.9375f);
        rb.inertiaTensorRotation = Quaternion.identity;
        rb.centerOfMass = center;
    }

    void Update()
    {
        localVelZ = transform.InverseTransformDirection(rb.velocity).z;
        downForce = downForceCurve.Evaluate(Mathf.Abs(localVelZ) * 3.6f);

        Debug.DrawLine(transform.position, transform.position + transform.rotation * center);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * center, 0.1f);
    }
}
