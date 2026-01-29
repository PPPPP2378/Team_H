using UnityEngine;

public class ButtonSE : MonoBehaviour
{
    public AudioSource se;

    //ƒ{ƒ^ƒ“‚ğ‰Ÿ‚µ‚½uŠÔ‚ÉSE‚ğÄ¶‚·‚é
    public void PlaySE()
    {
        se.PlayOneShot(se.clip);
    }
}
