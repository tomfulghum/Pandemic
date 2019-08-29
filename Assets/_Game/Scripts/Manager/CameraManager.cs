using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private AnimationCurve m_impactScreenShakeAmplitude = default;

    //******************//
    //    Properties    //
    //******************//

    public static CameraManager Instance { get; private set; }

    //**********************//
    //    Private Fields    //
    //**********************//

    private CinemachineBrain m_brain;
    private CinemachineVirtualCamera m_activeCamera;
    private CinemachineBasicMultiChannelPerlin m_activeCameraNoise;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }

        m_brain = GetComponent<CinemachineBrain>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CinemachineVirtualCamera activeCamera = (CinemachineVirtualCamera)m_brain.ActiveVirtualCamera;
        if (activeCamera != m_activeCamera) {
            m_activeCamera = activeCamera;
            m_activeCameraNoise = m_activeCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator ImpactScreenShakeCoroutine(float _duration, float _strength)
    {
        m_activeCameraNoise.m_AmplitudeGain = 1f;
        float step = 1f / _duration;
        float progress = 0;

        while (progress < 1f) {
            m_activeCameraNoise.m_AmplitudeGain = m_impactScreenShakeAmplitude.Evaluate(progress) * _strength;
            progress += step * Time.deltaTime;
            yield return null;
        }

        m_activeCameraNoise.m_AmplitudeGain = 0;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void ImpactScreenShake(float _duration, float _strength)
    {
        StartCoroutine(ImpactScreenShakeCoroutine(_duration, _strength));
    }
}
