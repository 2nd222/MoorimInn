using TMPro;
using UnityEngine;

public class QuestRewardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI rewardText;

    public void Init(QuestRewardEntry reward)
    {
        switch (reward.type)
        {
            case QuestRewardType.Gold:
                nameText.text = "금화";
                rewardText.text = reward.amount.ToString();
                break;

            case QuestRewardType.Reputation:
                nameText.text = "명성";
                rewardText.text = reward.amount.ToString();
                break;

            case QuestRewardType.Recipe:
            {
                RecipeData recipe = DataManager.Instance.GetRecipeMasteryData(reward.id).data;

                nameText.text = "조리법 해금";
                rewardText.text = recipe.recipeName;
                break;
            }

            case QuestRewardType.Ingredient:
            {
                IngredientData ingredient = DataManager.Instance.GetIngredientData(reward.id);

                nameText.text = "재료 해금";
                rewardText.text = ingredient.itemName;
                break;
            }
        }
    }
}
