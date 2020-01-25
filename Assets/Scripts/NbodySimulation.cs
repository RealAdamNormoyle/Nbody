using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class NbodySimulation : MonoBehaviour
{
    const double G = 6.67408;
    const double YEAR = 3.154;
    public static NbodySimulation Instance;
    public bool globalHighlight = false;
    public int BodyBufferSize = 16 + 8 * sizeof(float) + 4 * sizeof(int);

    [SerializeField] Transform m_bodyContainer;
    [SerializeField] GameObject m_sunPrefab;
    [SerializeField] GameObject m_planetPrefab;
    [SerializeField] ComputeShader m_compute;
    [SerializeField] Transform m_listContents;
    [SerializeField] GameObject m_listItemPrefab;
    [SerializeField] Text m_yearText;
    [SerializeField] Text m_bodiesText;
    [SerializeField] Text m_planetsText;
    [SerializeField] Text m_starsText;
    [SerializeField] Text m_blackHolesText;


    public float timeStep = 1f;
    public Body[] m_bodiesList;
    public float[] m_massList;
    public int[] m_activeList;
    public Vector3[] m_positionList;
    public Vector3[] m_accelerationList;
    public Vector3[] m_velocityList;

    ComputeBuffer activeBuffer;
    ComputeBuffer massBuffer;
    ComputeBuffer positionBuffer;
    ComputeBuffer accelerationBuffer;
    ComputeBuffer velocityBuffer;
    ComputeBuffer testDataBuffer;
    public float[] m_testData;


    Body m_trackedBody;
    double m_years = 0;
    public bool isActive;

    enum ObjectType
    {
        Sun,
        Planet
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        StartCoroutine(StartUp());

    }

    IEnumerator StartUp()
    {
        yield return new WaitForSecondsRealtime(5);
        Setup();
    }

    public void Setup()
    {

        int size = 500;

        m_bodiesList = new Body[size];
        m_massList = new float[size];
        m_activeList = new int[size];
        m_accelerationList = new Vector3[size];
        m_positionList = new Vector3[size];
        m_velocityList = new Vector3[size];

        activeBuffer = new ComputeBuffer(size, 8);
        massBuffer = new ComputeBuffer(size, 4);
        positionBuffer = new ComputeBuffer(size, (4 * 3));
        accelerationBuffer = new ComputeBuffer(size, (4 * 3));
        velocityBuffer = new ComputeBuffer(size, (4 * 3));
        int c = m_compute.FindKernel("ComputeForces");
        m_compute.SetBuffer(c, "velocitys", velocityBuffer);
        m_compute.SetBuffer(c, "accelerations", accelerationBuffer);
        m_compute.SetBuffer(c, "positions", positionBuffer);
        m_compute.SetBuffer(c, "actives", activeBuffer);
        m_compute.SetBuffer(c, "masses", massBuffer);

        for (int i = 0; i < size; i++)
        {
            SpawnObject(ObjectType.Planet, i);
            var item = Instantiate(m_listItemPrefab, m_listContents);
            item.GetComponent<BodyListItem>().Setup(m_bodiesList[i], i);
        }

        positionBuffer.SetData(m_positionList);
        accelerationBuffer.SetData(m_accelerationList);
        velocityBuffer.SetData(m_velocityList);
        massBuffer.SetData(m_massList);
        m_compute.SetFloat("maxBodies", size);


        isActive = true;
    }

    public void TrackNewBody(Body b)
    {
        m_trackedBody = b;
    }

    public void RemoveBody(Body b,int id)
    {
        //m_bodies.Remove(b);
        NbodySimulation.Instance.m_activeList[id] = 0;
    }

    void SpawnObject(ObjectType t,int i)
    {
        switch (t)
        {
            case ObjectType.Sun:
                GameObject sun = Instantiate(m_sunPrefab, m_bodyContainer);
                var s_body = new Body();
                NbodySimulation.Instance.m_massList[i] = 100;
                NbodySimulation.Instance.m_positionList[i] = Random.insideUnitSphere * 1;
                sun.transform.position = NbodySimulation.Instance.m_positionList[i];
                NbodySimulation.Instance.m_activeList[i] = 1;
                m_bodiesList[i] = s_body;
                sun.GetComponent<NBody>().SetBody(s_body, i);
                break;
            case ObjectType.Planet:
                GameObject planet = Instantiate(m_planetPrefab, m_bodyContainer);
                var p_body = new Body();
                NbodySimulation.Instance.m_massList[i] = Random.Range(50f,200f);
                planet.transform.localScale = Vector3.one * (float)NbodySimulation.Instance.m_massList[i] / 100;
                Vector3 p = Random.onUnitSphere;
                p.y = Random.Range(-0.01f, 0.01f);
                NbodySimulation.Instance.m_positionList[i] = ((p) * Random.Range(1500f, 2500f));
                planet.transform.position = NbodySimulation.Instance.m_positionList[i];
                planet.transform.LookAt(Vector3.zero, Vector3.up);
                NbodySimulation.Instance.m_velocityList[i] = planet.transform.right;
                m_bodiesList[i] = p_body;
                planet.GetComponent<NBody>().SetBody(p_body, i);
                NbodySimulation.Instance.m_activeList[i] = 1;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;


        //int kernel = m_compute.FindKernel("ComputeForces");
        m_compute.SetFloat("deltaTime", Time.deltaTime * timeStep);
        //m_compute.SetBuffer(kernel, "testData", testDataBuffer);
        m_compute.Dispatch(m_compute.FindKernel("ComputeForces"), m_bodiesList.Length / 256, 1, 1);


        int suns = 0;
        int planets = 0;
        int blackholes = 0;
        //Parallel.For(0, m_bodiesList.Length, delegate (int id) {

        //    switch (m_bodiesList[id].type)
        //    {
        //        case 0:
        //            planets++;
        //            break;
        //        case 1:
        //            suns++;
        //            break;
        //        case 2:
        //            blackholes++;
        //            break;
        //    }

        //});
        m_years += (Time.deltaTime / YEAR);

        m_yearText.text = $"{m_years.ToString("F2")} Years";
        m_bodiesText.text = $"Total : {m_bodiesList.Length}";
        m_planetsText.text = $"Planets : {planets}";
        m_starsText.text = $"Stars : {suns}";
        m_blackHolesText.text = $"Black Holes : {blackholes}";
    }

    private void OnRenderObject()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            return;
        }

        if (!isActive)
            return;

        //activeBuffer.GetData(m_activeList);
        //velocityBuffer.GetData(m_velocityList);
        //massBuffer.GetData(m_massList);
        positionBuffer.GetData(m_positionList);
        //accelerationBuffer.GetData(m_accelerationList);

        if (m_trackedBody != null)
        {

        }


    }

    //double CalculateForce(Body b1,Body b2)
    //{
    //    //-(GMm / r ^ 3) * r
    //    return -((G * b1.mass * b2.mass) / Mathf.Pow(Vector3.Distance(b1.position, b2.position),3)) * Vector3.Distance(b1.position, b2.position);
    //}


    private void OnDestroy()
    {
        // must deallocate here
        activeBuffer.Release();
        massBuffer.Release();
        positionBuffer.Release();
        accelerationBuffer.Release();
    }

}



[System.Serializable]
public class Body
{
    public double timeStep;
    public int dirty;
    public int type;
    public int highlight;
}

public enum BodyTypes
{
    Asteroid,
    Star,
    BlackHole
}

