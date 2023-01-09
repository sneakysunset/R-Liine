using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LineSound : MonoBehaviour
{
    private LineCreator lineC;
    [HideInInspector] public FMOD.Studio.EventInstance sound;
    public float soundUpdateTimer;
    WaitForSeconds waiter;
    private float maxHeight;
    private float minHeight;
    public GameObject visualPrefab;
    private Transform currentVisual;
    IEnumerator soundEnum;
    public bool pingpong;
    public float startTimer;

    private void Start()
    {
        waiter = new WaitForSeconds(soundUpdateTimer);
    }

    //Event pour l'instant activ� manuellement avec un bouton.
    //Il lance la m�canique principale sonore (synth�tiseur sonore avec pitch d�pendant de la courbe cr��e par la ligne).
    public void PlaySound()
    {
        lineC = FindObjectOfType<LineCreator>();

        //Si la m�canique est d�ja activ� on arr�te l'instance de son actuelle. Sinon on instancie la visualisation du lecteur de la courbe.
        if (IsPlaying())
        {
            sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sound.release();
            currentVisual.position = lineC.pointList[0];
            StopCoroutine(soundEnum);
            soundEnum = null;
        }
        else
        {
            currentVisual = Instantiate(visualPrefab, lineC.pointList[0], Quaternion.identity).transform;
        }

        soundEnum = SoundControl();
        
        //Cr�ation de l'instance du son.
        sound = FMODUnity.RuntimeManager.CreateInstance("event:/MouvementCorde/LineSound");
        sound.start();
        //M�thode actuellement non dynamique. Elle devrait permettre de rep�rer la hauteur du mur au dessus et en dessous du joueur. Pour l'instant elle r�cup�re des valeurs constantes.
        GetMaxHeight();
        //Coroutine qui fait fonctionner le son.
        StartCoroutine(soundEnum);
    }

    //M�thode qui retourne si le son de la m�canique sonore est entrain de jouer.
    bool IsPlaying()
    {
        FMOD.Studio.PLAYBACK_STATE state;
        sound.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    //M�thode actuellement non dynamique. Elle devrait permettre de rep�rer la hauteur du mur au dessus et en dessous du joueur. Pour l'instant elle r�cup�re des valeurs constantes.
    void GetMaxHeight()
    {
        RaycastHit2D hitTop = Physics2D.Raycast((Vector2)lineC.transform.position, Vector2.up, 1000, 15); 
        RaycastHit2D hitBottom = Physics2D.Raycast((Vector2)lineC.transform.position, Vector2.down, 1000, 15);

        //print(hitTop.transform.name + " " + hitTop.point.y);
        //print(hitBottom.transform.name + " " + hitBottom.point.y);
        //maxHeight = hitTop.point.y - hitBottom.point.y;
        //minHeight = hitBottom.point.y;
        maxHeight = 47;
        minHeight = 0;
    }

    IEnumerator SoundControl()
    {
        //Timer servant � attendre la fin du fade in de la piste sonore jou�e.
        yield return new WaitForSeconds(startTimer);
        int i = 0;
        
        //De mani�re it�rative on parcours les points de la ligne de gauche � droite. A chaque it�ration le param�tre locale de FMOD "Pitch" r�cup�re la valeure de la position sur l'axe y du point actuel.
        while(i < lineC.pointList.Count - 1)
        {
            i++;
            sound.setParameterByName("Pitch", (lineC.pointList[i].y - minHeight)/ maxHeight, true);
            currentVisual.position = lineC.pointList[i];

            yield return waiter;
        }
        //M�me proc�d� dans le sens contraire.
        while (i > 0)
        {
            i--;
            sound.setParameterByName("Pitch", (lineC.pointList[i].y - minHeight) / maxHeight, true);
            currentVisual.position = lineC.pointList[i];

            yield return waiter;
        }

        //Si le bool�en est activ� cette coroutine se r�p�te � l'infini jusqu'� ce que la sc�ne soit chang�.
        if (!pingpong)
        {
            sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sound.release();
            Destroy(currentVisual.gameObject);
            StopCoroutine(soundEnum);
            soundEnum = null;
        }
        else
        {
            PingPongSoundControl();
        }
    }

    //M�thode permettant de r�p�ter la m�canique sonore � l'infini.
    void PingPongSoundControl()
    {
        startTimer = 0;
        StartCoroutine(SoundControl());
    }

   
}
