using UnityEngine;
using UnityEngine.AI;

public class SampleSceneScript : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Transform Target;
    
    private void Start()
    {
        Agent.SetDestination(Target.position);
    }
}
