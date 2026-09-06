using UnityEngine;
using UnityEngine.AI;

public class GuestMover : MonoBehaviour
{
    [Header("이동(A*) 설정")]
    private NavMeshAgent agent; 
    private Vector3 targetDestination; 
    
    private bool pausedByManager = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 destination)
    {
        targetDestination = destination;
        agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public bool HasArrived(float threshold = -1f)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return false;
        if (agent.pathPending) return false;
        if (agent.pathStatus != NavMeshPathStatus.PathComplete) return false;
        
        float distance = threshold >= 0f ? threshold : agent.stoppingDistance;

        if (Vector3.Distance(transform.position, targetDestination) > distance + 0.5f)
        {
            return false;
        }

        return agent.remainingDistance <= distance;
    }
    
    public void RecalculatePath()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;
        
        if (agent != null && agent.enabled && !agent.isStopped)
        {
            if (!HasArrived())
            {
                agent.SetDestination(targetDestination);
            }
        }
    }
    

    public void StopAndDisable()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }

    public void ResetAgent()
    {
        agent.enabled = false;
        targetDestination = Vector3.zero;
    }

    public void EnableAgent()
    {
        agent.enabled = true;
        agent.ResetPath();
        agent.isStopped = false;
        agent.velocity = Vector3.zero;
    }
    
    public void SetPaused(bool pause)
    {
        if (pausedByManager == pause) return;
        pausedByManager = pause;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = pause;
        if (pause)
            agent.velocity = Vector3.zero;
    }
}