using UnityEngine;

[RequireComponent(typeof(Interactable))]

public class NormalKey : MonoBehaviour
{
    //**********************//
    //    Private Fields    //
    //**********************//

    private Interactable m_interactable;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_interactable = GetComponent<Interactable>();
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void PickUp()
    {
        if (m_interactable.player) {
            m_interactable.player.GetComponent<PlayerInventory>().AddNormalKey();
            Destroy(gameObject);
        }
    }
}
