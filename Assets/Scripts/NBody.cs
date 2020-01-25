using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NBody : MonoBehaviour
{
    public Body m_body { get { return (id >-1)?NbodySimulation.Instance.m_bodiesList[id]:null; } }
    bool isSun = false;
    bool isBlackHole = false;
    int id = -1;
    Light light;
    TrailRenderer m_lr;
    // Update is called once per frame
    void Update()
    {
        if (id == -1)
            return;

        if (NbodySimulation.Instance.m_activeList[id] == 0)
            return;

        transform.position = NbodySimulation.Instance.m_positionList[id];

        if (light)
            light.intensity = (float)NbodySimulation.Instance.m_massList[id] / 300;

        if (m_lr)
            m_lr.enabled = (m_body.highlight == 1 | NbodySimulation.Instance.globalHighlight);
    }

    internal void SetBody(Body body,int i)
    {
        id = i;
        transform.localScale = Vector3.one * ((float)NbodySimulation.Instance.m_massList[id] / ((isBlackHole)?500f:100f));
        m_body.type = 0;
        m_lr = GetComponent<TrailRenderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var b = collision.gameObject.GetComponent<NBody>();
        if(NbodySimulation.Instance.m_massList[id] > NbodySimulation.Instance.m_massList[b.id])
        {
            NbodySimulation.Instance.m_massList[id] += NbodySimulation.Instance.m_massList[b.id];
            NbodySimulation.Instance.m_velocityList[id] += NbodySimulation.Instance.m_velocityList[b.id] / (float)NbodySimulation.Instance.m_massList[id];

            if(!isBlackHole && !isSun && NbodySimulation.Instance.m_massList[id] > 300 && NbodySimulation.Instance.m_massList[id] < 1500)
            {
                m_body.type = 0;
                transform.localScale = Vector3.one * ((float)NbodySimulation.Instance.m_massList[id] / 100f);
            }

            if (NbodySimulation.Instance.m_massList[id] > 1500)
            {
                var mat = GetComponent<MeshRenderer>(); 
                if(isSun == false)
                {
                    isSun = true;
                    mat.material = new Material(Shader.Find("Unlit/Color"));
                    //mat.material.SetColor("_Color", mat.material.color);
                    light = gameObject.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = 1000000;
                    name = "Star";
                    m_body.type = 1;
                }

                mat.material.color = Color.Lerp(Color.white, Color.yellow, (float)NbodySimulation.Instance.m_massList[id] / 10000);
                transform.localScale = Vector3.one * Mathf.Clamp((float)NbodySimulation.Instance.m_massList[id] / 700f,15,50);

            }

            if (NbodySimulation.Instance.m_massList[id] > 500000)
            {
                if (isSun)
                {
                    //Blackhole
                    isBlackHole = true;
                    Destroy(light);
                    transform.localScale = Vector3.one * ((float)NbodySimulation.Instance.m_massList[id] / 10000f);
                    var mat = GetComponent<MeshRenderer>();
                    mat.material = new Material(Shader.Find("Unlit/Color"));
                    mat.material.color = Color.black;
                    name = "BlackHole";
                    m_body.type = 2;
                }


            }

        }
        else
        {
            NbodySimulation.Instance.RemoveBody(m_body,id);
            Destroy(gameObject);
        }

    }
}
