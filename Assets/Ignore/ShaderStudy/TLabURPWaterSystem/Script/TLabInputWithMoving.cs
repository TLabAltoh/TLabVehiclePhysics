using UnityEngine;

public class TLabInputWithMoving : MonoBehaviour
{
    private TLabWaterManager waterManager;
    private Vector2 prevHit;
    private LayerMask waterLayer;

    public TLabWaterManager WaterManager
    {
        set
        {
            waterManager = value;
        }
    }

    void Start()
    {
        prevHit = Vector2.zero;
        waterLayer = LayerMask.NameToLayer("Water");
    }

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(new Ray(transform.position, -transform.up), out hit, 10) && waterManager)
        {
            if (hit.collider.gameObject.layer == waterLayer)
            {
                waterManager.InputInWater(hit.textureCoord);
                prevHit = hit.textureCoord;
            }
            else
                waterManager.InputInWater(prevHit);
        }
    }
}
