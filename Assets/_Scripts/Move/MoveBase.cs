using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string name;
    public string Name => name;

    [TextArea][SerializeField] private string description;
    public string Description => description;

    [SerializeField] private PokemonType type;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private bool alwaysHit;
    [SerializeField] private int pp;
    [SerializeField] private int priority;
    [SerializeField] private MoveType moveType;
    [SerializeField] private MoveStatEffect effects;
    [SerializeField] private List<SecondaryMoveStatEffect> secondaryEffects;
    [SerializeField] private MoveTarget target;
    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool AlwaysHit => alwaysHit;

    public int PP => pp;

    public int Priority => priority;
    public MoveType MoveType => moveType;

    public MoveStatEffect Effect => effects;
    public List<SecondaryMoveStatEffect> SecondaryEffect => secondaryEffects;

    public MoveTarget Target => target;

    public bool IsSpecialMove => moveType == MoveType.Special;
}

public enum MoveType
{
    Physical, Special, Stats
}

[Serializable]
public class MoveStatEffect
{
    [SerializeField] List<StatBoosting> boostings;
    [SerializeField] private StatusConditionId status;
    [SerializeField] private StatusConditionId volatileStatus;

    public List<StatBoosting> Boostings => boostings;

    public StatusConditionId Status => status;
    public StatusConditionId VolatileStatus => volatileStatus;
}

[Serializable]
public class StatBoosting
{
    public Stat stat;
    public int boost;
    public MoveTarget target;
}

public enum MoveTarget
{
    Self, Other
}

[Serializable]
public class SecondaryMoveStatEffect : MoveStatEffect
{
    [SerializeField] private int chance;
    [SerializeField] private MoveTarget target;

    public int Chance => chance;
    public MoveTarget Target => target;
}