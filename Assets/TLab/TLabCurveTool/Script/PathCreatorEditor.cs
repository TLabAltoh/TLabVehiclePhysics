using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;

    Path path
    {
        get
        {
            return creator.path;
        }
    }

    const float segmentSelectDistanceThreshold = 20.0f;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(creator, "Create new");
            creator.CreatePath();
        }

        bool isClosed = GUILayout.Toggle(path.IsClosed, "Closed");
        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(creator, "Toggle closed");
            path.IsClosed = isClosed;
        }

        if (!(path.NumSegments == 2 && path.IsClosed))
        {
            bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
            if (autoSetControlPoints != path.AutoSetControlPoints)
            {
                Undo.RecordObject(creator, "Toggle auto set controls");
                path.AutoSetControlPoints = autoSetControlPoints;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.KeyDown)
        {
            // add segment
            if (guiEvent.keyCode == KeyCode.A && !path.IsClosed)
            {
                Ray mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(mousePos, out hit, 100.0f))
                {
                    Undo.RecordObject(creator, "Add segment");
                    path.AddSegment(hit.point);
                }
            }

            // delete segment
            if (guiEvent.keyCode == KeyCode.D && !(path.NumSegments == 3 && path.IsClosed))
            {
                float minDstToAnchor = 20.0f;
                int closestAnchorIndex = -1;

                for (int i = 0; i < path.NumPoints; i += 3)
                {
                    Vector2 mousePosOnScene = new Vector2(guiEvent.mousePosition.x, SceneView.lastActiveSceneView.camera.pixelHeight - guiEvent.mousePosition.y);
                    Vector3 pathScreenPos = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(path[i]);
                    float dst = Vector2.Distance(mousePosOnScene, new Vector2(pathScreenPos.x, pathScreenPos.y));

                    if (dst < minDstToAnchor)
                    {
                        minDstToAnchor = dst;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(creator, "Delete Segment");
                    path.DeleteSegment(closestAnchorIndex);
                }
            }

            // split segment
            if (guiEvent.keyCode == KeyCode.S)
            {
                float minDstToSegment = segmentSelectDistanceThreshold;
                int SelectedSegmentIndex = -1;
                float lerpValue = -1.0f;

                Vector2 mousePosOnScene = new Vector2(guiEvent.mousePosition.x, SceneView.lastActiveSceneView.camera.pixelHeight - guiEvent.mousePosition.y);

                for (int i = 0; i < path.NumSegments; i++)
                {
                    Vector3[] points = path.GetPointInSegment(i);

                    for (float j = 0.0f; j < 1.0f; j += 0.01f)
                    {
                        Vector3 bezierPosition = Bezier.EvaluateCubic(points[0], points[1], points[2], points[3], j);
                        Vector3 bezierPositionOnScene = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(bezierPosition);

                        float dst = Vector2.Distance(mousePosOnScene, new Vector2(bezierPositionOnScene.x, bezierPositionOnScene.y));

                        if (dst < minDstToSegment)
                        {
                            minDstToSegment = dst;
                            SelectedSegmentIndex = i;
                            lerpValue = j;
                        }
                    }
                }

                if (SelectedSegmentIndex != -1)
                {
                    Vector3[] points = path.GetPointInSegment(SelectedSegmentIndex);
                    Vector3 bezierPosition = Bezier.EvaluateCubic(points[0], points[1], points[2], points[3], lerpValue); ;
                    Undo.RecordObject(creator, "Split segment");
                    path.SplitSegment(bezierPosition, SelectedSegmentIndex);
                }
            }
        }

        HandleUtility.AddDefaultControl(0);
    }

    void Draw()
    {
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointInSegment(i);

            if (creator.displayControlPoints)
            {
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }

            Handles.DrawBezier(points[0], points[3], points[1], points[2], creator.segmentCol, null, 2);
        }

        Handles.color = Color.red;

        for (int i = 0; i < path.NumPoints; i++)
        {
            if (i % 3 == 0 || creator.displayControlPoints)
            {
                Vector3 newPos;

                if(i % 3 == 0)
                    newPos = Handles.PositionHandle(path[i], Quaternion.identity);
                else
                {
                    Handles.color = creator.controlCol;
                    newPos = Handles.FreeMoveHandle(path[i], creator.controlDiameter, Vector3.zero, Handles.CylinderHandleCap);
                }

                if (path[i] != newPos)
                {
                    Undo.RecordObject(creator, "Move point");
                    path.MovePoint(i, newPos);
                }
            }
        }
    }

    void OnEnable()
    {
        creator = (PathCreator)target;

        if (creator.path == null) creator.CreatePath();
    }
}
#endif
