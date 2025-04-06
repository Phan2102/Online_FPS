using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIHandler : MonoBehaviour
{
    Transform target;
    HPHandler targetHPHandler;

    NavMeshPath navMeshPath;
    bool isCompletePath = false;

    Vector3 lastRecreatePathPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        navMeshPath = new NavMeshPath();
    }

    void SetTarget()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        target = potentialTargets[Random.Range(0, potentialTargets.Length)].transform;
        targetHPHandler = target.GetComponent<HPHandler>();

        if(transform == target)
            target = null;
    }

    public Vector3 GetDirectionToTarget(out float distanceToTarget)
    {
        if(navMeshPath == null)
        {
            distanceToTarget = 1000;
            return Vector3.zero;
        }

        distanceToTarget = 0;

        if (target == null)
            SetTarget();

        if(targetHPHandler.isDead)
            SetTarget();

        if (target == null)
            return Vector3.zero;


        DebugDrawNavMeshPath();

        distanceToTarget = (target.position - transform.position).magnitude;


        if ((transform.position - lastRecreatePathPosition).magnitude < 1)
            return GetVectorToPath();

        isCompletePath = NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navMeshPath);

        return GetVectorToPath();
    }

    Vector3 GetVectorToPath()
    {
        Vector3 vectorToPath = Vector3.zero;

        if(navMeshPath.corners.Length > 1)
            vectorToPath = navMeshPath.corners[1] - transform.position;
        else vectorToPath = target.position - transform.position;

        vectorToPath.Normalize();

        return vectorToPath;

    }

    void DebugDrawNavMeshPath()
    {
        if(navMeshPath == null) return;

        for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
        {
            Color pathLineColor = Color.white;

            if(!isCompletePath)
                pathLineColor = Color.magenta;

            Vector3 corner1Position = navMeshPath.corners[i];

            if(i == 0)
                corner1Position = transform.position;
        }
    }
}
