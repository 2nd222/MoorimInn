using System;
using UnityEngine;

[Serializable]
public struct ReceiptData
{
    public DayData week;             // ��¥
    public int totalRevenue;        // ��ü ����
    public int fame;                // ���� ����
    public int debt;                // �� 
    public int happyGuests;         // ������ �մ� ��
    public int angryGuests;         // �Ҹ����� �մ� ��
    public int netIncome;           // ������
}
