using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]

public class Interactable : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private UnityEvent m_onInteraction = default;

    //**********************//
    //    Private Fields    //
    //**********************//

    private PlayerInput m_input;
    private bool m_canInteract;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (m_canInteract && m_input.player.GetButtonDown(m_input.interactButton)) {
            m_onInteraction?.Invoke();
        }  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            m_canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            m_canInteract = false;
        }
    }
}
