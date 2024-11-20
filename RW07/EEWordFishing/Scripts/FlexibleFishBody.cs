using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlexibleFishBody : MonoBehaviour
{   
    public TextMeshPro _tmp;

    public GameObject body;
    public GameObject head;
    public GameObject tail;
    private char[] shortLetter = new char[]{'t','i','l',' '};
    private char[] longLetter = new char[]{'w','m'};
    // Start is called before the first frame update
    void Start()
    {   
        float scaleFactor = setScaleNumber(_tmp.text)*(_tmp.text.Length - 1);
        Vector3 bodyScale = body.transform.localScale;
        bodyScale = new Vector3(scaleFactor, bodyScale.y, bodyScale.z);
        body.transform.localScale = bodyScale;
        Vector3 headPos = head.transform.localPosition;
        headPos = new Vector3(- scaleFactor / 2 - 0.97f, headPos.y, headPos.z);
        head.transform.position = headPos;
        
        Vector3 tailPos = tail.transform.localPosition;
        tailPos = new Vector3(scaleFactor / 2 + 1.05f, tailPos.y, tailPos.z);
        tail.transform.position = tailPos;
    }

    public float setScaleNumber(string text)
    {
        if (text.IndexOfAny(shortLetter) != -1 && text.IndexOfAny(longLetter) == -1)
        {   
            Debug.Log(1);
            return 0.65f;
        }
        Debug.Log(2);
        return 0.7f;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
