using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMove : MonoBehaviour
{
    public static HandMove instance;
    [SerializeField]
    private Vector3 from, to;
    [SerializeField]
    private bool isUpdate = false;
    [SerializeField]
    private float smoothSpeed = 1;
    private Vector3 velocity = Vector3.zero;
    private void Awake()
    {
        instance = this;
    }


    public void ActiveHandMove(Vector3 from,Vector3 to)
    {
        transform.position = from;
        this.to = to;
        this.from = from;
        isUpdate = true;

    }

    public void DeActiveHandMove( )
    {
        transform.position = new Vector3(9999, 9999, 0);
       
        isUpdate = false;

    }

    private void Update()
    {
        if (!isUpdate) return;
        transform.position = Vector3.SmoothDamp(transform.position, to, ref velocity, smoothSpeed);
        float dst = Vector3.Distance(transform.position, to);
        if (dst <= 0.5f)
        {
            transform.position = from;
        }
    }

}
