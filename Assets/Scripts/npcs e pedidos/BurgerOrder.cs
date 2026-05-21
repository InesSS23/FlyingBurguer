using System.Collections.Generic;

[System.Serializable]
public class BurgerOrder
{
    public string orderName;
    public List<string> ingredients = new List<string>();

    public BurgerOrder(string name, List<string> orderIngredients)
    {
        orderName = name;
        ingredients = orderIngredients;
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

        return text;
    }
}