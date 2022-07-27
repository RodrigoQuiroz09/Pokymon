using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Linq;
public enum BattleState
{
    StartBattle,
    ActionSelection,
    MovementSelection,
    Busy,
    YesNoChoice,
    PartySelectScreen,
    ForgetMovement,
    FinishBattle,
    RunTurn
}

public enum BattleAction
{
    Move, SwitchPokemon, UseItem, Run
}

public enum BattleType
{
    WildPokemon,
    Trainer,
    Leader
}
public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleDialogBox battleDialogBox;
    [SerializeField] private PartyHUD partyHUD;
    [SerializeField] private GameObject pokeball;
    [SerializeField] private SelectionMovementUI selectionMovementUI;
    [SerializeField] private Image playerImage, trainerImage;

    private PokemonParty playerParty;
    private PokemonParty trainerParty;
    private Pokemon wildPokemon;
    public UnityAction<bool> OnBattleFinish;
    public BattleState state;
    public BattleState? previousState;

    public BattleType type;

    private int currentSelectedMovement;
    private int currentSelectedAction;
    private int currentSelectedPokemon;
    private bool currentSelectedChoice = true;
    private int escapeAttempts;
    private float timeSinceLastClick;
    [SerializeField] float timeBetweenClicks = 0.5f;
    private MoveBase moveToLearn;

    public AudioClip attackClip, damageClip, faintedClip, levelUpClip, endBattleClip, pokeballClip;

    private PlayerController player;
    private TrainerController trainer;

    public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        type = BattleType.WildPokemon;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        escapeAttempts = 0;
        StartCoroutine(SetupBattle());
    }

    public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool isLeader = false)
    {
        type = (isLeader ? BattleType.Leader : BattleType.Trainer);
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        escapeAttempts = 0;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        state = BattleState.StartBattle;
        playerUnit.ClearHUD();
        enemyUnit.ClearHUD();
        if (type == BattleType.WildPokemon)
        {
            playerUnit.SetupPokemon(playerParty.GetFirstHealthyPokemon());
            battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
            enemyUnit.SetupPokemon(wildPokemon);
            yield return battleDialogBox.SetDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje apareció.");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            var playerInitialPos = playerImage.transform.localPosition;
            playerImage.transform.localPosition = playerInitialPos - new Vector3(400f, 0, 0);
            playerImage.transform.DOLocalMoveX(playerInitialPos.x, 0.5f);


            var trainerInitialPos = trainerImage.transform.localPosition;
            trainerImage.transform.localPosition = trainerInitialPos + new Vector3(400f, 0, 0);
            trainerImage.transform.DOLocalMoveX(trainerInitialPos.x, 0.5f);

            playerImage.sprite = player.PlayerSprite;
            trainerImage.sprite = trainer.TrainerSprite;

            yield return battleDialogBox.SetDialog($" {trainer.TrainerName} quiere luchar");

            yield return trainerImage.transform.DOLocalMoveX(trainerImage.transform.localPosition.x + 400f, 0.5f);
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetFirstHealthyPokemon();
            enemyUnit.SetupPokemon(enemyPokemon);
            yield return battleDialogBox.SetDialog($" {trainer.TrainerName} ha enviado a {enemyPokemon.Base.Name}");
            trainerImage.transform.localPosition = trainerInitialPos;

            yield return playerImage.transform.DOLocalMoveX(trainerImage.transform.localPosition.x - 400f, 0.5f);
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetFirstHealthyPokemon();
            playerUnit.SetupPokemon(playerPokemon);
            battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
            yield return battleDialogBox.SetDialog($" Ve {playerPokemon.Base.Name}");
            playerImage.transform.localPosition = playerInitialPos;



        }

        partyHUD.InitPartyHUD();

        PlayerActionSelection();
    }


    void BattleFinish(bool playerHasWon)
    {
        SoundManager.SharedInstance.PlaySound(endBattleClip);
        state = BattleState.FinishBattle;
        playerParty.Pokemons.ForEach((p) => p.OnBattleFinish());
        OnBattleFinish(playerHasWon);
    }

    void PlayerActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(battleDialogBox.SetDialog("Selecciona una acción"));
        battleDialogBox.ToggleActions(true);
        battleDialogBox.ToggleDialogText(true);
        battleDialogBox.ToggleMovements(false);
        currentSelectedAction = 0;
        battleDialogBox.SelectAction(currentSelectedAction);
    }

    void PlayerMovementSelection()
    {
        state = BattleState.MovementSelection;
        battleDialogBox.ToggleDialogText(false);
        battleDialogBox.ToggleActions(false);
        battleDialogBox.ToggleMovements(true);
        currentSelectedMovement = 0;
        battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
    }

    IEnumerator YesNoChoice(Pokemon newTrainerpokemon)
    {
        state = BattleState.Busy;
        yield return battleDialogBox.SetDialog($"{trainer.TrainerName} va a sacar a {newTrainerpokemon.Base.Name}. ¿Quieres cambiar tu pokemon?");
        state = BattleState.YesNoChoice;
        battleDialogBox.ToggleYesNoBox(true);
    }

    void OpenPartySelectionScreen()
    {
        state = BattleState.PartySelectScreen;
        partyHUD.SetPartyData(playerParty.Pokemons);
        partyHUD.gameObject.SetActive(true);
        currentSelectedPokemon = playerParty.GetPositionFromPokemon(playerUnit.Pokemon);
        partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);

    }


    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;

        if (timeSinceLastClick < timeBetweenClicks || battleDialogBox.isWriting) return;

        if (state == BattleState.ActionSelection) HandlePlayerActionSelection();

        else if (state == BattleState.MovementSelection) HandlePlayerMovementSelection();

        else if (state == BattleState.PartySelectScreen) HandlePlayerPartySelection();

        else if (state == BattleState.YesNoChoice) HandleYesNoChoice();

        else if (state == BattleState.ForgetMovement)
        {
            selectionMovementUI.HandleForgetMoveSelection(
                (moveIndex) =>
                {
                    if (moveIndex < 0)
                    {
                        timeSinceLastClick = 0;
                        return;
                    }
                    StartCoroutine(ForgetOldMove(moveIndex));
                });
        }

    }

    IEnumerator ForgetOldMove(int moveIndex)
    {
        selectionMovementUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.NUMBER_OF_LEARNABLE_MOVE)
        {
            yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} no ha aprendido {moveToLearn.Name}");
        }
        else
        {
            var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
            yield return battleDialogBox.SetDialog(
                $"{playerUnit.Pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}");
            playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = BattleState.FinishBattle;

    }

    void HandlePlayerActionSelection()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;

            currentSelectedAction = (currentSelectedAction + 2) % 4;

            battleDialogBox.SelectAction(currentSelectedAction);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;

            currentSelectedAction = (currentSelectedAction + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedAction / 2);

            battleDialogBox.SelectAction(currentSelectedAction);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            if (currentSelectedAction == 0)
            {
                PlayerMovementSelection();

            }
            else if (currentSelectedAction == 1)
            {
                //Pokemon
                previousState = state;
                OpenPartySelectionScreen();

            }
            else if (currentSelectedAction == 2)
            {
                // Mochila
                StartCoroutine(RunTurn(BattleAction.UseItem));
            }
            else if (currentSelectedAction == 3)
            {
                StartCoroutine(RunTurn(BattleAction.Run));
            }
        }

    }

    void HandlePlayerMovementSelection()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedMovment = currentSelectedMovement;
            currentSelectedMovement = (currentSelectedMovement + 2) % 4;
            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count) currentSelectedMovement = oldSelectedMovment;
            battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);

        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedMovment = currentSelectedMovement;

            currentSelectedMovement = (currentSelectedMovement + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedMovement / 2);


            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count) currentSelectedMovement = oldSelectedMovment;
            battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            battleDialogBox.ToggleDialogText(true);
            battleDialogBox.ToggleActions(false);
            battleDialogBox.ToggleMovements(false);
            StartCoroutine(RunTurn(BattleAction.Move));
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            timeSinceLastClick = 0;
            PlayerActionSelection();

        }

    }

    void HandlePlayerPartySelection()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon -= (int)Input.GetAxisRaw("Vertical") * 2;
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon += (int)Input.GetAxisRaw("Horizontal");
        }
        currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
        partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);


        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
            if (selectedPokemon.HP <= 0)
            {
                partyHUD.SetMessage("No puedes enviar un pokemon debilitado");
                return;
            }
            else if (selectedPokemon == playerUnit.Pokemon)
            {
                partyHUD.SetMessage("No puedes seleccionar el pokemon en Batalla");
                return;
            }

            partyHUD.gameObject.SetActive(false);
            battleDialogBox.ToggleActions(false);
            if (previousState == BattleState.ActionSelection)
            {
                previousState = null;
                StartCoroutine(RunTurn(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedPokemon));
            }

        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyHUD.SetMessage("Tienes que seleccionar un pokemon para continuar");

                return;
            }
            timeSinceLastClick = 0;
            partyHUD.gameObject.SetActive(false);
            if (previousState == BattleState.YesNoChoice)
            {
                previousState = null;
                StartCoroutine(SendNextTrainerPokemonToBattle());
            }
            else
            {
                PlayerActionSelection();

            }
        }
    }

    void HandleYesNoChoice()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedChoice = !currentSelectedChoice;
        }
        battleDialogBox.SelectYesNoAction(currentSelectedChoice);
        if (Input.GetAxisRaw("Submit") != 0)
        {
            battleDialogBox.ToggleYesNoBox(false);
            timeSinceLastClick = 0;
            if (currentSelectedChoice)
            {
                previousState = BattleState.YesNoChoice;
                OpenPartySelectionScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemonToBattle());
            }
        }
    }

    IEnumerator RunTurn(BattleAction playerAction)
    {
        state = BattleState.RunTurn;
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentSelectedMovement];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.RandomMove();
            bool playerGoesFirst = true;

            if (enemyUnit.Pokemon.CurrentMove.Base.Priority > playerUnit.Pokemon.CurrentMove.Base.Priority)
            {
                playerGoesFirst = false;
            }
            else if (enemyUnit.Pokemon.CurrentMove.Base.Priority == playerUnit.Pokemon.CurrentMove.Base.Priority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst ? playerUnit : enemyUnit);
            var secondUnit = (playerGoesFirst ? enemyUnit : playerUnit);

            var secondUnitPokemon = secondUnit.Pokemon;

            yield return RunMovement(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.FinishBattle) yield break;

            if (secondUnitPokemon.HP > 0)
            {
                yield return RunMovement(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.FinishBattle) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                battleDialogBox.ToggleActions(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscapeFromBattle();
            }
            var enemyMove = enemyUnit.Pokemon.RandomMove();
            yield return RunMovement(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.FinishBattle) yield break;
        }

        if (state != BattleState.FinishBattle)
        {
            PlayerActionSelection();
        }
    }

    IEnumerator RunMovement(BattleUnit attacker, BattleUnit target, Move move)
    {
        bool canRunMovement = attacker.Pokemon.OnStartTurn();
        if (!canRunMovement)
        {
            yield return ShowStatsMessages(attacker.Pokemon);
            yield return attacker.HUD.UpdatePokemonData();
            yield break;
        }

        yield return ShowStatsMessages(attacker.Pokemon);

        move.Pp--;

        yield return battleDialogBox.SetDialog($"{attacker.Pokemon.Base.Name} ha usado {move.Base.Name}");

        if (MoveHits(move, attacker.Pokemon, target.Pokemon))
        {
            yield return RunMoveAnims(attacker, target);

            if (move.Base.MoveType == MoveType.Stats)
            {
                yield return RunMoveStats(attacker.Pokemon, target.Pokemon, move.Base.Effect, move.Base.Target);
            }
            else
            {

                var damageDesc = target.Pokemon.RecieveDamage(attacker.Pokemon, move);
                StartCoroutine(target.HUD.UpdatePokemonData());
                yield return ShowDamageDescription(damageDesc);
            }

            if (move.Base.SecondaryEffect != null && move.Base.SecondaryEffect.Count > 0)
            {
                foreach (var sec in move.Base.SecondaryEffect)
                {
                    if ((sec.Target == MoveTarget.Other && target.Pokemon.HP > 0) ||
                        (sec.Target == MoveTarget.Self && attacker.Pokemon.HP > 0))
                    {
                        var rnd = Random.Range(0, 100);
                        if (rnd < sec.Chance)
                        {
                            yield return RunMoveStats(attacker.Pokemon, target.Pokemon, sec, sec.Target);
                        }
                    }
                }
            }

            if (target.Pokemon.HP <= 0) yield return HandlePokemonFainted(target);
        }
        else
        {
            yield return battleDialogBox.SetDialog($"El ataque de {attacker.Pokemon.Base.Name} ha fallado");
        }

    }

    bool MoveHits(Move move, Pokemon attacker, Pokemon target)
    {
        if (move.Base.AlwaysHit) return true;

        float rnd = Random.Range(0, 100);
        float moveAcc = move.Base.Accuracy;

        float accuracy = attacker.StatsBoosted[Stat.Accuracy];
        float evasion = target.StatsBoosted[Stat.Evasion];

        float multiplierAcc = 1.0f + Mathf.Abs(accuracy) / 3.0f;
        float multiplierEvs = 1.0f + Mathf.Abs(evasion) / 3.0f;


        moveAcc = (accuracy > 0 ? multiplierAcc * moveAcc : moveAcc / multiplierAcc);

        moveAcc = (evasion > 0 ? moveAcc / multiplierEvs : moveAcc * multiplierEvs);


        return rnd < moveAcc;
    }

    IEnumerator RunMoveStats(Pokemon attacker, Pokemon target, MoveStatEffect effect, MoveTarget moveTarget)
    {
        foreach (var boost in effect.Boostings)
        {
            if (boost.target == MoveTarget.Self) attacker.ApplyBoost(boost);
            else target.ApplyBoost(boost);
        }

        if (effect.Status != StatusConditionId.NONE)
        {
            if (moveTarget == MoveTarget.Other) target.SetConditionStatus(effect.Status);
            else attacker.SetConditionStatus(effect.Status);
        }

        if (effect.VolatileStatus != StatusConditionId.NONE)
        {
            if (moveTarget == MoveTarget.Other) target.SetVolatileConditionStatus(effect.VolatileStatus);
            else attacker.SetVolatileConditionStatus(effect.VolatileStatus);
        }
        yield return ShowStatsMessages(attacker);
        yield return ShowStatsMessages(target);
    }

    IEnumerator RunMoveAnims(BattleUnit attacker, BattleUnit target)
    {
        attacker.PlayAttackAnimation();
        SoundManager.SharedInstance.PlaySound(attackClip);
        yield return new WaitForSeconds(1f);

        target.PlayReceiveAttackAnimation();
        SoundManager.SharedInstance.PlaySound(damageClip);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator ShowStatsMessages(Pokemon pokemon)
    {
        while (pokemon.StatusChangeMessages.Count > 0)
        {
            var message = pokemon.StatusChangeMessages.Dequeue();
            yield return battleDialogBox.SetDialog(message);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit attacker)
    {
        if (state == BattleState.FinishBattle) yield break;
        yield return new WaitUntil(() => state == BattleState.RunTurn);
        attacker.Pokemon.OnFinishTurn();
        yield return ShowStatsMessages(attacker.Pokemon);
        yield return attacker.HUD.UpdatePokemonData();
        if (attacker.Pokemon.HP <= 0) yield return HandlePokemonFainted(attacker);
        yield return new WaitUntil(() => state == BattleState.RunTurn);
    }

    void CheckForBattleFinish(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayer)
        {
            var nextPokemon = playerParty.GetFirstHealthyPokemon();

            if (nextPokemon != null) OpenPartySelectionScreen();
            else BattleFinish(false);
        }
        else
        {
            if (type == BattleType.WildPokemon) BattleFinish(true);
            else
            {
                var nextPokemon = trainerParty.GetFirstHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(YesNoChoice(nextPokemon));
                }
                else BattleFinish(true);
            }
        }
    }

    IEnumerator SendNextTrainerPokemonToBattle()
    {
        state = BattleState.Busy;
        var nextPokemon = trainerParty.GetFirstHealthyPokemon();
        enemyUnit.SetupPokemon(nextPokemon);
        yield return battleDialogBox.SetDialog($"{trainer.TrainerName} ha enviado a {nextPokemon.Base.Name}");
        state = BattleState.RunTurn;
    }

    IEnumerator ShowDamageDescription(DamageDescription desc)
    {
        if (desc.Critical > 1.0f)
        {
            yield return battleDialogBox.SetDialog("¡Un golpe crítico!");
        }
        if (desc.Type > 1.0f)
        {
            yield return battleDialogBox.SetDialog("¡Ataque super Efectivo!");
        }
        else if (desc.Type < 1.0f)
        {
            yield return battleDialogBox.SetDialog("No es muy efectivo");
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return battleDialogBox.SetDialog($"Vuelve {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
        }
        playerUnit.SetupPokemon(newPokemon);
        battleDialogBox.SetPokemonMovements(newPokemon.Moves);

        yield return battleDialogBox.SetDialog($"¡Yo te invoco {newPokemon.Base.Name}!");
        yield return new WaitForSeconds(1f);
        if (previousState == null)
        {
            state = BattleState.RunTurn;

        }
        else if (previousState == BattleState.YesNoChoice)
        {
            yield return SendNextTrainerPokemonToBattle();
        }
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (type != BattleType.WildPokemon)
        {
            yield return battleDialogBox.SetDialog("No puedes robar los pokemon de  otros entrenadores");
            state = BattleState.RunTurn;
            yield break;
        }

        yield return battleDialogBox.SetDialog($"Has lanzado una {pokeball.name}!");

        SoundManager.SharedInstance.PlaySound(pokeballClip);

        var pokeballInst = Instantiate(pokeball, playerUnit.transform.position - new Vector3(2, 1), Quaternion.identity);

        var pokeballSpt = pokeballInst.GetComponent<SpriteRenderer>();

        yield return pokeballSpt.transform.DOLocalJump(enemyUnit.transform.position + new Vector3(0, 1.5f, 0), 2.0f, 1, 1.5f)
            .WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeballSpt.transform.DOLocalMoveY(enemyUnit.transform.position.y - 2f, 1f).WaitForCompletion();

        var numberOfShakes = TryToCatchPokemon(enemyUnit.Pokemon);

        for (int i = 0; i < Mathf.Min(numberOfShakes, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeballSpt.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.5f).WaitForCompletion();
        }

        if (numberOfShakes == 4)
        {
            yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} capturado!");
            yield return pokeballSpt.DOFade(0, 1.5f).WaitForCompletion();

            if (playerParty.AddPokemonToParty(enemyUnit.Pokemon))
            {
                yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} se ha añadido a tu equipo!");
            }
            else
            {
                yield return battleDialogBox.SetDialog("Se enviará al Pc de Bill...s");
            }
            Destroy(pokeballInst);
            BattleFinish(true);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            pokeballSpt.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            yield return (numberOfShakes < 2
                            ? battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} se escapa!")
                            : battleDialogBox.SetDialog("¡Casi la has atrapado!"));
            Destroy(pokeballInst);
            state = BattleState.RunTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float bonusPokeball = 1;
        float bonusStat = 1;
        float a = (3 * pokemon.MaxHP - 2 * pokemon.HP) * pokemon.Base.CatchRate * bonusStat * bonusPokeball / (3 * pokemon.MaxHP);

        if (a >= 255) return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 4;
        while (shakeCount < 4)
        {
            if (Random.Range(0, 65535) >= b) break;
            else shakeCount++;
        }
        return shakeCount;

    }

    IEnumerator TryToEscapeFromBattle()
    {
        state = BattleState.Busy;
        battleDialogBox.ToggleActions(false);
        if (type != BattleType.WildPokemon)
        {
            yield return battleDialogBox.SetDialog("No puedes huir de un combate contra entrenadores Pokemon");
            state = BattleState.RunTurn;
            yield break;
        }
        escapeAttempts++;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (playerSpeed >= enemySpeed)
        {
            yield return battleDialogBox.SetDialog("Has escapado con éxito");
            yield return new WaitForSeconds(1);
            OnBattleFinish(true);
        }
        else
        {
            int oddsScape = (Mathf.FloorToInt(playerSpeed * 128 / enemySpeed) + 30 * escapeAttempts) % 256;
            if (Random.Range(0, 256) < oddsScape)
            {
                yield return battleDialogBox.SetDialog("Has escapado con éxito");
                yield return new WaitForSeconds(1);
                OnBattleFinish(true);
            }
            else
            {
                yield return battleDialogBox.SetDialog("No puedes Escapar");
                state = BattleState.RunTurn;
            }
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return battleDialogBox.SetDialog($"{faintedUnit.Pokemon.Base.Name} se ha debilitado");
        SoundManager.SharedInstance.PlaySound(faintedClip);
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(1f);

        if (!faintedUnit.IsPlayer)
        {
            int expBase = faintedUnit.Pokemon.Base.ExpBase;
            int level = faintedUnit.Pokemon.Level;
            float multiplier = (type == BattleType.WildPokemon ? 1 : 1.5f);
            int wonExp = Mathf.FloorToInt(expBase * level * multiplier / 7);

            playerUnit.Pokemon.Experience += wonExp;
            yield return new WaitForSeconds(0.5f);
            yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha ganado {wonExp} puntos de experiencia");
            yield return playerUnit.HUD.SetExpSmooth();

            yield return new WaitForSeconds(1f);

            while (playerUnit.Pokemon.NeedsToLevelUp())
            {
                playerUnit.HUD.SetLevelText();
                playerUnit.Pokemon.HasHPChange = true;
                yield return playerUnit.HUD.UpdatePokemonData();
                yield return new WaitForSeconds(1f);
                yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} sube de nivel!");
                SoundManager.SharedInstance.PlaySound(levelUpClip);
                var newLearnableMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newLearnableMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.NUMBER_OF_LEARNABLE_MOVE)
                    {
                        playerUnit.Pokemon.LearnMove(newLearnableMove);
                        yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha aprendido {newLearnableMove.Move.Name}");
                        battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} intenta aprender {newLearnableMove.Move.Name}");
                        yield return battleDialogBox.SetDialog($"Pero no puedo aprender más de {PokemonBase.NUMBER_OF_LEARNABLE_MOVE} movimeintos");
                        yield return ChooseMovementToForget(playerUnit.Pokemon, newLearnableMove.Move);
                        yield return new WaitUntil(() => state != BattleState.ForgetMovement);
                    }
                }
                yield return playerUnit.HUD.SetExpSmooth(true);
            }
        }

        CheckForBattleFinish(faintedUnit);
    }

    IEnumerator ChooseMovementToForget(Pokemon learner, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return battleDialogBox.SetDialog("Selecciona el movimiento que quieres olvidar");
        selectionMovementUI.gameObject.SetActive(true);
        selectionMovementUI.SetMovements(learner.Moves.Select(mv => mv.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = BattleState.ForgetMovement;
    }
}