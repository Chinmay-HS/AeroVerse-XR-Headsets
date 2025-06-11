using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TurboFanAnimationController : MonoBehaviour
{
    PlayableDirector director;
    void Start()
    {
        director = gameObject.GetComponent<PlayableDirector>();
        director.Play();
    }
}
