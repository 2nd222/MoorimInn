using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    public bool isBuilt = true; // 테이블 설치 상태

    private List<Chair> myChair; // 이 테이블에 속한 의자

    public List<Transform> foodDropSpots; // 음식을 놓을 위치

    void Awake()
    {
        myChair = new List<Chair>(GetComponentsInChildren<Chair>());

        // 의자들에게 부모 테이블 정보와 각자의 음식 위치를 할당
        for (int i = 0; i < myChair.Count; i++)
        {
            Chair chair = myChair[i];

            chair.table = this;

            // 의자 순서에 맞춰서 음식 놓을 위치를 배정
            if (i < foodDropSpots.Count)
                chair.myFoodSpot = foodDropSpots[i];
        }
    }
}
