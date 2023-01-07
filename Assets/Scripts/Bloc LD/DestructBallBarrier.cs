using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructBallBarrier : MonoBehaviour
{
    [TextArea]
    public string infoDestructBallBarrier = "N'oublie pas pour le moment de pr�ciser sur qu'elle spawner tu veux que la balle r�apparraise";
    [Tooltip("Le prefab du spawner")]
    public LineBallSpawner spawner;

    //Lors de la collision avec la balle lance la m�thode du script spawner (� r�f�rencer dans l'inspecteur).
    //Le script spawner d�truit la balle puis la r�instancie � sa position.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball")||collision.CompareTag("Held"))
        {
            spawner.Spawn();
        }
    }
}
