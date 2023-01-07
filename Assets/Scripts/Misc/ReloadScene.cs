using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{
    LineSound lineSound;

    private void Awake()
    {
        lineSound = FindObjectOfType<LineSound>();
    }

    //Relance la sc�ne et arr�te le son de la m�canique principale en cours.
    public void SceneReloader()
    {
        lineSound.sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        lineSound.sound.release();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //En fonction du chiffre rentr� dans l'input Field (de 0 � 3), lance la sc�ne associ�e et arr�te le son de la m�canique principale en cours.
    public void LoadScene(string input)
    {
        int intput = int.Parse(input);
        switch (intput)
        {
            case 0:
                lineSound.sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                lineSound.sound.release();
                SceneManager.LoadScene("SceneTest");
                break;
            case 1:
                lineSound.sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                lineSound.sound.release();
                SceneManager.LoadScene("PlayGround");
                break;
            case 2:
                lineSound.sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                lineSound.sound.release();
                SceneManager.LoadScene("Sound Test");
                break;
            case 3:
                lineSound.sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                lineSound.sound.release();
                SceneManager.LoadScene("LD_MainTest");
                break;
            default:
                break;
        }
    }
}
