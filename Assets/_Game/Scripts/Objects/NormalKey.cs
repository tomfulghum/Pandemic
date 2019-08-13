using UnityEngine;

[RequireComponent(typeof(Interactable))]

public class NormalKey : MonoBehaviour
{
    //**********************//
    //    Private Fields    //
    //**********************//

    private NormalKeyState m_state = default;
    private Interactable m_interactable = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_state = new NormalKeyState();
        m_interactable = GetComponent<Interactable>();
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void PickUp()
    {
        m_state.pickedUp = true;
        gameObject.SetActive(false);
    }

    public void AddKeyToPlayer()
    {
        if (m_interactable.player) {
            m_interactable.player.GetComponent<PlayerInventory>().normalKeyCount += 1;
        }
    }

    public void SetState(NormalKeyState _state)
    {
        m_state = _state;
        if (m_state.pickedUp) {
            m_interactable.SimulateInteraction();
        }
    }

    public NormalKeyState GetState()
    {
        return m_state;
    }
}
