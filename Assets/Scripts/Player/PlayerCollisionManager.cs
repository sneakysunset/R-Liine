using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionManager : MonoBehaviour
{
    [HideInInspector] public CharacterController2D charC;
    private Rigidbody2D rb;
    [HideInInspector] public GameObject coll;
    Vector2 prevVelocity;
    [HideInInspector] public IEnumerator groundCheckEnum;
    [HideInInspector] public List<Transform> holdableObjects;
    [HideInInspector] public bool holdingBall = false;
    [Range(-1f, 1f)] public float yGroundCheck = -.15f;
    [Range(0f, 1f)] public float yWallJump = .7f;
    [Range(-1f, 1f)] public float lineCollisionDetection = .1f;
    public float bounce = 1.5f;

    private void Start()
    {
        charC = GetComponent<CharacterController2D>();
        rb = GetComponent<Rigidbody2D>();
        coll = transform.Find("Collider").gameObject;
    }
    public bool yo;
    private void FixedUpdate()
    {
        StartCoroutine(WaitForPhysics());
    }

    private void Update()
    {
        if (Physics2D.Raycast((Vector2)transform.position, Vector2.up, 100, LayerMask.NameToLayer("LineCollider"))) yo = true;
        else yo = false;


        if ((charC.moveValue.y >= -1 && charC.moveValue.y < -.85f) || holdingBall || yo)
        {
            coll.layer = LayerMask.NameToLayer("PlayerOff");
        }
        else if (!inline)
        {
            coll.layer = LayerMask.NameToLayer("Player");
        }
    }

    bool enterAgain;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        LineCollisionEnter(collision);
        GroundCheckCollisionEnter(collision);
        CollisionWithPlayer(collision);

        //D�sactive la variable jumping lors de la collision avec un mur walljumpable.
        if (collision.contacts[0].normal.y > -yWallJump && collision.contacts[0].normal.y < yWallJump && collision.gameObject.CompareTag("Jumpable") /*|| collision.gameObject.CompareTag("LineCollider")*/)
        {
            charC.jumping = false;
        }
    }

    //Logique de collision avec la ligne.
    //Si le y de la normale de la collision est compris entre la valeure "yGroundCheck" entr�e en variable publique et cette m�me valeur n�gative, /n
    //on traverse la ligne (changement du layer du joueur vers un layer ne d�tectant pas de collisions avec la ligne).
    //Sinon la ligne arr�te le joueur et les sons de collisions sont lanc�s.
    //Penser � rajouter une variable remplacant "yGroundCheck" dans ce contexte.
    private void LineCollisionEnter(Collision2D collision)
    {
        bool condition1 = collision.gameObject.tag == "LineCollider";
        bool condition2 = collision.contacts[0].normal.y > lineCollisionDetection;

        if (condition1 && !condition2)
        {
            coll.layer = LayerMask.NameToLayer("PlayerOff");
            rb.velocity = -collision.relativeVelocity;
        }
        else if (condition1 && !condition2 && enterAgain)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/MouvementCorde/LineLand");
            enterAgain = false;
        }
        else if (!condition1)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/MouvementCharacter/Land");
        }
    }

    //Si le y de la normal de collision avec le sol est inf�rieur est sup�rieur � la valeur de "yGroundCheck" rentr�e en variable publique rend le bool�en groundCheck true.
    private void GroundCheckCollisionEnter(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > yGroundCheck)
        {
            charC.groundCheck = true;
            if (groundCheckEnum != null)
            {
                StopCoroutine(groundCheckEnum);
                groundCheckEnum = null;
            }
        }
    }
    
    //Si le y de la normale de la collision avec un autre joueur est sup�rieur � 0.65f fait rebondir le joueur et lance le son de rebond du joueur.
    private void CollisionWithPlayer(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player")  && collision.contacts[0].normal.y > .65f)
        {
            rb.AddForce(Vector2.up * bounce, ForceMode2D.Impulse);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Ball/Bounce");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        GroundCheckCollisionStay(collision);
        WallJumpCollisionStay(collision);
    }

    //Si le y de la normal de collision avec le sol est inf�rieur est sup�rieur � la valeur de "yGroundCheck" rentr�e en variable publique rend le bool�en groundCheck true.
    void GroundCheckCollisionStay(Collision2D collision)
    {
        charC.groundCheck = false;
        if (collision.contacts[0].normal.y > yGroundCheck)
        {
            charC.groundCheck = true;
            if (groundCheckEnum != null)
            {
                StopCoroutine(groundCheckEnum);
                groundCheckEnum = null;
            }
        }
    }

    //Tant que le joueur reste coll� � un mur walljumpable, la variable "wallJumpable" du CharacterController2D est �gale � la normale de la collision avec le mur.
    //Le joueur � sa v�locit� sur l'axe y �gale � 0 pour ne pas glisser le long du mur.
    void WallJumpCollisionStay(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > -yWallJump && collision.contacts[0].normal.y < yWallJump && collision.gameObject.CompareTag("Jumpable") /*|| collision.gameObject.CompareTag("LineCollider")*/)
        {
            //rb.velocity = new Vector3(rb.velocity.x, 0);
            charC.wallJumpable = collision.contacts[0].normal.x;
            rb.velocity = new Vector2(0, 0);
        }
    }

    //Quand le joueur sort d'une collision il lance une coroutine qui apr�s un court timer mets le bool�en "groundCheck" en false.
    //Le Vector3 "wallJumpable" est aussi �gal � 0 ce qui d�sactive l'effet de walljump pour le prochain saut.
    private void OnCollisionExit2D(Collision2D collision)
    {
        groundCheckEnum = waitForGroundCheckOff(charC.ghostInputTimer);
        StartCoroutine(groundCheckEnum);
        StartCoroutine(WaitForSeconds(.2f));

        charC.wallJumpable = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Ball"))
        {
            GetBallOnTriggerEnter(other);
        }
    }

    //Si le joueur rentre dans la zone de trigger autour d'une balle elle rejoint la liste des objets attrapables � proximit�.
    void GetBallOnTriggerEnter(Collider2D other)
    {
         other.transform.parent.Find("Highlight").gameObject.SetActive(true);
         holdableObjects.Add(other.transform.parent);
    }
    bool inline;
    //Quand la ligne est dans la zone de trigger � l'int�rieur du joueur, ce dernier n'a pas de collision avec elle.
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "LineCollider")
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerOff");
            inline = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        RemoveBallTriggerExit(other);
        ExitLineTriggerExit(other);
    }

    void RemoveBallTriggerExit(Collider2D other)
    {
        if (other.transform.CompareTag("Ball"))
        {
            other.transform.parent.Find("Highlight").gameObject.SetActive(false);
            holdableObjects.Remove(other.transform.parent);
        }
    }

    //Quand la ligne sort de la zone de trigger � l'int�rieur du joueur et que ce dernier ne tient pas de balle, le joueur recommence � rentrer en collision avec la ligne.
    void ExitLineTriggerExit(Collider2D other)
    {
        bool condition1 = other.tag == "LineCollider";
        if (condition1)
        {
            inline = false;
            coll.layer = LayerMask.NameToLayer("Player");
            if (holdingBall)
            {
                coll.layer = LayerMask.NameToLayer("PlayerOff");
            }
        }
    } 
    
    //Je sais plus honn�tement.
    IEnumerator WaitForSeconds(float timer)
    {
        yield return new WaitForSeconds(timer);
        enterAgain = true;
    }

    //Un petit d�lai avant de d�sactiver le ground check du joueur pour qu'il ait le temps de sauter quand il quitte bri�vement le sol.
    public IEnumerator waitForGroundCheckOff(float timer)
    {
        yield return new WaitForSeconds(timer);
        charC.groundCheck = false;
    }

    Vector3 prevprevVelo;

    public IEnumerator WaitForPhysics()
    {
        yield return new WaitForFixedUpdate();
       // prevprevVelo = prevVelocity;
       // prevVelocity = rb.velocity;
    }



}
