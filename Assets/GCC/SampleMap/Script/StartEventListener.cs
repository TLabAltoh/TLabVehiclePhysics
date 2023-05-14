using UnityEngine;

public class StartEventListener : MonoBehaviour
{
    private LayerMask m_player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == m_player)
        {
#if UNITY_EDITOR
            Debug.Log("start time attack");
#endif
            UIManager.Instance.StartTimeAttack();
        }
    }

    private void Start()
    {
        m_player = LayerMask.NameToLayer("Player");
    }
}
