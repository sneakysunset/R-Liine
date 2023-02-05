using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    public bool activated;
    [HideInInspector] public KeyChain keyChain;
    Color ogCol;

    private void Start()
    {
        ogCol = GetComponentInChildren<SpriteRenderer>().color;
    }

    //Quand la ligne rentre dans la zone de l'interrupteur active le son d'activation et change la couleur de l'interrupteur.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LineCollider"))
        {
            activated = true;
            FMODUnity.RuntimeManager.PlayOneShot("event:/BlockLd/SwitchOn");
            keyChain.KeyTriggered();
            GetComponentInChildren<SpriteRenderer>().color = Color.blue;
        }
    }

    //Tant que la ligne reste dans la zone de l'interrupteur la cl� reste activ�e.
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("LineCollider"))
        {
            activated = true;
        }
    }
    
    //Quand la ligne quitte la zone de l'interrupteur active le son de d�sactivation, change la couleur de l'interrupteur et d�sactive la cl�.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("LineCollider"))
        {
            activated = false;
            FMODUnity.RuntimeManager.PlayOneShot("event:/BlockLd/SwitchOff");

            keyChain.KeyTriggered();
            GetComponentInChildren<SpriteRenderer>().color = ogCol;
        }
    }
}
