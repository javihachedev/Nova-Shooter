using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsPlayer : MonoBehaviour
{
    private Transform player;
    public float speed = 2.0f;

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player Ship").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (player) { 
            Vector3 delta = player.position - transform.position;
            delta.Normalize();
            /*float moveSpeed = speed * Time.deltaTime;
            transform.position = transform.position + (delta * moveSpeed);
            transform.rotation = Quaternion.LookRotation(delta);*/

            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, player.position, step);

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}