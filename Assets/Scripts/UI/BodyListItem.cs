using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BodyListItem : MonoBehaviour
{
    [SerializeField] Text text = default;
    Body m_body;
    int id;
    public void Setup(Body b,int i)
    {
        id = i;
        m_body = b;
    }

    public void Update()
    {
        //text.text = $"{(BodyTypes)m_body.type} (M{NbodySimulation.Instance.m_massList[id]})";

        //if (m_body.dirty == 1)
        //    Destroy(gameObject);
        //else
        //{
        //    var index = transform.GetSiblingIndex() - 1;
        //    var child = transform.parent.GetChild(Mathf.Max(0, index)).GetComponent<BodyListItem>();

        //    if (NbodySimulation.Instance.m_massList[child.id] < NbodySimulation.Instance.m_massList[id])
        //    {
        //        transform.SetSiblingIndex(Mathf.Max(0, index));
        //    }

        //}
    }

    public void OnClicked()
    {
        m_body.highlight = 1;
    }
}
