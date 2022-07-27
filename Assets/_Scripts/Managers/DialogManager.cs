using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] private Text dialogText;
    public float characterPerSecond = 10.0f;
    public static DialogManager SharedInstance;
    private int currentLine = 0;
    private Dialog currentDialog;
    private bool isWriting;

    public UnityAction OnDialogStart, OnDialogFinish;

    private float timeSinceLastClick;
    [SerializeField] float timeBetweenClicks = 0.5f;

    public bool IsBeingShown;

    private UnityAction onDialogClose;

    private void Awake()
    {
        if (SharedInstance == null) SharedInstance = this;
        else Destroy(this);
    }

    public void ShowDialog(Dialog dialog, UnityAction onDialogFinish)
    {
        OnDialogStart?.Invoke();
        dialogBox.SetActive(true);
        currentDialog = dialog;
        IsBeingShown = true;
        this.onDialogClose = onDialogFinish;
        StartCoroutine(SetDialog(dialog.Lines[0]));
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (Input.GetAxisRaw("Submit") != 0 && !isWriting)
        {
            if (timeSinceLastClick >= timeBetweenClicks)
            {
                timeSinceLastClick = 0;
                currentLine++;
                if (currentLine < currentDialog.Lines.Count)
                {
                    StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));
                }
                else
                {
                    currentLine = 0;
                    dialogBox.SetActive(false);
                    IsBeingShown = false;
                    OnDialogFinish?.Invoke();
                    onDialogClose?.Invoke();
                }
            }

        }
    }
    public IEnumerator SetDialog(string line)
    {
        isWriting = true;
        dialogText.text = "";
        foreach (var character in line)
        {
            if (character != ' ') SoundManager.SharedInstance.PlayRandomCharacterSound();
            dialogText.text += character;
            yield return new WaitForSeconds(1 / characterPerSecond);
        }
        isWriting = false;
    }

}
