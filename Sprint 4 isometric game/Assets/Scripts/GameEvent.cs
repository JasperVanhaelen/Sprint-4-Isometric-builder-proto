using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent { }

public class CurrencyChangeGameEvent: GameEvent
{
    public int amount;
    public CurrencyType currencyType;

    public CurrencyChangeGameEvent(int amount, CurrencyType currencyType)
    {
        this.amount = amount;
        this.currencyType = currencyType;
    }
}

public class NotEnoughCurrencyGameEvent : GameEvent
{
    public int amount;
    public CurrencyType currencyType;

    public NotEnoughCurrencyGameEvent(int amount, CurrencyType currencyType)
    {
        this.amount = amount;
        this.currencyType = currencyType;
    }
}

public class EnoughCurrencyGameEvent : GameEvent
{

}

public class XPAddedGameEvent : GameEvent
{
    public int amount;

    public XPAddedGameEvent(int amount)
    {
        this.amount = amount;
    }
}

public class LevelChangedGameEvent : GameEvent
{
    public int NewLvl;

    public LevelChangedGameEvent(int currLvl)
    {
        NewLvl = currLvl;
    }
}