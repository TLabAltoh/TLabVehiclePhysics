using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using TLab.VehiclePhysics;
using TLab.InputField;

public class UIManager : MonoBehaviour
{
    [Header("Vehicle System Manager")]
    [SerializeField] VehicleSystemManager systemManager;

    [Header("Vehicle Engine")]
    [SerializeField] VehicleEngine engine;

    [Header("Check Points")]
    [SerializeField] CheckPoint[] m_checkPoints;

    [Header("Base Window")]
    [SerializeField] GameObject m_epsTimePanel;
    [SerializeField] GameObject m_statePanel;
    [SerializeField] TextMeshProUGUI m_elapsedTime;
    [SerializeField] TextMeshProUGUI m_state;

    [Header("Menu Window")]
    [SerializeField] GameObject m_menuWindow;
    [SerializeField] string m_rankingUrl;

    [Header("Result Window")]
    [SerializeField] GameObject m_resultWindow;
    [SerializeField] TLabInputField m_nameInputField;
    [SerializeField] TextMeshProUGUI m_resultWindowRankingText;
    [SerializeField] string m_registerUrl;

    [Header("Audio")]
    [SerializeField] AudioSource clickAudio;

    private IEnumerator m_startTimeAttack = null;
    private bool m_alreadyRegistered;

    public static UIManager Instance;

    public void ExitFromResultWindow()
    {
        engine.SwitchEngine(true);
        systemManager.GettingOff = false;
        m_resultWindow.gameObject.SetActive(false);
    }

    private void DestroyTimeAttackTask()
    {
        if (m_startTimeAttack != null)
        {
            // StopCoroutine("TimeAttackTask");
            StopCoroutine(m_startTimeAttack);
#if UNITY_EDITOR
            Debug.Log("stop time attack");
#endif
            m_startTimeAttack = null;
        }
    }

    public void StartTimeAttack()
    {
        m_resultWindow.gameObject.SetActive(false);
        m_epsTimePanel.gameObject.SetActive(true);
        m_elapsedTime.text = "";

        ResetCheckPointFlag(true);

        DestroyTimeAttackTask();
        // https://mono-pro.net/archives/6743
        m_startTimeAttack = TimeAttackTask();
        StartCoroutine(m_startTimeAttack);

        // Ç±Ç¡ÇøÇ≈Ç‡ê≥èÌÇ…èIóπÇ≈Ç´ÇÈ(ëΩï™)
        // StartCoroutine("TimeAttackTask");
    }

    private void ResetCheckPointFlag(bool active)
    {
        for (int i = 0; i < m_checkPoints.Length; i++)
        {
            CheckPoint current = m_checkPoints[i];
            current.m_flag = !active;
            current.gameObject.SetActive(active);
        }
    }

    IEnumerator TimeAttackTask()
    {
        m_statePanel.gameObject.SetActive(true);
        m_state.text = "Start";

        float remain = 1.0f;
        while ((remain -= Time.deltaTime) > 0)
            yield return null;

        m_state.text = "";
        m_statePanel.gameObject.SetActive(false);

        float elapsed = 0;
        bool finish = false;

        while (!finish)
        {
            finish = true;

            elapsed += Time.deltaTime;
            m_elapsedTime.text = string.Format("{0:f2}", elapsed) + " [s]";

            for (int i = 0; i < m_checkPoints.Length; i++)
            {
                CheckPoint current = m_checkPoints[i];
                finish = finish && current.m_flag;

                if (current.m_flag)
                    current.gameObject.SetActive(false);
            }

            yield return null;
        }

        m_statePanel.gameObject.SetActive(true);
        m_state.text = "Finish";

        remain = 3.0f;
        while ((remain -= Time.deltaTime) > 0)
            yield return null;

        engine.SwitchEngine(false);
        systemManager.GettingOff = true;

        m_state.text = "";
        m_statePanel.gameObject.SetActive(false);

        m_resultWindowRankingText.text = "Ranking : ";

        m_alreadyRegistered = false;
        m_resultWindow.gameObject.SetActive(true);
        m_nameInputField.SetPlaceHolder("Enter your name ...");
    }

    // 
    // PlayWindow
    //

    public void OnMenuButtonPress()
    {
        clickAudio.Play();
        m_menuWindow.SetActive(!m_menuWindow.activeSelf);
    }

    public void OnRankingButtonPress()
    {
        Application.OpenURL(m_rankingUrl);
    }

    //
    // MenuWindow
    //

    public void OnBackToTitleButtonPress()
    {
        clickAudio.Play();
        DestroyTimeAttackTask();
        MapManager.Instance.BackToTitle();
    }

    public void OnResetRotationButtoPress()
    {
        clickAudio.Play();

        var rb = systemManager.transform.parent.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.transform.up = Vector3.up;

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    IEnumerator RegisterTime()
    {
        yield return null;

        WWWForm form = new WWWForm();
        form.AddField("table", "test_mod_ranking");
        form.AddField("name", m_nameInputField.text);
        form.AddField("time", m_elapsedTime.text);
        form.AddField("is_regist", "true");
        using (UnityWebRequest www = UnityWebRequest.Post(m_registerUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }                
            else if(www.result == UnityWebRequest.Result.InProgress)
            {
                yield return null;                
            }
            else if(www.result == UnityWebRequest.Result.Success)
            {
#if TLAB_RELEASE
                Debug.Log("Form upload complete! ");
#endif
                m_resultWindowRankingText.text = www.downloadHandler.text;
            }
        }
    }

    public void OnResisterButtonPress()
    {
        clickAudio.Play();

        if (m_alreadyRegistered == true)
            return;

        if (m_nameInputField.text == "")
            m_nameInputField.SetPlaceHolder("! Enter your name");
        else if(m_nameInputField.text.Length > 20)
        {
            m_nameInputField.SetPlaceHolder("! Must be 20 characters or less");
        }
        else
        {
            m_alreadyRegistered = true;
            m_resultWindowRankingText.text = "Ranking : •••";
            m_nameInputField.Display();

#if TLAB_RELEASE
            StartCoroutine("RegisterTime");
#endif
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_state.text = "";
        m_elapsedTime.text = "";

        m_statePanel.gameObject.SetActive(false);
        m_epsTimePanel.gameObject.SetActive(false);
        m_resultWindow.gameObject.SetActive(false);
        m_menuWindow.gameObject.SetActive(false);

        ResetCheckPointFlag(false);
    }
}
