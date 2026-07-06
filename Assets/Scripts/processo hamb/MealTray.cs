using System.Collections.Generic;

[System.Serializable]
public class MealTray
{
    public List<string> burger = new List<string>();
    public bool hasFries = false;
    public bool hasDrink = false;

    public bool IsEmpty()
    {
        return !HasBurger() && !hasFries && !hasDrink;
    }

    public bool HasBurger()
    {
        return burger != null && burger.Count > 0;
    }

    public List<string> GetBurgerCopy()
    {
        if (burger == null)
            return new List<string>();

        return new List<string>(burger);
    }

    public MealTray Copy()
    {
        MealTray copy = new MealTray();
        copy.burger = GetBurgerCopy();
        copy.hasFries = hasFries;
        copy.hasDrink = hasDrink;
        return copy;
    }

    public void Clear()
    {
        if (burger != null)
            burger.Clear();

        hasFries = false;
        hasDrink = false;
    }
}
