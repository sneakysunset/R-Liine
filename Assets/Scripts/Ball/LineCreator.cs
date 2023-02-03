using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineCreator : MonoBehaviour
{
    #region Variables
    private float[] pointArray;
    public List<Point> pointList = new List<Point>();
    private Color col;
    private CharacterController2D charC;
    Transform lineFolder;
    //[HideInInspector] public LineRenderer lineR;
    [HideInInspector] public EdgeCollider2D edgeC;
    [HideInInspector] public Transform lineT;

    private CharacterController2D.Team pType;
    Collider coll;
    [Header("Components")]
    [Space(5)]
    [HideInInspector] public GameObject linePrefab;
    Vector2 ogPos;
    int prevUpdatedIndex;
    public GameObject ballPrefab;
    private MeshFilter meshF;


    [Space(10)]
    [Header("Line Variables")]
    [Space(5)]
    [Range(.001f, 1)] public float lineResolution = .5f;
    public float lineBeginningX = -13f;
    public float lineEndX = 13f;
    public float width = .3f;
    public float lineYOffSet = 0;
    public bool fond;
    private bool flag;
    [Space(10)]
    [Header("Refresh Variables")]
    [Space(5)]
    public float threshHoldToUpdatePoints;
    public float updateSoundFrequency;
    public AnimationCurve updateAnim;
    public float updateAnimSpeed;
    public float fallTimer;
    public float fallSpeed;
    #endregion

    IEnumerator fixDeMerdeSpawnLigne()
    {
        yield return new WaitForEndOfFrame();
        if (pointList.Count > 0) 
        pointList.Clear();
        var a = transform.position.x - Mathf.FloorToInt(transform.position.x);
        var b = a - (a % lineResolution);
        var posX = Mathf.FloorToInt(transform.position.x) + b + lineResolution;
        pointList.Add(new Point(new Vector2(posX, transform.position.y))) ;
        //lineR.positionCount = 0;
    }

    private void Start()
    {
        lineFolder = GameObject.FindGameObjectWithTag("LineFolder").transform;
        pointArray = Utils_Points.GeneratePointArray(pointArray, lineBeginningX, lineEndX, lineResolution);
        if (GetComponent<CharacterController2D>())
        {
            charC = GetComponent<CharacterController2D>();
            col = charC.col;
            pType = charC.playerType;
        }
        else pType = CharacterController2D.Team.Ball;
        ogPos = transform.position;
        var firstPoint = new Vector2(Utils_Points.closestPoint(pointArray, transform.position.x), transform.position.y);
        pointList.Add(new Point(firstPoint));
        InstantiateLine();
        StartCoroutine(fixDeMerdeSpawnLigne());
    }
    private void Update()
    {
        if (fond)
        {
            foreach (Point point in pointList)
            {
                point.Fond(fallSpeed);
            }
        }

        if(fond && !flag)
        {
            foreach (Point point in pointList) point.TimerTrigger(true, fallTimer);
            flag = true;
        }
        else if(!fond && flag)
        {
            foreach (Point point in pointList) point.TimerTrigger(false, fallTimer);
            flag = false;
        }
    }

    //Au start cr� la ligne et prend des r�f�rences du lineRenderer, du edgeCollider et du transform. Change aussi la couleur de la ligne son nom et son layer.
    private void InstantiateLine()
    {
        lineT = Instantiate(linePrefab, lineFolder).transform;
        //lineR = lineT.GetComponentInChildren<LineRenderer>();
        edgeC = lineT.GetComponentInChildren<EdgeCollider2D>();
        meshF = lineT.GetComponentInChildren<MeshFilter>();
        //lineR.positionCount = 0;
        //lineR.material.color = col;
        //lineT.name = "Mesh " + pType.ToString() + " Off";
        edgeC.gameObject.layer = 6;
        if (pType != CharacterController2D.Team.Ball)
            charC.meshObj = lineT.gameObject;
    }

    //Ordonne la liste de point puis invoque la fonction qui ajoute et actualise des points.
    //R�ordonne la liste de points pour que les nouveaux points potentiels soient rang� dans le bon ordre.
    //Remplace la liste de points actuel du lineRenderer par la liste de point pointLine.
    //Lance la Coroutine "afterPhysics" qui actualise le edgeCollider apr�s la simulation physique.
    public void LineUpdater()
    {
        // var list = pointList;
        // list = pointList.OrderBy(v => v.x).ToList();
        // pointList = list;
        if (pointList.Count == 0) return;
        if (!UpdatePointList()) return;

        var list = pointList.OrderBy(v => v.pos.x).ToList();
        pointList = list;

        if (list.Count < 4 || edgeC.gameObject.layer == 10) return;

        List<Vector2> vec2 = AddMediumPoints();

        //lineR.positionCount = pointList.Count;
        Vector3[] vector3s = new Vector3[vec2.Count];
        for (int i = 0; i < vector3s.Length; i++)
        {
            vector3s[i] = vec2[i];
        }

        Mesh m = new Mesh();
        m.name = "trailMesh";

        Utils_Mesh.UpdateMeshVertices(vec2, width, m);
        Utils_Mesh.UpdateMeshTriangles(vec2.Count, m);
        m.MarkDynamic();
        m.Optimize();
        m.OptimizeReorderVertexBuffer();
        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();
        meshF.mesh = m;

        //lineR.SetPositions(vector3s);
        StartCoroutine(afterPhysics(vec2));
    }

    //Etape interm�diaire dans laquel on d�cide si on ajoute un/des point(s) ou actualise un/des point(s) de la liste lors de cette frame physique.
    //Si la balle est en dehors de la liste de points avec une distance d'au moins lineResolution (variable d'espacement des points) alors on ajoute un/des point(s) (condition1).
    //Si la balle est entre le premier et dernier point de la liste (condition2) et n'a pas la m�me position que la frame pr�c�dente on actualise un/des point(s) (condition3).
    //Si la balle n'a pas chang� de position depuis la derni�re frame alors la m�thode retourne False et le lineRenderer et le edgeCollider ne sont pas actualis� cette frame physique (!condition1 && !condition3).
    public bool UpdatePointList()
    {
        Vector2 pPos = new Vector2(transform.position.x, transform.position.y);
        bool condition1 = pointList[0].pos.x - pPos.x > lineResolution || pPos.x - pointList[pointList.Count - 1].pos.x > lineResolution;
        bool condition2 = pointList[0].pos.x - pPos.x < lineResolution && pPos.x - pointList[pointList.Count - 1].pos.x < lineResolution;
        bool condition3 = transform.position.x != prevUpdatedIndex;
        if (condition1)
        {
            AddPoint();
            return true;
        }
        else if (condition2 && condition3)
        {
            UpdatePoint();
            return true;
        }
        else return false;
    }



    //M�thode qui sert � rajouter un/des point(s).
    void AddPoint()
    {
        Vector2 pPos = new Vector2(transform.position.x, transform.position.y);
        prevUpdatedIndex = -1;
        float posX = 0;
        int numOfAdded = 0;

        //Ce calcule sert � trouver le point appartenant � la liste le plus proche de la balle sur l'axe x.
        //'a' contient les chiffres apr�s la virgule de la position de la balle sur x. 
        //'b' contient le coefficient de "lineResolution" qui permet d'avoir la valeur de 'a' arrondie � d�faut � un multiple de lineResolution.
        var a = pPos.x - Mathf.FloorToInt(pPos.x);
        var b = a - (a % lineResolution);

        //Quand la balle est � gauche de la liste posX la position de la balle sur x arrondi � l'exc�s au multiple de "lineResolution".
        //Quand la balle est � gauche de la liste posX la position de la balle sur x arrondi au d�faut au multiple de "lineResolution".
        if (pointList[0].pos.x - pPos.x > lineResolution)
        {
            posX = Mathf.FloorToInt(pPos.x) + b + lineResolution;

            //Afin que tous les points de la lignes aient le m�me �cartement et que la r�solution de la ligne soit uniforme /n
            //il faut rajouter � la liste de point non pas seulement le point le plus proche de la balle /n
            //mais aussi tous les points multiples de "lineResolution" s�parant la balle du point actuel de la liste le plus proche.
            //Pour ce faire on ajoute des points de mani�re it�rative de la position la plus proche de la balle arondie, � la position /n
            //la plus proche de la balle sur l'axe x appartenant � la liste avec une incr�mentation de "lineResolution".
            for (float i = posX; i < pointList[0].pos.x; i += lineResolution)
            {
                //Afin que la courbe de la ligne soit r�aliste et smooth on interpole la position sur l'axe y de chaque point rajout� /n
                //entre la position sur y de la balle et la position sur y du point de la liste le plus proche de la balle sur l'axe x.
                float posY = Mathf.Lerp(pPos.y, pointList[0].pos.y, numOfAdded / ((pointList[0].pos.x - pPos.x) / lineResolution));
                pointList.Add(new Point(new Vector2(i, posY)));
                if (fond)
                {
                    pointList[pointList.Count - 1].TimerTrigger(true, fallTimer);
                }
                else if (!fond)
                {
                    pointList[pointList.Count - 1].TimerTrigger(false, fallTimer);
                }
                numOfAdded++;
            }

        }
        else
        {
            posX = Mathf.FloorToInt(pPos.x) + b;

            for (float i = posX; i > pointList[pointList.Count - 1].pos.x; i -= lineResolution)
            {
                float posY = Mathf.Lerp(pPos.y, pointList[0].pos.y, numOfAdded / ((pPos.x - pointList[pointList.Count - 1].pos.x) / lineResolution));
                pointList.Add(new Point(new Vector2(i, posY)));
                if (fond)
                {
                    pointList[pointList.Count - 1].TimerTrigger(true, fallTimer);
                }
                else if (!fond)
                {
                    pointList[pointList.Count - 1].TimerTrigger(false, fallTimer);
                }
                numOfAdded++;
            }
        }
    }

    //M�thode qui sert � actualiser la position de points sur l'axe y.
    void UpdatePoint()
    {
        Vector2 pPos = new Vector2(transform.position.x, transform.position.y);

        float posX = Mathf.FloorToInt(pPos.x);

        float curDistance = 100000;
        int closestIndex = 10000;
        //Cette it�ration sert � trouver le point le plus proche de la balle appartenant � la liste.
        for (int i = 0; i < pointList.Count; i++)
        {
            if (Mathf.Abs(pointList[i].pos.x - pPos.x) < curDistance)
            {
                closestIndex = i;
                curDistance = Mathf.Abs(pointList[i].pos.x - pPos.x);
            }
        }


        Vector2 newPos = new Vector2(pointList[closestIndex].pos.x, pPos.y);
        pointList[closestIndex].pos = newPos;
        if (fond)
        {
            pointList[closestIndex].TimerTrigger(true, 1);
        }
        else if (!fond)
        {
            pointList[closestIndex].TimerTrigger(false, 1);
        }
        //Nous suivons ici un proc�d� similaire � celui de la m�thode AddPoint sauf que l'it�ration se fait entre la position /n
        //la plus proche de la balle et la position la plus proche de la balle � la frame physique pr�c�dente.
        //L'incr�mentation ne se fait aussi pas avec "lineResolution" mais avec les index s�parant les 2 points �voqu�s au-dessus.
        //Si la position pr�c�dente est � droite de la position actuelle on effectue un d�cr�mentation vers la position actuelle.
        if (prevUpdatedIndex != -1)
        {
            if (closestIndex - prevUpdatedIndex < 0)
            {
                for (int i = closestIndex; i < prevUpdatedIndex; i++)
                {
                    float posY = Mathf.Lerp(pointList[closestIndex].pos.y, pointList[prevUpdatedIndex].pos.y, (Mathf.Abs(i) - Mathf.Abs(closestIndex)) / (Mathf.Abs(prevUpdatedIndex - closestIndex)));
                    pointList[i].pos = new Vector2(pointList[i].pos.x, posY);
                    if (fond)
                    {
                        pointList[i].TimerTrigger(true, fallTimer);
                    }
                    else if (!fond)
                    {
                        pointList[i].TimerTrigger(false, fallTimer);
                    }
                }
            }
            else
            {
                for (int i = closestIndex; i > prevUpdatedIndex; i--)
                {
                    float posY = Mathf.Lerp(pointList[prevUpdatedIndex].pos.y, pointList[closestIndex].pos.y, (Mathf.Abs(i) - Mathf.Abs(prevUpdatedIndex)) / (Mathf.Abs(closestIndex - prevUpdatedIndex)));
                    pointList[i].pos = new Vector2(pointList[i].pos.x, posY);
                    if (fond)
                    {
                        pointList[i].TimerTrigger(true, fallTimer);
                    }
                    else if (!fond)
                    {
                        pointList[i].TimerTrigger(false, fallTimer);
                    }
                }
            }
        }
        else
        {
            pointList[closestIndex].pos = newPos;
            if (fond)
            {
                pointList[closestIndex].TimerTrigger(true, fallTimer);
            }
            else if (!fond)
            {
                pointList[closestIndex].TimerTrigger(false, fallTimer);
            }
        }

        prevUpdatedIndex = closestIndex;
    }

    //A cause de l'ordre d'execution des events physiques de Unity on doit actualiser le collider apr�s le yuekd WaitForFixedUpdate.
    //https://docs.unity3d.com/Manual/ExecutionOrder.html
    IEnumerator afterPhysics(List<Vector2> vec2s)
    {
        yield return new WaitForFixedUpdate();
        edgeC.SetPoints(vec2s);
    }

    //Lorsque la balle est d�truite la ligne associ�e est aussi d�truite.
    private void OnDestroy()
    {
        if (lineT)
            Destroy(lineT.gameObject);
    }

    public List<Vector2> AddMediumPoints()
    {
        List<Vector2> vec2 = new List<Vector2>();
        for (int i = 0; i < pointList.Count; i++) vec2.Add(pointList[i].pos);
        for (int i = 0; i < vec2.Count - 1; i++)
        {
            bool taskDone = false;
            while (!taskDone)
            {
                if (Vector2.Distance(vec2[i], vec2[i + 1]) > lineResolution * 2)
                {
                    vec2.Insert(i + 1, vec2[i] + (vec2[i + 1] - vec2[i]).normalized * lineResolution);
                    i++;
                }
                else taskDone = true;
            }

        }
        return vec2;
    }
}
