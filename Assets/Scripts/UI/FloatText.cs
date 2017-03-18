using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatText : MonoBehaviour {

    [SerializeField]
    Text popupText;

    [SerializeField]
    Animator anim;

    [SerializeField]
    Vector2 offset = new Vector2(0, 0.5f);

    public void ShowText(Transform target, string text)
    {
        transform.position = FloatTextManager.GetCanvasPos(target.position) + offset;
        popupText.text = text;
        gameObject.SetActive(true);
        StartCoroutine(RunAnimation());
    }

    IEnumerator<WaitForSeconds> RunAnimation()
    {
        //Debug.Log("Displaying float text");
        anim.Play(0);
        AnimatorClipInfo info = anim.GetCurrentAnimatorClipInfo(0)[0];

        yield return new WaitForSeconds(info.clip.length);
        anim.Stop();
        gameObject.SetActive(false);
        FloatTextManager.AddToPool(this);  
    }
}
