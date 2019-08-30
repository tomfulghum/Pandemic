using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGroupFader : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private CanvasGroup canvasGroup = default;

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator FadeCoroutine(float _target, float _duration)
    {
        float delta = _target - canvasGroup.alpha;
        float step = delta / _duration;

        float endTime = Time.time + _duration;

        while (Time.time < endTime) {
            float change = step * Time.deltaTime;
            canvasGroup.alpha += change;

            yield return null;
        }

        canvasGroup.alpha = _target;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void FadeOut(float _duration)
    {
        StartCoroutine(FadeCoroutine(0f, _duration));
    }

    public void FadeIn(float _duration)
    {
        StartCoroutine(FadeCoroutine(1f, _duration));
    }
}
