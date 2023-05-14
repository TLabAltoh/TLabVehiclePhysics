using System.Collections.Generic;
using UnityEngine;

public class TLabReflectionCameraOperator : MonoBehaviour
{
    [SerializeField] Material waveMat;
    private Transform waveTransform;
    private Dictionary<Camera, Camera> refCameraHash;
    private RenderTexture inputTexture;
    private Vector4 nearClipPlane;
    private static readonly int RefTex = Shader.PropertyToID("_RefTex");

    private void GetReflectionCamera(out Camera reflectionCamera)
    {
        // Get current reflection camera.
        // Create a new reflection camera if it doesn't exist.
        // If the texture and camera already exist, skip the following steps

        if (inputTexture == null)
        {
            const int width = 16;
            const int height = 9;
            const int screenSize = 20;
            inputTexture = new RenderTexture(
                width * screenSize,
                height * screenSize,
                32,
                RenderTextureFormat.RGB565
            );
        }

        refCameraHash.TryGetValue(Camera.main, out reflectionCamera);

        if (reflectionCamera == null)
        {
            GameObject reflectionCameraObj = new GameObject();
            reflectionCameraObj.name = "ReflectionCamera for " + Camera.main.name;
            reflectionCamera = reflectionCameraObj.AddComponent<Camera>();

            // Don't render unless you call camera.Render()
            reflectionCamera.enabled = false;
            reflectionCamera.useOcclusionCulling = false;
            reflectionCamera.nearClipPlane = 0.3f;
            reflectionCamera.farClipPlane = 1000f;
            reflectionCamera.targetTexture = inputTexture;

            refCameraHash[Camera.main] = reflectionCamera;
        }
    }

    private void CreateReflectionMatrix(ref Matrix4x4 mat)
    {
        // A matrix that flips coordinates along the normal in local space
        Vector4 plane = nearClipPlane;
        mat.m00 = (1f - 2f * plane.x * plane.x);
        mat.m01 = (-2f * plane.x * plane.y);
        mat.m02 = (-2f * plane.x * plane.z);
        mat.m03 = (-2f * plane.x * plane.w);
        mat.m10 = (-2f * plane.y * plane.x);
        mat.m11 = (1f - 2f * plane.y * plane.y);
        mat.m12 = (-2f * plane.y * plane.z);
        mat.m13 = (-2f * plane.y * plane.w);
        mat.m20 = (-2f * plane.z * plane.x);
        mat.m21 = (-2f * plane.z * plane.y);
        mat.m22 = (1f - 2f * plane.z * plane.z);
        mat.m23 = (-2f * plane.z * plane.w);
        mat.m30 = 0f;
        mat.m31 = 0f;
        mat.m32 = 0f;
        mat.m33 = 1f;
    }

    void Start()
    {
        waveTransform = GetComponent<Transform>();
        nearClipPlane = new Vector4(0f, 1f, 0f, 0f);
        refCameraHash = new Dictionary<Camera, Camera>();
    }

    void Update()
    {
        // --------------------------------------
        // Create or update reflection camera
        //

        Camera reflectionCamera;
        GetReflectionCamera(out reflectionCamera);

        // Flip camera position
        Vector3 mainCameraPos = Camera.main.transform.position;
        reflectionCamera.transform.position = new Vector3(
            mainCameraPos.x,
            waveTransform.position.y - (mainCameraPos.y - waveTransform.position.y),
            mainCameraPos.z
        );

        // Reverse camera direction
        Vector3 mainCameraEluer = Camera.main.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(
            -mainCameraEluer.x,
             mainCameraEluer.y,
            -mainCameraEluer.z
        );

        // ------------------------------------------------------
        // Calculate clipping planes for reflection cameras
        //

        Matrix4x4 reflectionMatrix = new Matrix4x4();
        CreateReflectionMatrix(ref reflectionMatrix);

        // A matrix that reverses the coordinates with the reflective surface
        // as the origin (after World --> Local, returns to Local --> World)
        Matrix4x4 localReflectionMatrix =
            Camera.main.worldToCameraMatrix *
            waveTransform.localToWorldMatrix *
            reflectionMatrix *
            waveTransform.worldToLocalMatrix;

        // Reflection camera clipping plane orientation (local axis)
        Vector3 nearClipPlaneXYZ = new Vector3(nearClipPlane.x, nearClipPlane.y, nearClipPlane.z);

        // Calculate the normal direction of a reflective surface in camera space
        Vector3 cnormal = localReflectionMatrix.MultiplyVector(nearClipPlaneXYZ);
        Vector3 cpos = localReflectionMatrix.MultiplyPoint(Vector3.zero);

        Vector4 clipPlane = new Vector4(
             cnormal.x,
             cnormal.y,
             cnormal.z,

            // Set nearClipPlane (Set a little lower so that the clipped
            // margin of the object is not visible due to wave fluctuation)
            -Vector3.Dot(cpos, cnormal) - waveTransform.position.y * 0.85f
        );

        reflectionCamera.worldToCameraMatrix = localReflectionMatrix;
        reflectionCamera.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlane);

        // ------------------------------------------------------------------------------
        // Render the reflection camera footage to a texture and pass it to the shader.
        //

        // Flip the direction of the polygon normals to face the reflection camera
        GL.invertCulling = true;

        // Rendering with Reflection Camera
        reflectionCamera.Render();

        // Undo normal direction
        GL.invertCulling = false;

        // Pass the texture of the rendered reflection result to the shader
        waveMat.SetTexture(RefTex, inputTexture);
    }

    void OnGUI()
    {
#if false
        float h = Screen.height / 3;
        GUI.DrawTexture(new Rect(0, 0 * h, h, h), inputTexture);
#endif
    }
}
