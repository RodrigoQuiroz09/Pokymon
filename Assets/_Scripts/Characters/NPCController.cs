using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcState
{
    Idle, Walking, Talking
}

public class NPCController : MonoBehaviour, Interactable
{
    private NpcState state;
    [SerializeField] private Dialog dialog;
    [SerializeField] private float idleTime = 3f;
    private float idleTimer = 0f;
    private Character character;

    [SerializeField] List<Vector3> moveDirections;
    private int currentDirection;


    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Vector3 source)
    {
        if (state == NpcState.Idle)
        {
            state = NpcState.Talking;
            character.LookTowards(source);
            DialogManager.SharedInstance.ShowDialog(dialog, () => { state = NpcState.Idle; idleTimer = 0; });
        }
    }

    private void Update()
    {
        if (state == NpcState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > idleTime)
            {
                idleTimer = 0;
                StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NpcState.Walking;
        var oldPosition = transform.position;
        var direction = Vector3.zero;
        if (moveDirections.Count > 0)
        {
            direction = moveDirections[currentDirection];
        }
        else
        {
            direction = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), 0);
        }

        yield return character.MoveTowards(direction);
        if (moveDirections.Count > 0 && oldPosition != transform.position)
        {
            currentDirection = (currentDirection + 1) % moveDirections.Count;
        }
        state = NpcState.Idle;
    }

}
