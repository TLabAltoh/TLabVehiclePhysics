using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TLabGCCSystemManager : MonoBehaviour
{
    public static TLabGCCSystemManager Instance;
    private AssetBundle m_assetBundle;

    private string GetThisName
    {
        get
        {
            return "[" + this.GetType().Name + "] ";
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this);

        Instance = this;
    }

    public IEnumerator DownloadAssetBundle(string modURL)
    {
        Debug.Log(GetThisName + "Start Load Asset");

        // Unload Existing Asset
        if (m_assetBundle != null) m_assetBundle.Unload(false);

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

        Debug.Log(GetThisName + "Finish Load Asset");

        var scene = m_assetBundle.GetAllScenePaths()[0];
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void LoadMod(string modURL)
    {
        StartCoroutine(DownloadAssetBundle(modURL));
    }
}
