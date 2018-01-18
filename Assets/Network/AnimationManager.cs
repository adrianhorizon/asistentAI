using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    public Animator mixer;
    public AnimationClip[] Animations;
    public bool LoopAnimation;
    // Use this for initialization
    void Start()
    {
        mixer = gameObject.GetComponent<Animator>();
    }

    public void Play(string stateName, float length)
    {
        StartCoroutine("PlayAnim", stateName);
        if (length > 0)
        {
            Invoke("StopAnimation", length);
        }
    }

    IEnumerator PlayAnim(string StateName)
    {
        Debug.Log("Playing Animation: " + StateName);
        while (!mixer.GetCurrentAnimatorStateInfo(0).IsName("idle") || mixer.IsInTransition(0))
        {
            mixer.SetInteger("pathIndex", -1);
            yield return null;
        }

        int index = 0;
        foreach (AnimationClip c in Animations)
        {
            if (c.name.Equals(StateName))
            {
                //Debug.Log("Animation index = " + index);
                break;
            }
            index++;
        }

        float endTime = Animations[index].length;
        //Debug.Log("Animation index length = " + Animations[index].length);
        if (!LoopAnimation)
        {
            Invoke("StopAnimation", endTime);
        }
        mixer.SetInteger("pathIndex", index);
    }

    public void StopAnimation()
    {
        //Debug.Log("Stopped animation");
        StopCoroutine("PlayAnim");
        mixer.SetInteger("pathIndex", -1);
    }
}
