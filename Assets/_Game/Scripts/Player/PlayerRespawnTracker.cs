using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor2D))]
public class PlayerRespawnTracker : MonoBehaviour
{
    [SerializeField] private float m_saveInterval = 30;
    [SerializeField] private LayerMask m_safeLayers = default;

    private Actor2D m_actor = default;
    private Vector2 m_respawnPoint = default;
    private float m_timer;

    // Start is called before the first frame update
    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_respawnPoint = transform.position;
        m_timer = m_saveInterval;
    }

    // Update is called once per frame
    void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer <= 0) {
            Transform contactBelow = m_actor.contacts.below;
            if (contactBelow && m_safeLayers.Contains(contactBelow.gameObject.layer)) {
                m_respawnPoint = transform.position;
                m_timer = m_saveInterval;
            }
        }
    }

    public Vector2 GetRespawnPoint()
    {
        return m_respawnPoint;
    }
}
