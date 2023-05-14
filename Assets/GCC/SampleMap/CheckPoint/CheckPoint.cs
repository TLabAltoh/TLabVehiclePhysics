using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [System.NonSerialized] public bool m_flag;
    private LayerMask m_player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == m_player)
        {
#if UNITY_EDITOR
            Debug.Log("Collision enter");
#endif
            m_flag = true;
        }
    }

    private void Start()
    {
        m_player = LayerMask.NameToLayer("Player");
    }
}
