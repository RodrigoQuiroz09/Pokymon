using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public enum GameState
{
    Travel, Battle, Dialog, Cutscene
}

public class GameManager : MonoBehaviour
{
    private GameState _gameState;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] Camera worldMainCamera;
    [SerializeField] Image transitionPanel;

    public static GameManager SharedInstance;

    public AudioClip worldClip, battleClip;

    private TrainerController trainer;

    private void Awake()
    {
        if (SharedInstance != null) Destroy(this);
        _gameState = GameState.Travel;

        SharedInstance = this;
    }


    private void Start()
    {
        StatusConditionFactory.InitFactory();
        SoundManager.SharedInstance.PlayMusic(worldClip);
        playerController.OnPokemonEncountered += StartPokemonBattle;
        playerController.OnEnterTrainerFov += (Collider2D trainerCollider) =>
        {
            _gameState = GameState.Cutscene;
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if (trainer != null) StartCoroutine(trainer.TriggerTrainerBattle(playerController));

        };
        battleManager.OnBattleFinish += FinishPokemonBattle;
        DialogManager.SharedInstance.OnDialogStart += () => { _gameState = GameState.Dialog; };
        DialogManager.SharedInstance.OnDialogFinish += () =>
        {
            if (_gameState == GameState.Dialog) _gameState = GameState.Travel;
        };
    }

    void StartPokemonBattle()
    {
        StartCoroutine(FadeInBattle());
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        this.trainer = trainer;
        StartCoroutine(FadeInTrainerBattle(trainer));
    }

    IEnumerator FadeInTrainerBattle(TrainerController trainer)
    {
        SoundManager.SharedInstance.PlayMusic(battleClip);
        _gameState = GameState.Battle;

        yield return transitionPanel.DOFade(1f, 1f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();

        var trainerParty = trainer.GetComponent<PokemonParty>();

        Debug.LogWarning(trainerParty);

        battleManager.HandleStartTrainerBattle(playerParty, trainerParty);
        yield return transitionPanel.DOFade(0f, 1f).WaitForCompletion();
    }

    IEnumerator FadeInBattle()
    {
        SoundManager.SharedInstance.PlayMusic(battleClip);
        _gameState = GameState.Battle;

        yield return transitionPanel.DOFade(1f, 1f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<PokemonMapArea>().GetComponent<PokemonMapArea>().GetRandomWildPokemon();
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleManager.HandleStartBattle(playerParty, wildPokemonCopy);
        yield return transitionPanel.DOFade(0f, 1f).WaitForCompletion();
    }

    void FinishPokemonBattle(bool playerHasWon)
    {
        if (trainer != null && playerHasWon)
        {
            trainer.AfterTrainerLostBattle();
            trainer = null;
        }
        StartCoroutine(FadeOutBattle(playerHasWon));
    }

    IEnumerator FadeOutBattle(bool playerHasWon)
    {
        yield return transitionPanel.DOFade(1f, 1f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        SoundManager.SharedInstance.PlayMusic(worldClip);


        battleManager.gameObject.SetActive(false);
        worldMainCamera.gameObject.SetActive(true);

        yield return transitionPanel.DOFade(0f, 1f).WaitForCompletion();
        _gameState = GameState.Travel;
    }

    private void Update()
    {
        if (_gameState == GameState.Travel)
        {
            playerController.HandleUpdate();
        }
        else if (_gameState == GameState.Battle)
        {
            battleManager.HandleUpdate();
        }
        else if (_gameState == GameState.Dialog)
        {
            DialogManager.SharedInstance.HandleUpdate();
        }
    }
}
