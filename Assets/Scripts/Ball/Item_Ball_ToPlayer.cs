using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Ball_ToPlayer : Item_Ball
{
    Player[] players;
    public Transform target;
    public bool flying;
    private Player pl;
    float ogGravity;
    public float speed = 5;

    public override void Awake()
    {
        base.Awake();
        players = new Player[2];
        players = FindObjectsOfType<Player>();
        ogGravity = rb.gravityScale;    
        throwPreview = false;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (flying)
        {
            rb.velocity = (target.position - transform.position).normalized * speed;
        }
    }

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);
        flying = false;
        target = null;
        rb.gravityScale = ogGravity;
        pl = player;
    }

    public override void ThrowStarted(float throwStrength, Player player)
    {
        setTagsLayers("Ball", "Ball", 7);

        player.throwing = false;
        player.canMove = true;
        player.canJump = true;
        flying = true;
        Physics2D.IgnoreCollision(player.coll, col, true);

        tP.pointFolder.gameObject.SetActive(false);
        tP._line.positionCount = 1;
        FMODUnity.RuntimeManager.PlayOneShot("event:/MouvementCharacter/Throw");

        rb.isKinematic = false;
        rb.gravityScale = 0;

        isHeld = false;
        player.heldItem = null;

        if (player != players[0]) target = player.transform;
        else target = players[1].transform;
    }

    public override void ThrowHeld(float throwStrength, Player player){}

    public override void CancelThrow(){}

    public override void ThrowRelease(float throwStrength, Player player)
    {

    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (flying)
        {
            flying = false;
            target = null;
            Physics2D.IgnoreCollision(pl.coll, lC.edgeC, false);
            rb.gravityScale = ogGravity;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (flying)
        {
            Physics2D.IgnoreCollision(pl.coll, lC.edgeC, false);
            rb.gravityScale = ogGravity;
        }
    }
}
