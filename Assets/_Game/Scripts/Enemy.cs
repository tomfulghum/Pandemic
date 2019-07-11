using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    bool CurrentlyHit;
    bool KnockBackActive;
    int colorChangeCounter;
    Color originalColor;
    // Start is called before the first frame update
    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentlyHit)
        {
            if (KnockBackActive == false)
            {
                StartCoroutine(KnockBack(10, Vector2.zero, 5));
            }
            else
            {
                colorChangeCounter++;
                if (colorChangeCounter % 5 == 0)
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = originalColor;
                }
            }
        }
    }

    public void GetHit()
    {
        CurrentlyHit = true;
        //EnemyFreeze = true
    }

    IEnumerator KnockBack(float _repetissions, Vector2 _knockBackDirection, float _knockBackStrength) //deactivate layer collission?
    {
        Physics2D.IgnoreLayerCollision(10, 11, true);
        KnockBackActive = true;
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 3)/100;
            if(test < 0)
            {
                test = 0;
            }
            Debug.Log(test);
            transform.position = new Vector2(transform.position.x + 0.2f * test, transform.position.y);
            yield return new WaitForSeconds(0.03f);
        }
        KnockBackActive = false;
        CurrentlyHit = false;
        GetComponent<SpriteRenderer>().color = originalColor;
        colorChangeCounter = 0;
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }
}
