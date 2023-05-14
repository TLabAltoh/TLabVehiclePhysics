using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TLabGCCSystemManager : MonoBehaviour
{
    public static TLabGCCSystemManager Instance;

    [System.NonSerialized] public float m_arbRear;
    [System.NonSerialized] public float m_arbFront;

    private AssetBundle m_assetBundle;

    private void Awake()
    {
        if (TLabGCCSystemManager.Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this);

        Instance = this;
    }

    public void LoadMod(string modURL)
    {
        StartCoroutine(DownloadAssetBundle(modURL));
    }

    public IEnumerator DownloadAssetBundle(string modURL)
    {
        Debug.Log("Start Load Asset");

        if (m_assetBundle != null)
            m_assetBundle.Unload(false);

        var request = UnityWebRequestAssetBundle.GetAssetBundle(modURL);
        yield return request.SendWebRequest();

        // Handle error
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError(request.error);
            yield break;
        }

        var handler = request.downloadHandler as DownloadHandlerAssetBundle;
        m_assetBundle = handler.assetBundle;

        Debug.Log("Finish Load Asset");

        var scene = m_assetBundle.GetAllScenePaths()[0];
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
