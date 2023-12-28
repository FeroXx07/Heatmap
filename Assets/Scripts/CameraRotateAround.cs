using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotateAround : MonoBehaviour
{ 
    public GameObject target;
    public float angle = 20.0f;
    void Update()
    {
        transform.RotateAround(target.transform.position, Vector3.up, angle * Time.deltaTime);
    }
}
