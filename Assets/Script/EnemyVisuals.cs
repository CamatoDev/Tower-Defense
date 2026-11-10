using UnityEngine;
using System.Collections;

/// <summary>
/// EnemyVisuals — Anime les ennemis de façon procédurale (sans Animator).
/// Gère : flottement vertical (drones), inclinaison en virage (banking),
/// pulsation néon HDR et hit flash à l'impact.
/// Conçu pour être directement compatible avec les futurs modèles 3D (drones, mechs, etc.).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class EnemyVisuals : MonoBehaviour
{
    [Header("Type d'unité")]
    [Tooltip("Activer le flottement vertical (drones volants). Décocher pour les unités au sol (tanks/mechs).")]
    public bool enableHover = true;

    [Header("Hover / Flottement (Drones)")]
    [Tooltip("Amplitude verticale du flottement (en unités Unity)")]
    public float hoverAmplitude = 0.15f;
    [Tooltip("Fréquence du flottement (cycles par seconde)")]
    public float hoverFrequency = 1.5f;

    [Header("Banking / Inclinaison en virage (Drones)")]
    [Tooltip("Incliner l'unité dans le sens du virage pour un rendu de vol dynamique")]
    public bool enableBanking = true;
    [Tooltip("Angle maximal d'inclinaison (en degrés)")]
    public float maxBankingAngle = 18f;
    [Tooltip("Vitesse de lissage de l'inclinaison")]
    public float bankingSmoothSpeed = 6f;

    [Header("Neon Pulse (HDR)")]
    [Tooltip("Couleur de base de l'émission (néon de l'ennemi)")]
    public Color emissionBaseColor = Color.cyan;
    [Tooltip("Multiplicateur HDR d'émission pour faire briller le néon sans éblouir le décor")]
    public float emissionHDRMultiplier = 2.0f;
    [Tooltip("Intensité minimale du pulse (0 à 1)")]
    public float pulseMinIntensity = 0.5f;
    [Tooltip("Intensité maximale du pulse (0 à 1)")]
    public float pulseMaxIntensity = 1.0f;
    [Tooltip("Fréquence de la pulsation (cycles par seconde)")]
    public float pulseRate = 1.2f;

    [Header("Hit Flash")]
    [Tooltip("Durée du flash blanc lors d'un impact (secondes)")]
    public float flashDuration = 0.08f;
    private Color flashColor = Color.white;

    // Références internes
    private Renderer rend;
    private Material mat;
    private float baseY;
    private float hoverOffset;      // décalage de phase pour désynchroniser les ennemis
    private bool isFlashing = false;
    private float currentRoll = 0f;
    private Quaternion lastRotation;

    private void Start()
    {
        rend = GetComponent<Renderer>();

        // Clone le matériau pour éviter de modifier le matériau partagé
        // et permettre des effets indépendants par instance
        mat = rend.material;

        baseY = transform.position.y;
        lastRotation = transform.rotation;

        // Décalage aléatoire de la phase pour que tous les ennemis
        // ne bougent et ne pulsent pas exactement en même temps
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);

        // Active l'émission sur ce matériau (nécessaire pour _EmissionColor)
        mat.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        HandleHover();

        // Ne pas modifier l'émission pendant un flash
        if (!isFlashing)
        {
            HandlePulse();
        }
    }

    private void LateUpdate()
    {
        HandleBanking();
    }

    /// <summary>
    /// Fait osciller doucement l'unité volante sur l'axe Y si enableHover est activé.
    /// Les unités au sol (tanks) restent stables à leur hauteur initiale.
    /// </summary>
    private void HandleHover()
    {
        if (!enableHover) return;

        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin((Time.time + hoverOffset) * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
        transform.position = pos;
    }

    /// <summary>
    /// Applique une inclinaison subtile (roulis sur l'axe Z) proportionnelle à la vitesse de virage en Y.
    /// Donne une impression de vol dynamique et prépare l'intégration des futurs modèles 3D.
    /// </summary>
    private void HandleBanking()
    {
        if (!enableBanking) return;

        // Calcule la vitesse angulaire sur l'axe Y (delta d'angle par frame)
        float deltaYaw = Mathf.DeltaAngle(lastRotation.eulerAngles.y, transform.eulerAngles.y);
        lastRotation = transform.rotation;

        // Inclinaison cible : virage à droite (deltaYaw > 0) -> roulis négatif (penche à droite)
        float targetRoll = -Mathf.Clamp(deltaYaw / (Time.deltaTime * 60f), -1f, 1f) * maxBankingAngle;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, bankingSmoothSpeed * Time.deltaTime);

        // Applique l'inclinaison sur l'axe Z local sans affecter la direction Y
        Vector3 euler = transform.eulerAngles;
        euler.z = currentRoll;
        transform.eulerAngles = euler;
    }

    /// <summary>
    /// Fait pulser la lumière émissive néon en HDR.
    /// </summary>
    private void HandlePulse()
    {
        float t = Mathf.PingPong(Time.time * pulseRate, 1f);
        float intensity = Mathf.Lerp(pulseMinIntensity, pulseMaxIntensity, t) * emissionHDRMultiplier;

        mat.SetColor("_EmissionColor", emissionBaseColor * intensity);
    }

    /// <summary>
    /// Appelée par Enemy.TakeDamage() — flash blanc bref pour signaler un impact.
    /// </summary>
    public void FlashHit()
    {
        if (!isFlashing)
        {
            StartCoroutine(DoFlash());
        }
    }

    private IEnumerator DoFlash()
    {
        isFlashing = true;

        Color originalColor = mat.GetColor("_Color");

        mat.SetColor("_Color", flashColor);
        mat.SetColor("_EmissionColor", flashColor * 2.5f);

        yield return new WaitForSeconds(flashDuration);

        mat.SetColor("_Color", originalColor);
        isFlashing = false;
    }

    private void OnDestroy()
    {
        if (mat != null)
        {
            Destroy(mat);
        }
    }
}
