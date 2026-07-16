using System.Collections.Generic;

[System.Serializable]
public class BurgerOrder
{
    public string orderName;
    public List<string> ingredients = new List<string>();
    public bool wantsFries = false;
    public bool wantsDrink = false;
    public int orderNumber = 0;

    public BurgerOrder(string name, List<string> orderIngredients)
    {
        orderName = name;
        ingredients = orderIngredients;
    }

    public BurgerOrder(string name, List<string> orderIngredients, bool includeFries, bool includeDrink)
    {
        orderName = name;
        ingredients = orderIngredients;
        wantsFries = includeFries;
        wantsDrink = includeDrink;
    }

    public bool MatchesBurger(List<string> burger)
    {
        if (burger == null)
            return false;

        if (burger.Count != ingredients.Count)
            return false;

        for (int i = 0; i < ingredients.Count; i++)
        {
            if (burger[i] != ingredients[i])
            {
                return false;
            }
        }

        return true;
    }

    public bool MatchesTray(MealTray tray)
    {
        if (tray == null)
            return false;

        if (!tray.HasBurger())
            return false;

        if (!MatchesBurger(tray.GetBurgerCopy()))
            return false;

        if (tray.hasFries != wantsFries)
            return false;

        if (tray.hasDrink != wantsDrink)
            return false;

        return true;
    }

    public string GetOrderText()
    {
        string text = orderName + ": ";

        for (int i = 0; i < ingredients.Count; i++)
        {
            text += ingredients[i];

            if (i < ingredients.Count - 1)
            {
                text += " + ";
            }
        }

        if (wantsFries)
            text += " + Batatas";

        if (wantsDrink)
            text += " + Bebida";

        return text;
    }
}
