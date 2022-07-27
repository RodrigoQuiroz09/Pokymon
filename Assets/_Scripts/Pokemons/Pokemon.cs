using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Pokemon
{
    [SerializeField] private PokemonBase _base;
    [SerializeField] private int _level;

    public PokemonBase Base => _base;
    public int Level
    {
        get => _level;
        set => _level = value;
    }

    private List<Move> _moves;

    public List<Move> Moves
    {
        get => _moves;
        set => _moves = value;
    }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatsBoosted { get; private set; }
    public StatusCondition StatusCondition { get; set; }
    public int StatusNumberTurns { get; set; }
    public StatusCondition VolatileStatusCondition { get; set; }
    public int VolatileStatusNumberTurns { get; set; }

    public Queue<string> StatusChangeMessages { get; private set; }
    public event Action OnStatusConditionChange;
    public bool HasHPChange { get; set; } = false;
    public int previousHPValue;
    public Move CurrentMove { get; set; }

    private int _hp;
    public int HP
    {
        get => _hp;
        set
        {
            _hp = value;
            _hp = Mathf.FloorToInt(Mathf.Clamp(_hp, 0, MaxHP));
        }
    }

    private int _experience;

    public int Experience
    {
        get => _experience;
        set => _experience = value;
    }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;
        _experience = Base.GetNeccesaryExperienceForLevel(_level);
        InitPokemon();
    }

    public void InitPokemon()
    {
        _moves = new List<Move>();

        foreach (var lMove in _base.LearnableMoves)
        {
            if (lMove.Level <= _level) _moves.Add(new Move(lMove.Move));
            if (_moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVE) break;
        }

        CalculateStats();
        _hp = MaxHP;
        previousHPValue = HP;
        HasHPChange = true;
        ResetBoostings();
        VolatileStatusCondition = null;
        StatusCondition = null;
    }

    void ResetBoostings()
    {
        StatusChangeMessages = new Queue<string>();
        StatsBoosted = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpDefense, 0},
            {Stat.SpAttack, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0},
        };
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((_base.Attack * _level) / 100) + 2);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((_base.Defense * _level) / 100) + 2);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((_base.SpAttack * _level) / 100) + 2);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((_base.SpDefense * _level) / 100) + 2);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((_base.Speed * _level) / 100) + 1);

        MaxHP = Mathf.FloorToInt((_base.MaxHP * _level) / 20.0f) + _level + 10;
    }

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        int boost = StatsBoosted[stat];

        float multiplier = 1.0f + Mathf.Abs(boost) / 2.0f;
        if (boost >= 0)
        {
            statValue = Mathf.FloorToInt(statValue * multiplier);
        }
        else
        {
            statValue = Mathf.FloorToInt(statValue / multiplier);
        }
        return statValue;
    }

    public void ApplyBoost(StatBoosting boost)
    {

        var stat = boost.stat;
        var value = boost.boost;

        StatsBoosted[stat] = Mathf.Clamp(StatsBoosted[stat] + value, -6, 6);
        if (value > 0)
        {
            StatusChangeMessages.Enqueue($"{Base.Name} ha incrementado su {stat}");
        }
        else if (value < 0)
        {
            StatusChangeMessages.Enqueue($"{Base.Name} ha reducido su {stat}");
        }
        else
        {
            StatusChangeMessages.Enqueue($"{Base.Name} no nota ningun efecto");
        }
    }

    public int MaxHP { get; private set; }
    public int Attack => GetStat(Stat.Attack);
    public int Defense => GetStat(Stat.Defense);
    public int SpAttack => GetStat(Stat.SpAttack);
    public int SpDefense => GetStat(Stat.SpDefense);
    public int Speed => GetStat(Stat.Speed);

    public DamageDescription RecieveDamage(Pokemon attacker, Move move)
    {
        float critical = 1f;
        if (UnityEngine.Random.Range(0f, 100f) < 8f)
        {
            critical = 2f;
        }
        float type1 = TypeMAtrix.GetMultEffectiveness(move.Base.Type, this.Base.Type1);
        float type2 = TypeMAtrix.GetMultEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDescription = new DamageDescription()
        {
            Critical = critical,
            Type = type1 * type2,
            Fainted = false

        };


        float attack = (move.Base.IsSpecialMove ? attacker.SpAttack : attacker.Attack);
        float defense = (move.Base.IsSpecialMove ? this.SpDefense : this.Defense);

        float modifiers = UnityEngine.Random.Range(0.85f, 1.0f) * type1 * type2 * critical;
        float baseDamage = (((2 * attacker.Level / 5f + 2) * move.Base.Power * (attack / (float)defense)) / 50f) + 2;
        int totalDamage = Mathf.FloorToInt(baseDamage * modifiers);

        UpdateHP(totalDamage);
        if (HP <= 0) damageDescription.Fainted = true;
        return damageDescription;

    }

    public void UpdateHP(int dmg)
    {
        HasHPChange = true;
        previousHPValue = HP;
        HP -= dmg;

        if (HP <= 0)
        {
            HP = 0;
        }
    }

    public void SetConditionStatus(StatusConditionId id)
    {
        if (StatusCondition != null) return;

        StatusCondition = StatusConditionFactory.StatusConditions[id];
        StatusCondition?.OnApplyStatusCondition?.Invoke(this);
        StatusChangeMessages.Enqueue($"{Base.Name} {StatusCondition.StartMessage}");
        OnStatusConditionChange?.Invoke();
    }

    public void CureStatusCondition()
    {
        StatusCondition = null;
        OnStatusConditionChange?.Invoke();
    }

    public void SetVolatileConditionStatus(StatusConditionId id)
    {
        if (VolatileStatusCondition != null) return;

        VolatileStatusCondition = StatusConditionFactory.StatusConditions[id];
        VolatileStatusCondition?.OnApplyStatusCondition?.Invoke(this);
        StatusChangeMessages.Enqueue($"{Base.Name} {VolatileStatusCondition.StartMessage}");
    }
    public void CureVolatileStatusCondition()
    {
        VolatileStatusCondition = null;
    }
    public Move RandomMove()
    {
        var movesWithPP = Moves.Where(m => m.Pp > 0).ToList();
        if (movesWithPP.Count > 0)
        {
            int randId = UnityEngine.Random.Range(0, movesWithPP.Count);
            return movesWithPP[randId];
        }
        return null;
    }

    public bool NeedsToLevelUp()
    {
        int toNextLevelXP = (Base.GetNeccesaryExperienceForLevel(_level + 1) - Base.GetNeccesaryExperienceForLevel(_level));
        if (Experience > toNextLevelXP)
        {
            Experience -= toNextLevelXP;
            int currentMaxHP = this.MaxHP;
            _level++;
            HP += (this.MaxHP - currentMaxHP);
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(lm => lm.Level == _level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove learnableMove)
    {
        if (Moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVE) return;
        Moves.Add(new Move(learnableMove.Move));

    }



    public bool OnStartTurn()
    {
        bool canPerformMovement = true;
        if (StatusCondition?.OnStartTurn != null)
        {
            if (!StatusCondition.OnStartTurn(this))
            {
                canPerformMovement = false;
            }
        }
        if (VolatileStatusCondition?.OnStartTurn != null)
        {
            if (!VolatileStatusCondition.OnStartTurn(this))
            {
                canPerformMovement = false;
            }
        }
        return canPerformMovement;
    }

    public void OnBattleFinish()
    {
        VolatileStatusCondition = null;
        ResetBoostings();
    }
    public void OnFinishTurn()
    {
        StatusCondition?.OnFinishTurn?.Invoke(this);
        VolatileStatusCondition?.OnFinishTurn?.Invoke(this);
    }
}

public class DamageDescription
{
    public float Critical { get; set; }
    public float Type { get; set; }
    public bool Fainted { get; set; }
}
