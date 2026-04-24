using UnityEngine;

/// <summary>
/// Abstract base class for all Yokai ability scripts.
/// 
/// HOW TO USE:
///   1. Create a new script (e.g. YokaiDashSlash) that inherits from YokaiAbility.
///   2. Attach it to the same GameObject as YokaiAI.
///   3. YokaiAI.Start() will auto-register it via RegisterAbility().
///   4. Override TryTrigger() — return true if your ability fires, false to pass.
///   5. Call yokaiAI.NotifyAbilityComplete() when the ability finishes.
///   6. Override OnYokaiParried() if your ability needs special parry handling.
///   7. Override OnInterrupted() to clean up if the ability is force-stopped.
/// 
/// EXAMPLE ABILITIES TO BUILD ON THIS:
///   - YokaiDashSlash    : long-range dash into a slash
///   - YokaiComboAttack  : uninterruptible 3-hit combo the player must parry
///   - YokaiShadowClone  : spawns clones around the player that all thrust
/// </summary>
public abstract class YokaiAbility : MonoBehaviour
{
    // Set by YokaiAI.Start() — gives abilities easy access to the core AI
    [HideInInspector] public YokaiAI yokaiAI;

    protected Animator    animator;
    protected Transform   player;
    //protected YokaiVFXManager vfx;
    //protected YokaiSFXManager sfx;

    protected virtual void Awake()
    {
        yokaiAI  = GetComponent<YokaiAI>();
        animator = GetComponent<Animator>();
        //vfx      = GetComponent<YokaiVFXManager>();
        //sfx      = GetComponent<YokaiSFXManager>();
    }

    protected virtual void Start()
    {
        // Cache the player reference once YokaiAI is ready
        if(yokaiAI != null)
        {
            player = yokaiAI.player;
        }
    }

    /// <summary>
    /// Called every attack cycle by YokaiAI when the basic attack timer fires.
    /// Return TRUE if this ability is taking over (YokaiAI enters Ability state).
    /// Return FALSE to pass control to the next ability or fall back to a basic attack.
    /// </summary>
    public abstract bool TryTrigger(YokaiAI ai);

    /// <summary>
    /// Called when the player successfully parries the Yokai while this ability is active.
    /// Return TRUE if the ability handled the parry itself (e.g. only parriable on hit 3).
    /// Return FALSE to let YokaiAI apply its default parry stun.
    /// </summary>
    public virtual bool OnYokaiParried() { return false; }

    /// <summary>
    /// Called when YokaiAI.InterruptAllAbilities() fires (e.g. a phase change).
    /// Use this to stop coroutines and reset any flags owned by this ability.
    /// </summary>
    public virtual void OnInterrupted() { }
}
