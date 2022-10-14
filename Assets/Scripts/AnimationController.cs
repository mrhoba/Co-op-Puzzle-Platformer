using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator;

    public void PlayAnimation(string animationName)
    {
        animator.SetBool(animationName, true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
