using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Constants;

public class ServingCounter : MonoBehaviour
{
    [SerializeField]
    private ServingSlot[] allSlots = new ServingSlot[SERVINGSLOT_MAXCOUNT];

    private List<ServingSlot> listActiveSlots = new List<ServingSlot>();

    private int level = 1;

    void Start()
    {
        foreach(ServingSlot slot in this.allSlots)
        {
            slot.gameObject.SetActive(false);
        }
        UpdateSlot();
    }

    private void UpdateSlot()
    {
        int targetCount = 0;

        if (this.level == 1) targetCount = 2;
        else if (this.level == 2) targetCount = 4;
        else if (this.level >= 3) targetCount = 8;

        for (int i = 0; i < targetCount; i++)
        {
            if (this.allSlots[i] != null)
            {
                this.allSlots[i].gameObject.SetActive(true);

                if (!this.listActiveSlots.Contains(this.allSlots[i]))
                {
                    this.listActiveSlots.Add(this.allSlots[i]);
                }
            }
        }
    }
}
