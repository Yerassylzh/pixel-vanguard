# UI Animation System - Quick Reference

## Core Animation Methods

### UIAnimator Static Class

```csharp
using PixelVanguard.UI.Animations;

// Panel slide animations
UIAnimator.ShowPanel(panelGameObject, SlideDirection.Right, duration: 0.3f);
UIAnimator.HidePanel(panelGameObject, SlideDirection.Left, duration: 0.3f);

// Popup animations
UIAnimator.ShowPopup(transform, duration: 0.5f, useUnscaledTime: true);
UIAnimator.HidePopup(transform, duration: 0.3f);

// Value count-up
UIAnimator.AnimateValue(textComponent, fromValue: 0, toValue: 100, duration: 0.5f);

// Button effects
UIAnimator.AnimateButtonPress(buttonTransform);
UIAnimator.AnimateButtonHover(buttonTransform);

// Effects
UIAnimator.Pulse(transform, intensity: 0.2f, loops: 2);
UIAnimator.Shake(transform, strength: 10f, duration: 0.3f);
UIAnimator.PunchScale(transform, strength: 0.2f);

// Fade
UIAnimator.FadeIn(canvasGroup, duration: 0.3f);
UIAnimator.FadeOut(canvasGroup, duration: 0.3f);

// Stagger (animate multiple elements with delay)
UIAnimator.StaggerAnimate(transformArray, (t) => t.DOScale(1f, 0.3f), staggerDelay: 0.1f);
```

---

## Components

### AnimatedPanel
Attach to panels that need slide animations.

```csharp
AnimatedPanel panel = GetComponent<AnimatedPanel>();
panel.Show();  // Slide in
panel.Hide();  // Slide out
panel.ShowImmediate();  // Instant show
panel.HideImmediate();  // Instant hide
```

**Inspector Settings:**
- Show Direction: Right/Left/Up/Down
- Hide Direction: Right/Left/Up/Down
- Animation Duration: 0.3
- Disable On Hide: true
- Start Hidden: true/false

---

### AnimatedButton
Attach to buttons for automatic hover/press animations.

**Automatic behavior:**
- Hover → Scale to 1.1x
- Press → Scale to 0.95x
- Exit → Scale to 1.0x

**Manual methods:**
```csharp
AnimatedButton button = GetComponent<AnimatedButton>();
button.Pulse(intensity: 0.2f, loops: 1);  // Manual pulse
button.ResetScale();  // Reset to original
```

---

### AnimatedPopup
Attach to popup dialogs for bounce animation.

```csharp
AnimatedPopup popup = GetComponent<AnimatedPopup>();
popup.Show();  // Bounce in
popup.Hide();  // Shrink out
```

**Inspector Settings:**
- Show Duration: 0.5
- Hide Duration: 0.3
- Use Unscaled Time: false (true for pause menus)
- Fade Background: true
- Start Hidden: true

---

## Effects

### FloatingTextAnimator

```csharp
using PixelVanguard.UI.Animations;

// Show floating text
FloatingTextAnimator.Show("+100", worldPosition, Color.green, fontSize: 42f);

// Helper methods
FloatingTextAnimator.ShowGoldEarned(100, worldPosition);
FloatingTextAnimator.ShowGoldSpent(50, uiTransform);
FloatingTextAnimator.ShowPositive("Level Up!", worldPosition);
FloatingTextAnimator.ShowNegative("Hit!", worldPosition);
```

---

### CoinAnimator

```csharp
using PixelVanguard.UI.Animations;

// Animate single coin from world to UI
CoinAnimator.AnimateCoinToTarget(
    worldStartPos, 
    goldIconTransform, 
    goldAmount: 10,
    onComplete: () => Debug.Log("Coin collected!")
);

// Animate multiple coins with stagger
CoinAnimator.AnimateMultipleCoins(
    worldStartPos,
    goldIconTransform,
    coinCount: 5,
    goldPerCoin: 10,
    staggerDelay: 0.05f,
    onAllComplete: () => Debug.Log("All coins collected!")
);

// Reverse: coins from UI to item (for purchases)
CoinAnimator.AnimateCoinsFromUIToItem(
    goldIconTransform,
    itemTransform,
    coinCount: 3
);
```

---

## Navigation

### MenuNavigationController

```csharp
// Automatically managed by MainMenuManager
// Manual usage example:

MenuNavigationController nav = GetComponent<MenuNavigationController>();

// Initialize with root panel
nav.Initialize(mainMenuPanel);

// Navigate forward (slides right)
nav.NavigateToPanel(shopPanel);

// Navigate back (slides left)
nav.NavigateBack();

// Back to root (clear stack)
nav.NavigateBackToRoot();

// Check state
bool isAtRoot = nav.IsAtRoot();
int depth = nav.GetStackDepth();
bool transitioning = nav.IsTransitioning();
```

---

## Complete Usage Examples

### Main Menu Navigation

```csharp
// Already integrated in MainMenuManager
private void OnShopClicked()
{
    NavigateToPanel(shopPanel);  // Uses MenuNavigationController
}

public void ReturnToMainMenu()
{
    navigationController.NavigateBack();  // Slides back
}
```

---

### Level Up Popup

```csharp
// Already integrated in LevelUpPanel
private void ShowLevelUpOptions()
{
    Time.timeScale = 0;  // Pause game
    
    panelRoot.SetActive(true);
    
    // Popup animation
    UIAnimator.ShowPopup(panelRoot.transform, 0.5f, useUnscaledTime: true);
    
    // Stagger animate cards
    for (int i = 0; i < upgradeCards.Length; i++)
    {
        upgradeCards[i].localScale = Vector3.zero;
        upgradeCards[i].DOScale(Vector3.one, 0.3f)
            .SetDelay(0.5f + i * 0.1f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
}
```

---

### Purchase Feedback (Example)

```csharp
using PixelVanguard.UI.Animations;

public void OnPurchaseUpgrade(int cost)
{
    // Deduct gold
    currentGold -= cost;
    
    // Animate coins from UI to item
    CoinAnimator.AnimateCoinsFromUIToItem(
        goldIconTransform,
        upgradeCardTransform,
        coinCount: 3
    );
    
    // Show floating text
    FloatingTextAnimator.ShowGoldSpent(cost, goldIconTransform);
    
    // Shake gold counter
    UIAnimator.Shake(goldIconTransform, strength: 10f, duration: 0.3f);
    
    // Count down gold
    UIAnimator.AnimateValue(goldText, currentGold + cost, currentGold, 0.5f);
    
    // Pulse purchased item
    UIAnimator.PunchScale(upgradeCardTransform, strength: 0.2f);
}
```

---

### Coin Collection (Example)

```csharp
using PixelVanguard.UI.Animations;

public void OnCoinPickup(Vector3 coinWorldPos, int goldAmount)
{
    // Animate coin to UI
    CoinAnimator.AnimateCoinToTarget(
        coinWorldPos,
        goldUIIconTransform,
        goldAmount,
        onComplete: () =>
        {
            // Update gold value
            currentGold += goldAmount;
            
            // Count up animation
            UIAnimator.AnimateValue(goldText, currentGold - goldAmount, currentGold, 0.5f);
            
            // Play sound
            AudioManager.Instance.PlaySFX("coin_collect");
        }
    );
}
```

---

## Tips & Best Practices

### 1. Use Unscaled Time for Pause Menus
```csharp
UIAnimator.ShowPopup(transform, 0.5f, useUnscaledTime: true);
```

### 2. Always Clean Up Tweens
```csharp
private void OnDestroy()
{
    UIAnimator.KillTweens(transform);
    // Or DOTween.Kill(transform);
}
```

### 3. Chain Animations with Callbacks
```csharp
UIAnimator.HidePanel(panel, SlideDirection.Right, 0.3f, () =>
{
    // Called when animation completes
    panel.SetActive(false);
});
```

### 4. Stagger for Polish
```csharp
for (int i = 0; i < items.Length; i++)
{
    float delay = i * 0.1f;
    items[i].DOScale(1f, 0.3f).SetDelay(delay);
}
```

### 5. Combine Effects
```csharp
Sequence seq = DOTween.Sequence();
seq.Append(transform.DOScale(1.2f, 0.2f));
seq.Append(transform.DOScale(1f, 0.2f));
seq.Join(canvasGroup.DOFade(1f, 0.4f));
```

---

## Common Patterns

### Menu Panel Transition
```csharp
// Forward (slides from right)
MenuNavigationController.NavigateToPanel(targetPanel);

// Back (slides to right)
MenuNavigationController.NavigateBack();
```

### Popup Show/Hide
```csharp
// Show with bounce
AnimatedPopup.Show();

// Hide with shrink
AnimatedPopup.Hide();
```

### Button Feedback
```csharp
// Automatic via AnimatedButton component
// Or manual:
UIAnimator.PunchScale(buttonTransform, 0.2f);
```

### Currency Animation
```csharp
// Earning
CoinAnimator.AnimateCoinToTarget(pickupPos, goldUI, amount);
FloatingTextAnimator.ShowGoldEarned(amount, pickupPos);

// Spending
CoinAnimator.AnimateCoinsFromUIToItem(goldUI, itemUI, 3);
FloatingTextAnimator.ShowGoldSpent(cost, goldUI);
```

---

## Performance Considerations

- **Pooling**: For repeated animations (coins, floating text), consider object pooling
- **Batch Animations**: Use `Sequence` to group related animations
- **Kill Tweens**: Always kill active tweens in `OnDestroy()`
- **Unscaled Time**: Use sparingly (only for pause menus)

---

## Troubleshooting

### Animation doesn't play
- Verify component is attached
- Check that GameObject is active
- Ensure RectTransform exists (for UI)
- Verify DOTween is imported

### Wrong slide direction
- Check `showDirection` / `hideDirection` in AnimatedPanel
- Verify navigation controller is using correct direction logic

### Jittery animations
- Reduce animation count
- Check for multiple tweens on same transform
- Kill previous tweens before starting new ones

### Animations too fast/slow
- Adjust `duration` parameter
- For buttons: change `animationDuration` in AnimatedButton
- For panels: change `animationDuration` in AnimatedPanel

---

That's it! The system is designed to be simple yet powerful. Start with basic animations and add complexity as needed. 🚀
