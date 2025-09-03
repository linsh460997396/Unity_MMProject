using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;
using EmeraldAI.Utility;

public class MoveToMousePosition : MonoBehaviour
{
    EmeraldSystem EmeraldComponent;

    private void Start()
    {
        EmeraldComponent = GetComponent<EmeraldSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 80))
            {
                if (EmeraldComponent != null)
                {
                    EmeraldAPI.Movement.SetCustomDestination(EmeraldComponent, hit.point);
                }
            }
        }
    }
}
