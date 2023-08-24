using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [SerializeField, HideInInspector] public Path path;

    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public float anchorDiameter = 0.1f;
    public float controlDiameter = 0.75f;
    public bool displayControlPoints = true;

    public void CreatePath()
    {
        path = new Path(transform.position);
    }

    void Reset()
    {
        CreatePath();
    }
}
