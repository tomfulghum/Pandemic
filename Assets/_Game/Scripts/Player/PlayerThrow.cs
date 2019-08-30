using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//
    public enum ThrowState 
    {
        Inactive,
        Aiming
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private bool m_slowTimeWhileAiming = default;
    [SerializeField] private float m_minThrowVelocity = 15f;
    [SerializeField] private float m_maxThrowVelocity = 25f;

    //******************//
    //    Properties    //
    //******************//

    public GameObject pickedUpObject
    {
        get { return m_pickedUpObject; }
        set { m_pickedUpObject = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private ThrowState m_currentThrowState = ThrowState.Inactive;

    private float m_normalTimeScale;
    private GameObject m_pickedUpObject;

    private Vector2 m_throwVelocity;
    private Vector2 m_contDirWithoutDeadzone;

    private PlayerAnim m_pa;
    private PlayerHook m_ph;
    private PlayerInput m_input;
    private PlayerMovement m_pm;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        m_normalTimeScale = Time.timeScale;
        m_ph = GetComponent<PlayerHook>();
        m_pa = GetComponent<PlayerAnim>();
        m_input = GetComponent<PlayerInput>();
        m_pm = GetComponent<PlayerMovement>();
    }


    private void Update()
    {
        if (m_pa.currentPlayerState == PlayerAnim.PlayerState.Waiting || m_pa.currentPlayerState == PlayerAnim.PlayerState.Aiming || m_pa.currentPlayerState == PlayerAnim.PlayerState.Hook)
        {
            if (m_input.player.GetAxis(m_input.aimHorizontalAxis) != 0 || m_input.player.GetAxis(m_input.aimVerticalAxis) != 0)
            {
                m_contDirWithoutDeadzone.x = m_input.player.GetAxis(m_input.aimHorizontalAxis);
                m_contDirWithoutDeadzone.y = m_input.player.GetAxis(m_input.aimVerticalAxis);
            }

            SetPlayerState();

            if (m_input.player.GetButton(m_input.throwButton)) //can use aim
            {
                if (m_pickedUpObject != null && m_pickedUpObject.GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.PickedUp)
                {
                    if (m_pa.currentPlayerState == PlayerAnim.PlayerState.Hook || m_ph.currentHookState != PlayerHook.HookState.Inactive)
                    {
                        m_ph.CancelHook();
                    }
                    AimThrow();
                }
            }
            else if (m_input.player.GetButtonUp(m_input.throwButton) && m_currentThrowState == ThrowState.Aiming)
            {
                if (m_pickedUpObject != null && m_pickedUpObject.GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.PickedUp)
                {
                    ThrowObject(m_throwVelocity);
                }
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetPlayerState() //vllt muss man die nochmal überarbeiten
    {
        if (m_currentThrowState != ThrowState.Inactive)
        { 
            m_pa.currentPlayerState = PlayerAnim.PlayerState.Aiming;
        }
        else
        {
            if (m_pa.currentPlayerState == PlayerAnim.PlayerState.Aiming && m_currentThrowState == ThrowState.Inactive)
            {
                m_pa.currentPlayerState = PlayerAnim.PlayerState.Waiting;
            }
        }
    }

    private void AimThrow()
    {
        m_currentThrowState = ThrowState.Aiming;

        m_pm.DisableUserInput(true);
        m_pm.externalVelocity = Physics2D.gravity * 0.5f;
        //m_pm.momentum = MomentumVelocity;

        if (m_slowTimeWhileAiming)
        {
            SlowTime();
        }

        m_throwVelocity = GetAimDirection(m_contDirWithoutDeadzone);
        //if (m_usingController)
        //{
        //    m_throwVelocity = GetAimDirection(m_contDirWithoutDeadzone);
        //}
        //else
        //{
        //    m_throwVelocity = GetAimDirection(m_mouseDirection); //wird das funktionieren? // wenn ja wie gut? --> funktioniert ganz ok --> bei maus wird immer mit maximaler kraft geworfen
        //}

        if (m_input.player.GetButtonDown(m_input.jumpButton))
        {
            m_pickedUpObject.GetComponent<ThrowableObject>().Drop();
            m_pickedUpObject = null;
            DeactivateThrow();
            GetComponent<VisualizeTrajectory>().RemoveVisualDots();
        }
    }

    private Vector2 GetAimDirection(Vector2 _direction) //evlt während aim den spieler anhalten oder bewegung verlangsamen //schauen ob man die deadzone lassen kann ansonsten als parameter übergeben
    {
        float velocity = Mathf.Lerp(m_minThrowVelocity, m_maxThrowVelocity, (Mathf.Abs(_direction.x) + Mathf.Abs(_direction.y)));
        Vector2 throwVelocity = new Vector2(_direction.x, _direction.y).normalized * velocity; //falls wir nicht lerpen --> public float ThrowSpeed
        GetComponent<VisualizeTrajectory>().VisualizeDots(transform.position, throwVelocity);
        return throwVelocity;
    }

    private void SlowTime()
    {
        if (Time.timeScale > m_ph.maxTimeSlow)
        {
            Time.timeScale *= 0.93f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }

    private void ThrowObject(Vector2 _throwVelocity)
    {
        GetComponent<VisualizeTrajectory>().RemoveVisualDots();
        m_pickedUpObject.GetComponent<ThrowableObject>().Throw(_throwVelocity, true);
        m_pickedUpObject = null;
        DeactivateThrow();
    }

    private void DeactivateThrow()
    {
        m_currentThrowState = ThrowState.Inactive;
        m_pm.DisableUserInput(false);
        Time.timeScale = m_normalTimeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void CancelAim()
    {
        GetComponent<VisualizeTrajectory>().RemoveVisualDots(); //vllt auch in deactivate hook?
        if (m_pickedUpObject != null)
        {
            m_pickedUpObject.GetComponent<ThrowableObject>().Drop();
            m_pickedUpObject = null;
        }
        DeactivateThrow();
    }
}
