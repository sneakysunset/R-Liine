using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructBallBarrier : MonoBehaviour
{
    [TextArea]
    public string infoDestructBallBarrier = "N'oublie pas pour le moment de pr�ciser sur qu'elle spawner tu veux que la balle r�apparraise";
    [Tooltip("Le prefab du spawner")]
    public LineBallSpawner spawner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball")||collision.CompareTag("Held"))
        {
            spawner.Spawn();
        }
    }
}
