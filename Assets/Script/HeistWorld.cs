using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Added automatically by GameManager in the gameplay scene.
public sealed class HeistWorld : MonoBehaviour
{
    public float respawnSeconds = 5f;
    public float sightRange = 25f;
    public float sightAngle = 65f;
    public float ghostSpeed = 10f;
    public float redLightIntensity = 100f;
    private GhostPresentation ghostPresentation;
    private sealed class GhostState
    {
        public CharacterController controller;
        public GhostPresentation presentation;
        public RectTransform dot;
        public float patrolTime;
        public Vector3 patrolDirection;
    }

    private readonly List<GhostState> ghosts = new List<GhostState>();

    private sealed class PlayerState
    {
        public PlayerController player;
        public Vector3 spawn;
        public float remaining;
        public float immuneUntil;
        public bool caught;
    }

    private readonly List<PlayerState> players = new List<PlayerState>();
    private readonly Dictionary<Gem, RectTransform> gemDots = new Dictionary<Gem, RectTransform>();
    private readonly List<RectTransform> playerDots = new List<RectTransform>();
    private RectTransform map;
    private TMP_Text status;
    private float mapRadius = 15f;
    private Material ghostMaterial;

    private IEnumerator Start()
    {
        // Wait until spawners and player Start methods finish.
        yield return null;
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            players.Add(new PlayerState { player = player, spawn = player.transform.position });

        GemSpawner spawner = FindFirstObjectByType<GemSpawner>();
        if (spawner != null) mapRadius = Mathf.Max(mapRadius, spawner.mapSize);
        foreach (Gem gem in FindObjectsByType<Gem>(FindObjectsSortMode.None))
            mapRadius = Mathf.Max(mapRadius, Mathf.Abs(gem.transform.position.x) + 2f, Mathf.Abs(gem.transform.position.z) + 2f);
        foreach (PlayerState state in players)
            mapRadius = Mathf.Max(mapRadius, Mathf.Abs(state.spawn.x) + 2f, Mathf.Abs(state.spawn.z) + 2f);

        BuildHud();
        for (int i = 0; i < 2; i++)
            SpawnGhost(i + 1);
        InvokeRepeating(nameof(RefreshGems), 0f, 0.5f);
    }

    private void BuildHud()
    {
        GameObject canvasObject = new GameObject("Heist HUD", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform top = Panel(canvasObject.transform, new Vector2(0, 1), new Vector2(24, -24), new Vector2(420, 98));
        GameManager manager = GameManager.Instance;
        Restyle(manager.timerText, top, new Vector2(20, -16), new Vector2(175, 60), 28);
        Restyle(manager.gemCountText, top, new Vector2(200, -16), new Vector2(200, 60), 23);

        RectTransform result = Panel(canvasObject.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 80));
        result.pivot = new Vector2(0.5f, 0.5f);
        result.GetComponent<Image>().color = Color.clear;
        Restyle(manager.resultText, result, new Vector2(0, 0), new Vector2(560, 80), 34);
        if (manager.resultText != null) manager.resultText.alignment = TextAlignmentOptions.Center;

        RectTransform mapPanel = Panel(canvasObject.transform, Vector2.one, new Vector2(-24, -24), new Vector2(250, 288));
        mapPanel.pivot = Vector2.one;
        Label(mapPanel, "GEM MAP", new Vector2(16, -8), new Vector2(220, 28), 18);
        map = Panel(mapPanel, new Vector2(0, 1), new Vector2(15, -42), new Vector2(220, 220));
        map.GetComponent<Image>().color = new Color(0.055f, 0.075f, 0.13f, 0.95f);
        Label(mapPanel, "Pink: gems   Blue: players   Red: ghosts", new Vector2(10, -264), new Vector2(235, 20), 11);
        foreach (PlayerState state in players)
            playerDots.Add(Dot(map, Color.cyan, 9));
        RectTransform bottom = Panel(canvasObject.transform, Vector2.zero, new Vector2(24, 24), new Vector2(540, 56));
        bottom.pivot = Vector2.zero;
        status = Label(bottom, "", new Vector2(14, -8), new Vector2(510, 40), 18);
    }

    private static RectTransform Panel(Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject item = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = item.GetComponent<Image>();
        image.color = new Color(0.06f, 0.075f, 0.13f, 0.9f);
        image.raycastTarget = false;
        return rect;
    }

    private static TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size, int fontSize)
    {
        TMP_Text label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
        Restyle(label, parent, position, size, fontSize);
        label.text = text;
        return label;
    }

    private static void Restyle(TMP_Text label, Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        if (label == null) return;
        label.transform.SetParent(parent, false);
        RectTransform rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = 11;
        label.fontSizeMax = fontSize;
        label.color = new Color(0.94f, 0.91f, 1f);
        label.raycastTarget = false;
    }

    private static RectTransform Dot(Transform parent, Color color, int size)
    {
        RectTransform dot = Panel(parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
        dot.pivot = new Vector2(0.5f, 0.5f);
        dot.GetComponent<Image>().color = color;
        return dot;
    }

    private void PlaceDot(RectTransform dot, Vector3 position)
    {
        dot.anchoredPosition = new Vector2(Mathf.Clamp(position.x / mapRadius, -1, 1), Mathf.Clamp(position.z / mapRadius, -1, 1)) * 104f;
    }

    private void RefreshGems()
    {
        List<Gem> removed = new List<Gem>();
        foreach (var entry in gemDots)
        {
            if (entry.Key == null) { Destroy(entry.Value.gameObject); removed.Add(entry.Key); }
            else PlaceDot(entry.Value, entry.Key.transform.position);
        }
        foreach (Gem gem in removed) gemDots.Remove(gem);
        foreach (Gem gem in FindObjectsByType<Gem>(FindObjectsSortMode.None))
        {
            if (gemDots.ContainsKey(gem)) continue;
            RectTransform dot = Dot(map, new Color(1f, 0.3f, 0.64f), 6);
            gemDots.Add(gem, dot);
            PlaceDot(dot, gem.transform.position);
        }
    }

    private void SpawnGhost(int number)
    {
        // Spawn at a free gem location away from all players, on the floor beneath it.
        Vector3 spawn = Vector3.zero;
        bool found = false;
        foreach (Gem gem in gemDots.Keys)
        {
            if (TrySpawn(gem.transform.position, out spawn)) { found = true; break; }
        }
        // RefreshGems may not have run yet.
        if (!found)
            foreach (Gem gem in FindObjectsByType<Gem>(FindObjectsSortMode.None))
                if (TrySpawn(gem.transform.position, out spawn)) { found = true; break; }
        if (!found)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                Vector3 candidate = new Vector3(Random.Range(-mapRadius, mapRadius), 0f, Random.Range(-mapRadius, mapRadius));
                if (TrySpawn(candidate, out spawn)) { found = true; break; }
            }
        }
        if (!found)
        {
            Debug.LogWarning("HeistWorld: no clear ghost spawn found.");
            return;
        }

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Red Lantern Ghost " + number;
        body.transform.rotation = Quaternion.Euler(0f, (number - 1) * 180f, 0f);
        body.transform.position = spawn;
        Collider primitiveCollider = body.GetComponent<Collider>();
        primitiveCollider.enabled = false;
        Destroy(primitiveCollider);
        CharacterController ghost = body.AddComponent<CharacterController>();
        ghost.height = 2f;
        ghost.radius = 0.45f;
        body.transform.SetParent(transform, true);
        if (ghostMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            ghostMaterial = new Material(shader);
        }
        body.GetComponent<Renderer>().enabled = false;
        GhostPresentation ghostPresentation = body.AddComponent<GhostPresentation>();
        ghostPresentation.Initialize(ghostMaterial);
        Light lamp = new GameObject("Red searchlight", typeof(Light)).GetComponent<Light>();
        lamp.transform.SetParent(body.transform, false);
        lamp.transform.localPosition = new Vector3(0, 0.45f, 0.55f);
        lamp.type = LightType.Spot;
        lamp.color = Color.red;
        lamp.range = sightRange;
        lamp.spotAngle = sightAngle;
        lamp.intensity = redLightIntensity;
        lamp.shadows = LightShadows.Soft;
        ghosts.Add(new GhostState
        {
            controller = ghost,
            presentation = ghostPresentation,
            dot = Dot(map, new Color(1f, 0.18f, 0.2f), 10),
            patrolDirection = body.transform.forward
        });
    }

    private bool TrySpawn(Vector3 position, out Vector3 spawn)
    {
        spawn = position;
        foreach (PlayerState state in players)
            if (Vector3.Distance(position, state.spawn) < 8f) return false;
        if (!Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out RaycastHit floor, 20f, ~0, QueryTriggerInteraction.Ignore))
            return false;
        spawn = floor.point + Vector3.up * 1.05f;
        foreach (GhostState existing in ghosts)
            if (Vector3.Distance(spawn, existing.controller.transform.position) < 6f) return false;
        return !Physics.CheckCapsule(spawn - Vector3.up * 0.5f, spawn + Vector3.up * 0.5f, 0.46f, ~0, QueryTriggerInteraction.Ignore);
    }

    private void Update()
    {
        if (map == null) return;
        string message = "Stay out of the ghost's red light";
        for (int i = 0; i < players.Count; i++)
        {
            PlayerState state = players[i];
            if (state.player == null) continue;
            PlaceDot(playerDots[i], state.player.transform.position);
            if (state.caught) message = "Player " + state.player.playerId + " respawns in " + Mathf.CeilToInt(state.remaining) + "s";
        }
        status.text = message;
        foreach (GhostState state in ghosts)
            UpdateGhost(state);
    }

    private void UpdateGhost(GhostState ghostState)
    {
        CharacterController ghost = ghostState.controller;
        if (ghost == null || GameManager.Instance.IsGameOver)
        {
            if (ghostState.presentation != null) ghostState.presentation.SetChasing(false);
            return;
        }
        PlaceDot(ghostState.dot, ghost.transform.position);

        PlayerState target = null;
        float closest = sightRange;
        Vector3 eye = ghost.transform.position + ghost.transform.forward * 0.55f + Vector3.up * 0.45f;
        foreach (PlayerState state in players)
        {
            if (state.player == null || state.caught || Time.time < state.immuneUntil) continue;
            CharacterController controller = state.player.GetComponent<CharacterController>();
            Vector3 delta = controller.bounds.center - eye;
            if (delta.magnitude > closest || Vector3.Angle(ghost.transform.forward, delta) > sightAngle * 0.5f) continue;
            if (Physics.Raycast(eye, delta.normalized, out RaycastHit hit, delta.magnitude + 0.1f, ~0, QueryTriggerInteraction.Ignore)
                && hit.transform.GetComponentInParent<PlayerController>() == state.player)
            {
                target = state;
                closest = delta.magnitude;
            }
        }
        ghostState.presentation.SetChasing(target != null);
        ghostState.patrolTime -= Time.deltaTime;
        if (ghostState.patrolTime <= 0f)
        {
            ghostState.patrolTime = 2.5f;
            ghostState.patrolDirection = Quaternion.Euler(0, Random.Range(-110f, 110f), 0) * ghost.transform.forward;
        }
        Vector3 direction = target == null ? ghostState.patrolDirection : target.player.transform.position - ghost.transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
            ghost.transform.rotation = Quaternion.RotateTowards(ghost.transform.rotation, Quaternion.LookRotation(direction), 150f * Time.deltaTime);
        CollisionFlags flags = ghost.Move((ghost.transform.forward * (target == null ? ghostSpeed * 0.45f : ghostSpeed) + Vector3.down * 5f) * Time.deltaTime);
        if ((flags & CollisionFlags.Sides) != 0)
        {
            ghostState.patrolDirection = Quaternion.Euler(0, 100f, 0) * ghost.transform.forward;
            ghostState.patrolTime = 1f;
        }
        if (target != null && closest < 1.3f)
        {
            ghostState.presentation.SetChasing(false);
            StartCoroutine(Respawn(target));
        }
    }

    private IEnumerator Respawn(PlayerState state)
    {
        state.caught = true;
        PlayerController player = state.player;
        player.enabled = false;
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        bool[] visible = new bool[renderers.Length];
        bool[] enabledColliders = new bool[colliders.Length];
        for (int i = 0; i < renderers.Length; i++) { visible[i] = renderers[i].enabled; renderers[i].enabled = false; }
        for (int i = 0; i < colliders.Length; i++) { enabledColliders[i] = colliders[i].enabled; colliders[i].enabled = false; }
        state.remaining = respawnSeconds;
        while (state.remaining > 0f && !GameManager.Instance.IsGameOver)
        {
            state.remaining -= Time.deltaTime;
            yield return null;
        }
        if (player == null) yield break;
        player.transform.position = state.spawn;
        player.ResetVerticalVelocity();
        for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].enabled = visible[i];
        for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = enabledColliders[i];
        player.enabled = true;
        state.immuneUntil = Time.time + 3f;
        state.caught = false;
    }

    private void OnDestroy()
    {
        if (ghostMaterial != null) Destroy(ghostMaterial);
    }
}
