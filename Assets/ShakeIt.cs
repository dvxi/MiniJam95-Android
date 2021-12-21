using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeIt : MonoBehaviour
{
    public AnimationCurve animation;
    public float scale;
    public float hz;
    float time = 0;
    void Update()
    {

        if (transform.GetChild(0).gameObject.activeInHierarchy)
        {
            time += Time.deltaTime;
            transform.eulerAngles = new Vector3(0, 0, animation.Evaluate(time / hz) * scale);
            transform.GetChild(0).eulerAngles = new Vector3(0, 0,0);


        }
        else
        {
            time = 0;
            transform.rotation = Quaternion.identity;
        }
    }
}
