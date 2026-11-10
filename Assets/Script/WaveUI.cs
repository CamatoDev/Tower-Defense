using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Serialization;

public class WaveUI : MonoBehaviour
{
    [FormerlySerializedAs("livesText")]
    [Tooltip("Référence au composant Text affichant les vagues (conserve la liaison existante dans toutes les scènes)")]
    public Text waveText;

    [Header("Format d'affichage")]
    [Tooltip("Cocher pour afficher sous forme de ratio (ex: VAGUE 1 / 5) au lieu des vagues restantes")]
    public bool showAsRatio = false;

    void Update()
    {
        if (waveText == null) return;

        if (WaveSpawner.instance != null)
        {
            int remaining = WaveSpawner.instance.WavesRemaining;
            int total = WaveSpawner.instance.TotalWaves;
            int current = WaveSpawner.instance.CurrentWaveIndex + 1;

            if (showAsRatio)
            {
                waveText.text = $"VAGUE {Mathf.Min(current, total)} / {total}";
            }
            else
            {
                if (remaining > 1)
                {
                    waveText.text = $"{remaining} VAGUES RESTANTES";
                }
                else if (remaining == 1)
                {
                    waveText.text = "DERNIÈRE VAGUE !";
                }
                else
                {
                    waveText.text = "VAGUE FINALE EN COURS";
                }
            }
        }
    }
}
