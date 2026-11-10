using UnityEngine;
using UnityEditor;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace TowerDefense.Editor
{
    public static class PostProcessSetupMenu
    {
#if UNITY_POST_PROCESSING_STACK_V2
        [MenuItem("Tools/Tower Defense/Configurer Post-Processing (Bloom & Sci-Fi)")]
        public static void SetupPostProcessing()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[PostProcessing] Aucune Main Camera trouvée dans la scène active !");
                return;
            }

            // S'assure que le PostProcessLayer est sur la caméra
            PostProcessLayer layer = cam.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = cam.gameObject.AddComponent<PostProcessLayer>();
                Undo.RegisterCreatedObjectUndo(layer, "Add PostProcessLayer");
            }

            layer.volumeLayer = ~0; // Écoute tous les layers par défaut
            layer.volumeTrigger = cam.transform;

            // Cherche un volume existant ou en crée un nouveau
            PostProcessVolume volume = Object.FindObjectOfType<PostProcessVolume>();
            if (volume == null)
            {
                GameObject volGo = new GameObject("Global Post-Processing Volume");
                Undo.RegisterCreatedObjectUndo(volGo, "Create Global Volume");
                volume = volGo.AddComponent<PostProcessVolume>();
                volume.isGlobal = true;
                volume.priority = 1f;
            }

            // Profil Sci-Fi
            string profilePath = "Assets/Materials/SciFi_Bloom_Profile.asset";
            PostProcessProfile profile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
                AssetDatabase.SaveAssets();
            }

            // Réglage du Bloom (calibré anti-éblouissement pour les surfaces blanches)
            Bloom bloom;
            if (!profile.HasSettings<Bloom>())
            {
                bloom = profile.AddSettings<Bloom>();
            }
            else
            {
                bloom = profile.GetSetting<Bloom>();
            }
            bloom.enabled.Override(true);
            bloom.intensity.Override(0.85f);  // Halo doux et élégant au lieu de 2.2f
            bloom.threshold.Override(1.15f);  // Seuil strict > 1.0 : exclut les dalles blanches et l'UI
            bloom.softKnee.Override(0.1f);   // Coupure nette anti-surexposition

            // Réglage d'une vignette subtile pour ambiance sci-fi
            Vignette vignette;
            if (!profile.HasSettings<Vignette>())
            {
                vignette = profile.AddSettings<Vignette>();
            }
            else
            {
                vignette = profile.GetSetting<Vignette>();
            }
            vignette.enabled.Override(true);
            vignette.intensity.Override(0.28f);
            vignette.smoothness.Override(0.4f);

            volume.sharedProfile = profile;
            EditorUtility.SetDirty(profile);
            EditorUtility.SetDirty(cam.gameObject);
            if (volume != null) EditorUtility.SetDirty(volume.gameObject);

            AssetDatabase.SaveAssets();

            Debug.Log("<color=cyan><b>[PostProcessing]</b></color> Post-Processing Stack v2 configuré avec succès ! Bloom (2.2) et Vignette (0.28) sont maintenant actifs dans votre scène.");
        }
#else
        [MenuItem("Tools/Tower Defense/Configurer Post-Processing (Bloom & Sci-Fi)")]
        public static void NotifyPackagePending()
        {
            Debug.LogWarning("[PostProcessing] Le package 'com.unity.postprocessing' est en cours d'installation ou requiert le rechargement des scripts par Unity. Cliquez dans la fenêtre d'Unity pour laisser le compilateur s'actualiser, puis relancez cette commande !");
        }
#endif
    }
}
