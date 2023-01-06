using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ThrowPreview : MonoBehaviour
{
    [SerializeField] Transform SolidesParent;
    private Scene _simulatedScene;
    private PhysicsScene2D _physicsScene;
    public GameObject BallPrefab;
    public LineRenderer _line;


    public void Sim(Vector2 velocity)
    {
        Vector2[] vector2s = trajArray(GetComponent<Rigidbody2D>(), transform.position, velocity, _maxPhysicsFrameIterations);
        _line.positionCount = _maxPhysicsFrameIterations;
        Vector3[] vec = new Vector3[_maxPhysicsFrameIterations];
        for (int i = 0; i < _maxPhysicsFrameIterations; i++)
        {
            vec[i] = vector2s[i];
        }
        _line.SetPositions(vec);
    }

    Vector2[] trajArray(Rigidbody2D rb, Vector2 pos, Vector2 velocity, int steps)
    {
        Vector2[] results =  new Vector2[steps];

        float timestep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * rb.gravityScale * timestep * timestep;

        float drag = 1 - timestep * rb.drag;
        Vector2 movestep = velocity* timestep;

        for (int i = 0; i < steps; i++)
        {
            movestep += gravityAccel;
            movestep *= drag;
            pos += movestep;
            results[i] = pos;
        }
        return results;
    }

    #region Not Used
    void CreatePhysicsScene()
    {
        _simulatedScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        _physicsScene = _simulatedScene.GetPhysicsScene2D();

        foreach(Transform obj in SolidesParent)
        {
            var ghostObj =  Instantiate(obj.gameObject, obj.position, obj.rotation);
            ghostObj.GetComponentInChildren<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostObj, _simulatedScene);  
        }
    }
    public int _maxPhysicsFrameIterations = 100;
    public void SimulateTrajectory(float throwStrength, Vector2 direction)
    {
        print(1);
        var ghostObj = Instantiate(this.gameObject, transform.position, transform.rotation);
        SceneManager.MoveGameObjectToScene(ghostObj, _simulatedScene);

        ghostObj.GetComponent<LineCreator>().enabled = false;
        ghostObj.GetComponent<BallBehavior>().enabled = false;

        ghostObj.GetComponent<Rigidbody2D>().AddForce(direction * throwStrength, ForceMode2D.Impulse);
        
        _line.positionCount = _maxPhysicsFrameIterations;
        for (int i = 0; i < _maxPhysicsFrameIterations; i++)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            _line.SetPosition(i, ghostObj.transform.position);
        }

        Destroy(ghostObj);
    }
    #endregion
}
