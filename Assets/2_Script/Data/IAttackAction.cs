using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackAction
{
    void Attack();

    float attackRange { get; }

    AttackName attackName { get; }

    int attackCost { get; }

    void Cancel();
}
