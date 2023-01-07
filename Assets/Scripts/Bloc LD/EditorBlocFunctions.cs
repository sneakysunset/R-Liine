using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script ne s'excutant qu'en editor et pas en playmode.
[ExecuteInEditMode]
public class EditorBlocFunctions : MonoBehaviour
{
    public bool freeMove;
    public bool freeScale;
    private Vector3 previousPos;
    private Vector3 previousScale;

    //M�thode permettant d'emp�cher le d�placement et le scaling libre de l'objet quand le bool�en associ� est d�sactiv�.
    private void LateUpdate()
    {
        if (!freeMove && previousPos != transform.position)
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);

        if (!freeScale && previousScale != transform.localScale)
            transform.localScale = new Vector3(Mathf.RoundToInt(transform.localScale.x ), Mathf.RoundToInt(transform.localScale.y), 1);

        previousPos = transform.position;
        previousScale = transform.localScale;
    }

}
