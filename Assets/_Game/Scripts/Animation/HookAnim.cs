using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//requires component animator
public class HookAnim : MonoBehaviour
{
    bool AnimationPlaying;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AnimationPlaying == false)
            StartCoroutine(PlayAnim()); 
    }


    public IEnumerator PlayAnim()
    {
        AnimationPlaying = true;
        int RandomWaitTime = Random.Range(3, 9); //größere Varianz damit nicht zuviele HookPoints gleichzeitig blingen         
        yield return new WaitForSeconds(RandomWaitTime);
        anim.SetTrigger("Bling"); //Trigger erst hier unten damit nicht jeder bling zu start der szene gleichzeitig getriggert wird
        AnimationPlaying = false;
    }
}
