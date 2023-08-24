using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector] private List<Vector3> points;
    [SerializeField, HideInInspector] private bool isClosed;
    [SerializeField, HideInInspector] private bool autoSetControlPoints;

    public Vector3 this [int i]
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if (isClosed != value)
            {
                isClosed = value;

                if (isClosed)
                {
                    // anchor point added at the path end
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);

                    // add anchor point at the path beginning
                    points.Add(points[0] * 2 - points[1]);

                    if (autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    // remove anchor point at path end and begining.
                    points.RemoveRange(points.Count - 2, 2);

                    if (autoSetControlPoints)
                        AutoSetStartAndEndControls();
                }
            }
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }
        set
        {
            if(autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                
                if (autoSetControlPoints)
                    AutoSetAllControlPoints();
            }
        }
    }

    public Path(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.left,
            center + (Vector3.left + Vector3.forward) * 0.5f,
            center + (Vector3.right - Vector3.forward) * 0.5f,
            center + Vector3.right
        };
    }

    public Path(List<Vector3> points)
    {
        this.points = points;
    }

    public void AddSegment(Vector3 anchorPos)
    {
        /*
         * start, backward;
         * offset = start - backward;
         * forward = start + offset;
         *         = start * (start - backward);
         *         = start * 2 - backward;
         */

        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);

        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(points.Count - 1);
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });

        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        else
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
    }

    public void DeleteSegment(int anchorIndex)
    {
        if(NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                    points.RemoveRange(0, 3);
                }
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
                points.RemoveRange(anchorIndex - 2, 3);
            else
                points.RemoveRange(anchorIndex - 1, 3);
        }

        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(anchorIndex);
    }

    public Vector3[] GetPointInSegment(int i)
    {
        return new Vector3[]
        {
            points[i * 3 + 0],
            points[i * 3 + 1],
            points[i * 3 + 2],
            points[LoopIndex(i * 3 + 3)]
        };
    }

    public void MovePoint(int i, Vector3 pos)
    {
        if (i % 3 == 0 || !autoSetControlPoints)
        {
            Vector3 deltaMove = pos - points[i];

            points[i] = pos;

            if (autoSetControlPoints)
                AutoSetAllAffectedControlPoints(i);
            else
            {
                if (i % 3 == 0)
                {
                    // if is this path point, move anchor points with same offset.

                    if (i + 1 < points.Count || isClosed)
                        points[LoopIndex(i + 1)] += deltaMove;

                    if (i - 1 > -1 || isClosed)
                        points[LoopIndex(i - 1)] += deltaMove;
                }
                else
                {
                    /*      
                     *     control
                     *        |
                     *        v
                     *  acnhor 
                     *    |   
                     *    V   1          2
                     * 
                     *    0                   3
                     *   
                     * -1                        4  
                     */

                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
                    int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;

                    if (correspondingControlIndex > -1 && correspondingControlIndex < points.Count || isClosed)
                    {
                        float dst = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)]).magnitude;
                        Vector3 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;
                        points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)] + dir * dst;
                    }
                }
            }
        }
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1.0f)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0.0f;

        for(int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector3[] p = GetPointInSegment(segmentIndex);

            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2.0f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            
            float t = 0.0f;

            while(t <= 1.0f)
            {
                t += 1.0f / divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    void AutoSetAllAffectedControlPoints(int updateAnchorIndex)
    {
        for(int i = updateAnchorIndex - 3; i < updateAnchorIndex + 4; i += 3)
            if (i > -1 && i < points.Count || isClosed)
                AutoSetAnchorControlPoints(LoopIndex(i));
    }

    void AutoSetAllControlPoints()
    {
        for(int i = 0; i < points.Count; i += 3)
            AutoSetAnchorControlPoints(i);
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        /*      
         * target     
         *   |   +-1          +-2
         *   v
         *   
         *   0                   +-3
         */

        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector3.zero;
        float[] neighbourDistance = new float[2];
        
        // if neighbour exist or close enabled
        if(anchorIndex - 3 > -1 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistance[0] = offset.magnitude;
        }

        // if neighbour exist or close enabled
        if (anchorIndex + 3 < points.Count || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistance[1] = -offset.magnitude;
        }

        dir.Normalize();

        for(int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;

            if(controlIndex > -1 && controlIndex < points.Count || isClosed)
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistance[i] * 0.5f;
        }
    }

    void AutoSetStartAndEndControls()
    {
        /*      
         * start control
         *      |
         *      v
         *      
         *      1          2
         *
         *  0                   3
         */

        /*              end control
         *                   |
         *                   v
         *      
         *     l-3          l-2
         *     
         * l-4                   l-1
         * 
         * l = points.count
         */

        points[1] = (points[0] + points[3]) * 0.5f;
        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 4]) * 0.5f;
    }
}