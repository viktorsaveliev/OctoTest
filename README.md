# Unity Developer Test Assignment — Octo Games

**Position:** Middle Unity Developer  
**Focus:** 3D / Visual Novels / Gameplay  
**Tools:** Unity, Rider

---

## Task 1 — Coding Principles

> Describe two coding principles you consider most important when working on Unity projects that mix 3D gameplay, UI systems, and designer iteration.

### Separation of Concerns

Gameplay logic, UI, and data live in separate layers and do not reference each other directly. A `Character` component publishes events; a `CharactersView` subscribes to them — neither knows the other exists. This makes systems independently testable, replaceable, and safe to hand off to different team members.

Applied everywhere: gameplay components never hold UI references, UI never calls gameplay methods directly, communication goes through events or a service layer.

### Data-Driven Design via ScriptableObjects

All designer-facing parameters — character stats, dialogue configs, popup texts, UI settings — live in ScriptableObjects, not in code. Designers tweak values and create variants directly in the Editor without touching a script or triggering a recompile.

Applied to: entity configs, popup configs (`PopupData`), UI intervals, any value a designer might want to iterate on.

---

## Task 2 — Save / Load Utility

> Implement a generic save/load utility that saves any serializable class to file, loads it back safely, and handles missing or invalid data gracefully.

### Implementation — `SaveLoadService.cs`

```csharp
public class SaveLoadService
{
    private string GetPath(string key) => Path.Combine(Application.persistentDataPath, $"{key}.json");

    public void Save<T>(string key, T data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        string path = GetPath(key);
        string tmp = path + ".tmp";

        try
        {
            File.WriteAllText(tmp, json);
            File.Move(tmp, path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadService] Save failed for '{key}': { e.Message}");
            throw;
        }
    }

    public T Load<T>(string key, T defaultValue = default)
    {
        string path = GetPath(key);

        if (!File.Exists(path))
        {
            Debug.Log($"[SaveLoadService] No save for '{key}', returning default.");
            return defaultValue;
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveLoadService] Corrupt save for '{key}': {e.Message}. Returning default.");
            return defaultValue;
        }
    }

    public bool HasSave(string key) => File.Exists(GetPath(key));

    public void Delete(string key)
    {
        string path = GetPath(key);

        if (File.Exists(path)) 
        {
            File.Delete(path);
        }
    }
}
```

### Key decisions

- **Graceful fallback** — corrupt or missing files return `defaultValue`, never throw to the caller.
- **Generic API** — reusable across player progress, settings, VN state, and gameplay flags without modification.

### Usage

```csharp
SaveLoadService.Save("vn_save", new VNState { ChapterId = 3 });
var state = SaveLoadService.Load<VNState>("vn_save", new VNState());
```

---

## Task 3 — Popup / UI System

Overview

This is a lightweight, reusable popup system designed for Unity UI.
It supports:

dynamic title and body text
1–5 configurable buttons
per-button label, color, and click behavior
designer-friendly configuration via Unity Inspector

The system is intended for use in gameplay popups such as:

confirmations
warnings
tutorials
simple narrative choices (visual novel style)
Architecture

The system is composed of three main parts:

1. PopupView (Presentation Layer)

Responsible only for displaying UI and binding data:

sets title and body text
manages button visibility
assigns button data to UI elements
shows/hides popup

It does not contain gameplay logic.

2. PopupData (Data Container)
   
```csharp
public class PopupData
{
    public string Title;
    public string Body;
    public PopupButtonData[] popupButtonDatas;
}
```
Acts as a simple container for popup content and button configuration.

3. PopupButton (UI Element)

Each button is responsible for:

displaying label and color
invoking assigned UnityEvent on click
```csharp
public struct PopupButtonData
{
    public string Label;
    public Color Color;
    public UnityEvent OnClickAction;
}
```

#Key Design Choice
UnityEvent-based Button Actions

Instead of using code-based delegates (e.g. Action), this system uses UnityEvent to define button behavior.

#Why?

This allows:

full configuration in the Unity Inspector
no need to modify code for adding new popup behaviors
fast iteration for designers
reuse of the same popup prefab across different scenarios

Each button can independently define:

visual appearance (label, color)
behavior (via UnityEvent callbacks)
#Usage
1. Create PopupData in Inspector
- Set Title and Body
- Add 1–5 buttons
- Configure each button:
- Label
- Color
- OnClick event

2. Show Popup
```csharp
_popup.Show(_popupData);
```

#Limitations / Assumptions
Maximum number of buttons is defined by prefab setup (default: 5)
Button actions are configured via UnityEvent (not code delegates)
System is intended for simple UI flows, not complex branching logic systems
Design Notes

#This system prioritizes:

designer iteration speed
simplicity of configuration
prefab reusability
separation of UI presentation from gameplay logic

#It is especially suitable for:

visual novel style dialogue choices
lightweight UI decision popups
prototype and production UI flows with frequent iteration
Possible Improvements
add pooling for popups
add animation transitions (open/close)
support async results (e.g. Task-based choice selection)
add localization layer for text
introduce button layout customization presets

### 3.1 — Unity Components for the prefab

| Component | Reason |
|---|---|
| **Canvas** (Screen Space — Overlay, high sort order) | Renders on top of all 3D content |
| **CanvasGroup** on root panel | Enables fade in/out via DOTween without extra code |
| **Image** on background panel | Dimming overlay behind the popup |
| **VerticalLayoutGroup** on popup root | Auto-resizes with content, no manual sizing code |
| **TMP_Text** for title and body | Full Unicode support, rich text, better than legacy Text |
| **Button + PopupButton** × 5 | Pre-instantiated slots, toggled active/inactive per config |

---

## Task 4 — UI Performance & Refactoring

> Fix bugs, improve structure, optimize performance, and limit UI updates to a fixed interval.

### Bugs in the original code

| # | Bug | Fix |
|---|---|---|
| 1 | `[SerializedField]` — wrong attribute name | `[SerializeField]` |
| 2 | `GetComponents<T>()` returns an array, not a component | `GetComponent<T>()` |
| 3 | `_characters.Length` — `List<T>` has no `.Length` | `.Count` |
| 4 | `_characters.Length / totalValue` — inverted division | `totalValue / _characters.Count` |
| 5 | `GetComponent<Text>()` called every `FixedUpdate` frame | Cached in `Awake` |
| 6 | `FixedUpdate` used for UI — runs on physics timestep | `Update` + interval, or Coroutine, or UniTask |
| 7 | `Debug.Log` every frame — GC alloc + console spam | Removed from hot path |
| 8 | Add tick interval for better perfomance |
| 9 | Using `StringBuilder` for avoid GC allocation |
| 10 | Use `Character` links on List, not a Transform |

### Refactored — `CharactersView.cs`

```csharp
public class CharactersView : MonoBehaviour
{
    [SerializeField] private TMP_Text         _text;
    [SerializeField] private List<Character>  _characters;
    [SerializeField, Range(0.2f, 5f)] private float _tickInterval = 0.5f;

    private Coroutine     _coroutine;
    private readonly StringBuilder _sb = new();

    private void OnEnable()  => _coroutine = StartCoroutine(UpdateCoroutine());
    private void OnDisable() => StopCoroutineIfRunning();

    private void StopCoroutineIfRunning()
    {
        if (_coroutine == null) return;
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private IEnumerator UpdateCoroutine()
    {
        var wait = new WaitForSeconds(_tickInterval);
        while (true)
        {
            RefreshDisplay();
            yield return wait;
        }
    }

    private void RefreshDisplay()
    {
        float totalValue = 0f;
        int   count      = 0;

        foreach (Character character in _characters)
        {
            if (character == null) continue; // destroyed at runtime
            totalValue += character.Value;
            count++;
        }

        float avg = count > 0 ? totalValue / count : 0f;

        _sb.Clear();
        _sb.Append("Characters: ").Append(count)
           .Append("  Avg value: ").Append(avg.ToString("F1"));

        _text.SetText(_sb); // TMP StringBuilder overload — zero GC alloc
    }
}
```

### Why these changes

- **Coroutine instead of FixedUpdate** — UI has nothing to do with the physics timestep. A coroutine with a configurable interval is more predictable and cheaper.
- **`WaitForSeconds` cached outside the loop** — allocates once, reused every tick.
- **`StringBuilder` reused** — `_sb.Clear()` + `TMP.SetText(sb)` produces zero GC allocations in the hot path.
- **`List<Character>` directly** — eliminates `GetComponent` calls per frame entirely.
- **`OnEnable`/`OnDisable`** — the coroutine lifecycle is tied to the object's active state automatically.
- **Null-check** — safe against characters destroyed during gameplay.
- **`[Range]`** — designer can tune the interval in the Inspector without touching code.

---

## Task 5 — Entity Registry (Gameplay / State Logic)

> Design a system that tracks gameplay entities, returns only active ones, and cleanly handles entities being removed or disabled.

### Architecture — event-driven registry

Entities self-register on `OnEnable`/`OnDisable`. The registry maintains a live cache of active entities updated via events — no polling, no per-frame iteration.

```
IGameplayEntity          ← interface all trackable entities implement
EntityRegistry           ← holds the active set, updated via OnActiveStateChanged
IEntityRegistry          ← abstraction for DI / testing
Entity : MonoBehaviour   ← example implementation, self-registering
```

### IGameplayEntity

```csharp
public interface IGameplayEntity
{
    public event Action<IGameplayEntity> OnActiveStateChanged;
    public bool IsActive { get; }
}
```

### IEntityRegistry

```csharp
public interface IEntityRegistry
{
    public IReadOnlyCollection<IGameplayEntity> GetActiveEntities();
    public void AddEntity(IGameplayEntity entity);
    public void RemoveEntity(IGameplayEntity entity);
}
```

### EntityRegistry

```csharp
public class EntityRegistry : IEntityRegistry
{
    private readonly HashSet<IGameplayEntity> _entities = new();
    private readonly HashSet<IGameplayEntity> _activeEntities = new();

    public void AddEntity(IGameplayEntity entity)
    {
        _entities.Add(entity);
        entity.OnActiveStateChanged += OnEntityStateChanged;
    }

    public IReadOnlyCollection<IGameplayEntity> GetActiveEntities() => _activeEntities;

    public void RemoveEntity(IGameplayEntity entity)
    {
        entity.OnActiveStateChanged -= OnEntityStateChanged;
        _entities.Remove(entity);
    }

    private void OnEntityStateChanged(IGameplayEntity entity)
    {
        if (entity.IsActive)
        {
            _activeEntities.Add(entity);
        }
        else
        {
            _activeEntities.Remove(entity);
        }
    }
}
```

### Entity — example MonoBehaviour

```csharp
public class Entity : MonoBehaviour, IGameplayEntity
{
    public event Action<IGameplayEntity> OnActiveStateChanged;
    public bool IsActive { get; private set; }

    private IEntityRegistry _registry;

    // [Inject] — Zenject / VContainer
    public void Construct(IEntityRegistry registry) => _registry = registry;

    private void OnEnable()
    {
        _registry.AddEntity(this);
    }

    private void OnDisable()
    {
        _registry.RemoveEntity(this);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        OnActiveStateChanged?.Invoke(this);
    }
}
```

### Why this approach

- **`HashSet`** — O(1) add/remove/contains, no duplicates by design.
- **Event-driven cache** — `GetActiveEntities()` is O(1), no iteration on every call.
- **Self-registering via `OnEnable`/`OnDisable`** — the registry never needs to know about concrete types.
- **`IEntityRegistry` interface** — easy to mock in tests, easy to swap implementations.
- **`Construct()` method** — ready for Zenject/VContainer injection with minimal changes.
- **Subscription cleanup in `RemoveEntity`** — no event listener leaks.

---

## Optional Bonus

**Scaling for larger projects**

- `SaveLoadService` can be extended with `async/await` (`File.WriteAllTextAsync`) for mobile where IO on the main thread causes frame hitches.
- `EntityRegistry` can be sharded by type — `Registry<Enemy>`, `Registry<Interactable>` — to avoid filtering overhead in large scenes.
- `PopupService` can support a priority queue so critical warnings always appear above story choices.

**Designer interaction**

- All intervals, texts, and visual configs are exposed via `[SerializeField]` or `ScriptableObject` — no recompile needed for iteration.
- `PopupData` can be created and modified entirely in the Editor.
- `[Range]` attributes on numeric fields prevent out-of-range values at edit time.
