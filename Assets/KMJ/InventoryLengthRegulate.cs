using UnityEngine;
using UnityEngine.UI;

public class InventoryLengthRegulate : MonoBehaviour
{
    private GridLayoutGroup gridLayout;

    private void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
    }

    private void OnEnable()
    {
        UpdateConstraint();
    }

    private void OnTransformChildrenChanged()
    {
        if (gameObject.activeInHierarchy) 
        {
            UpdateConstraint();
        }
    }

    private void UpdateConstraint()
    {
        if (gridLayout == null) 
            return;

        int actualSlotCount = 0;
        
        // 내 밑에 있는 모든 자식들을 하나씩 검사합니다.
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Slot")) 
            {
                actualSlotCount++;
            }
        }

        if (actualSlotCount <= 5)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayout.constraintCount = 1;
        }
        else
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayout.constraintCount = 2;
        }
    }
}