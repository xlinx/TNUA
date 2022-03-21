using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class xlinx : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {

            }
        }
#else
        if (Input.GetMouseButtonUp(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {

            }
        }
#endif
    }
}
