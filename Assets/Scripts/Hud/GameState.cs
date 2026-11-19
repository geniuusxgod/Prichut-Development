using System;
using UnityEngine;

// Jediny zdroj hodnot HUD. Odmeny a pravidla ekonomiky patria do dalsich uloh.
public sealed class GameState : MonoBehaviour
{
    [SerializeField] private int coins = 12450;
    [SerializeField] private int energy = 45;
    [SerializeField] private int energyCapacity = 50;
    [SerializeField] private int resortValue = 700;

    public int Coins => coins;
    public int Energy => energy;
    public int EnergyCapacity => energyCapacity;
    public int ResortValue => resortValue;
    public event Action Changed;

    public void SetCoins(int value)
    {
        if (coins == value) return;
        coins = value;
        Changed?.Invoke();
    }

    public void SetEnergy(int value)
    {
        if (energy == value) return;
        energy = value;
        Changed?.Invoke();
    }

    public void SetResortValue(int value)
    {
        if (resortValue == value) return;
        resortValue = value;
        Changed?.Invoke();
    }

    public void SetEnergyCapacity(int value)
    {
        if (energyCapacity == value) return;
        energyCapacity = value;
        Changed?.Invoke();
    }

    private void OnValidate()
    {
        Changed?.Invoke();
    }
}
