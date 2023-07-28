using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    public GameObject target;
    private void Awake() 
    {
        if (target != null)
        {
            transform.SetParent(target.transform.parent);
            target.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.name == "ActivationTrigger")
        {
            if (target != null)
            {
                target.SetActive(true);
                transform.SetParent(target.transform);
                //transform.position = target.transform.position;
                //transform.localPosition = Vector3.zero;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other) 
    {
        if ( other.gameObject.name == "ActivationTrigger")
        {
            if (target != null)
            {
                target.SetActive(false);
                transform.SetParent(target.transform.parent);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
