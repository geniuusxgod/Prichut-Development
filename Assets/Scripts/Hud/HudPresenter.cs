using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public sealed class HudPresenter : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text energyText;
    [SerializeField] private Text resortValueText;

    private void OnEnable()
    {
        if (gameState == null || coinsText == null || energyText == null || resortValueText == null)
        {
            Debug.LogError("HUD requires one GameState and all three value labels.", this);
            return;
        }

        gameState.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (gameState != null) gameState.Changed -= Refresh;
    }

    private void Refresh()
    {
        coinsText.text = gameState.Coins.ToString("N0", CultureInfo.InvariantCulture);
        energyText.text = gameState.Energy.ToString("N0", CultureInfo.InvariantCulture) + "/" +
                          gameState.EnergyCapacity.ToString("N0", CultureInfo.InvariantCulture);
        resortValueText.text = "Resort Value: " + gameState.ResortValue.ToString("N0", CultureInfo.InvariantCulture);
    }
}
