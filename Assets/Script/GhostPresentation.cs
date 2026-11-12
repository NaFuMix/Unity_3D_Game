using UnityEngine;

// Procedural low-poly ghost mesh and a chase loop owned by this scene.
public sealed class GhostPresentation : MonoBehaviour
{
    private Transform cloth;
    private Mesh clothMesh;
    private Material clothMaterial;
    private Material eyeMaterial;
    private AudioSource chaseAudio;
    private AudioClip chaseClip;
    private bool chasing;
    private float bobTime;

    public void Initialize(Material material)
    {
        clothMaterial = new Material(material);
        clothMaterial.color = new Color(0.62f, 0.67f, 0.73f);
        cloth = new GameObject("Floating shroud", typeof(MeshFilter), typeof(MeshRenderer)).transform;
        cloth.SetParent(transform, false);
        clothMesh = CreateShroud();
        cloth.GetComponent<MeshFilter>().sharedMesh = clothMesh;
        cloth.GetComponent<MeshRenderer>().sharedMaterial = clothMaterial;

        eyeMaterial = new Material(material);
        eyeMaterial.color = new Color(1f, 0.015f, 0.02f);
        eyeMaterial.EnableKeyword("_EMISSION");
        eyeMaterial.SetColor("_EmissionColor", new Color(3f, 0.02f, 0.03f));
        CreateEye(-0.16f);
        CreateEye(0.16f);

        chaseAudio = gameObject.AddComponent<AudioSource>();
        chaseAudio.playOnAwake = false;
        chaseAudio.loop = true;
        // Shared screen co-op uses one non-spatial warning for both players.
        chaseAudio.spatialBlend = 0f;
        chaseAudio.volume = 0f;
        chaseClip = CreateChaseClip();
        chaseAudio.clip = chaseClip;
    }

    private void CreateEye(float x)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Glowing red eye";
        eye.transform.SetParent(cloth, false);
        eye.transform.localPosition = new Vector3(x, 0.43f, 0.43f);
        eye.transform.localScale = new Vector3(0.11f, 0.16f, 0.07f);
        Collider collider = eye.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(collider);
        eye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
    }

    private static Mesh CreateShroud()
    {
        const int segments = 24;
        float[] heights = { -0.82f, -0.40f, 0.05f, 0.48f, 0.78f, 0.92f };
        float[] radii = { 0.65f, 0.54f, 0.47f, 0.39f, 0.25f, 0.015f };
        Vector3[] vertices = new Vector3[segments * heights.Length];
        int[] triangles = new int[(heights.Length - 1) * segments * 6];
        int index = 0;
        for (int ring = 0; ring < heights.Length; ring++)
        {
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment * Mathf.PI * 2f / segments;
                float fold = 1f + 0.07f * Mathf.Cos(angle * 8f);
                float hem = ring == 0 ? 0.12f * Mathf.Cos(angle * 6f) : 0f;
                vertices[ring * segments + segment] = new Vector3(
                    Mathf.Sin(angle) * radii[ring] * fold,
                    heights[ring] + hem,
                    Mathf.Cos(angle) * radii[ring] * fold);
                if (ring == heights.Length - 1) continue;
                int a = ring * segments + segment;
                int b = ring * segments + (segment + 1) % segments;
                int c = a + segments;
                int d = b + segments;
                triangles[index++] = a;
                triangles[index++] = b;
                triangles[index++] = c;
                triangles[index++] = b;
                triangles[index++] = d;
                triangles[index++] = c;
            }
        }
        Mesh mesh = new Mesh { name = "Ghost cloth mesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static AudioClip CreateChaseClip()
    {
        const int rate = 24000;
        const int seconds = 4;
        float[] samples = new float[rate * seconds];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / rate;
            float phase = t % 0.25f;
            float pulse = Mathf.Min(1f, phase * 180f) * Mathf.Exp(-phase * 20f);
            float throb = Mathf.Sin(2f * Mathf.PI * 68f * t) * pulse * 0.42f;
            float tension = (Mathf.Sin(2f * Mathf.PI * 207f * t) +
                Mathf.Sin(2f * Mathf.PI * 220f * t)) * 0.07f;
            float fade = Mathf.Clamp01(Mathf.Min(t, seconds - t) * 100f);
            samples[i] = (throb + tension) * fade;
        }
        AudioClip clip = AudioClip.Create("Ghost pursuit", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void SetChasing(bool value)
    {
        chasing = value;
    }

    private void Update()
    {
        if (chaseAudio == null) return;
        bobTime += Time.deltaTime;
        cloth.localPosition = Vector3.up * Mathf.Sin(bobTime * 2.5f) * 0.08f;
        if (chasing && !chaseAudio.isPlaying) chaseAudio.Play();
        chaseAudio.volume = Mathf.MoveTowards(chaseAudio.volume, chasing ? 0.65f : 0f, Time.unscaledDeltaTime * 1.3f);
        if (!chasing && chaseAudio.volume <= 0f) chaseAudio.Stop();
    }

    private void OnDisable()
    {
        chasing = false;
        if (chaseAudio != null) { chaseAudio.Stop(); chaseAudio.volume = 0f; }
    }

    private void OnDestroy()
    {
        Destroy(clothMesh);
        Destroy(clothMaterial);
        Destroy(eyeMaterial);
        Destroy(chaseClip);
    }
}
