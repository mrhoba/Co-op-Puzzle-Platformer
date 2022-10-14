using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
	public List<Transform> targets;
    private GameObject[] players;

    public Vector3 offset;
    public float smoothTime = 0.5f;

    public float minZoom = 40f;
    public float maxZoom = 10f;
    public float zoomLimiter = 5f;

    public float yPositionRestriction = -1;

    private Vector3 velocity;
    private Camera cam;

    private float nextTimeToSearch = 2;


    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    private void LateUpdate()
    { 

        if (targets.Count == 0 || targets[0] == null || targets[1] == null)
        {
            targets.Clear();
            FindPlayer();
            return;
        }

        Move();
        Zoom();
    }

    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();

        Vector3 newPosition = centerPoint + offset;

        newPosition = new Vector3(newPosition.x, Mathf.Clamp(newPosition.y, yPositionRestriction, Mathf.Infinity) ,newPosition.z);

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    private void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.center;
    }

    void FindPlayer()
    {
        if (nextTimeToSearch <= Time.time)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    targets.Add(players[i].transform);
                }
            }
        }
    }

}
