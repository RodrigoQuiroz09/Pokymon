using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] string trainerName;
    [SerializeField] Sprite trainerSprite;
    [SerializeField] Dialog dialog, afterLooseDialog;
    [SerializeField] GameObject exclamationMessage;
    [SerializeField] private GameObject fov;
    private Character character;

    private bool trainerLostBattle = false;
    public string TrainerName => trainerName;
    public Sprite TrainerSprite => trainerSprite;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovDirection(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    IEnumerator ShowExclamationMark()
    {
        exclamationMessage.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        exclamationMessage.SetActive(false);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        yield return ShowExclamationMark();

        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector3(Mathf.RoundToInt(moveVector.x), Mathf.RoundToInt(moveVector.y));

        yield return character.MoveTowards(moveVector);
        DialogManager.SharedInstance.ShowDialog(dialog, () =>
        {
            GameManager.SharedInstance.StartTrainerBattle(this);
        });
    }

    public void SetFovDirection(FacingDirection direction)
    {
        float angle = 0f;
        if (direction == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (direction == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (direction == FacingDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public void Interact(Vector3 source)
    {
        if (!trainerLostBattle) StartCoroutine(ShowExclamationMark());
        character.LookTowards(source);

        if (!trainerLostBattle)
        {
            DialogManager.SharedInstance.ShowDialog(dialog, () =>
            {
                GameManager.SharedInstance.StartTrainerBattle(this);
            });
        }
        else
        {
            DialogManager.SharedInstance.ShowDialog(afterLooseDialog, () => { });
        }
    }

    public void AfterTrainerLostBattle()
    {
        fov.gameObject.SetActive(false);
        trainerLostBattle = true;
    }
}
