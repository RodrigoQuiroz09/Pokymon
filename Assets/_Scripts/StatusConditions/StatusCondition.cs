using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class StatusCondition
{
    public StatusConditionId Id { set; get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public UnityAction<Pokemon> OnApplyStatusCondition { get; set; }
    public Func<Pokemon, bool> OnStartTurn { get; set; }
    public UnityAction<Pokemon> OnFinishTurn { get; set; }

}
