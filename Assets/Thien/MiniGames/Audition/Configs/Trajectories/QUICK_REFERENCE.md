# 🎮 Beat Trajectory - Quick Reference Card

## 🚀 Quick Start (30 seconds)

1. **Create**: Right-click → `Create > Audition > Trajectories > [Type]`
2. **Configure**: Adjust parameters in Inspector
3. **Assign**: Drag to `PhaseData.trajectoryConfig`
4. **Test**: Play and see the pattern!

---

## 📏 Linear Trajectory ⭐

**Best for**: Tutorial, Intro, Easy phases

### Common Presets

| Name | Angle | Length | Use Case |
|------|-------|--------|----------|
| Horizontal Left→Right | 0° | 200 | Tutorial |
| Horizontal Right→Left | 180° | 200 | Return pattern |
| Vertical Bottom→Top | 90° | 200 | Build-up |
| Vertical Top→Bottom | 270° | 200 | Drop |
| Diagonal ↗ | 45° | 200 | Transition |
| Diagonal ↖ | 135° | 200 | Variation |

### Quick Config
```
✓ Use Angle: true
  Angle: [0-360°]
  Length: [100-200]
```

---

## ⭕ Circular Trajectory ⭐⭐

**Best for**: Chorus, Dynamic sections, Medium difficulty

### Common Presets

| Name | Start | End | Radius | Clockwise | Use Case |
|------|-------|-----|--------|-----------|----------|
| Half Circle Top | 180° | 0° | 100 | ✓ | Smooth arc |
| Half Circle Bottom | 0° | 180° | 100 | ✓ | Reverse arc |
| Quarter Circle | 0° | 90° | 120 | ✓ | Corner turn |
| Full Circle | 0° | 360° | 80 | ✓ | Loop |
| Reverse Circle | 0° | 360° | 80 | ✗ | Counter-loop |

### Quick Config
```
  Radius: [80-120]
  Start Angle: [0-360°]
  End Angle: [0-360°]
✓ Clockwise: true/false
```

---

## ⚡ ZigZag Trajectory ⭐⭐⭐

**Best for**: Intense sections, Drops, Hard difficulty

### Common Presets

| Name | Direction | Amplitude | Count | Pattern | Use Case |
|------|-----------|-----------|-------|---------|----------|
| Horizontal ZigZag | 0° | 50 | 3 | Sharp | Fast section |
| Vertical ZigZag | 90° | 40 | 3 | Sharp | Intense |
| Smooth Wave | 0° | 30 | 2 | Smooth | Flowing |
| Lightning | 90° | 60 | 5 | Sharp | Extreme |

### Quick Config
```
  Direction: [0-360°]
  Amplitude: [30-60]
  ZigZag Count: [2-5]
  Total Length: [150-250]
  Pattern: Sharp / Smooth
```

---

## 🎯 Difficulty Guide

### Easy (Tutorial)
```
Linear Horizontal
  angle = 0°
  length = 150
```

### Medium (Normal)
```
Circular Half Arc
  radius = 100
  startAngle = 180°
  endAngle = 0°
```

### Hard (Challenge)
```
ZigZag Smooth
  direction = 0°
  amplitude = 40
  count = 3
  pattern = Smooth
```

### Extreme (Boss)
```
ZigZag Sharp
  direction = 90°
  amplitude = 60
  count = 5
  pattern = Sharp
```

---

## 🎨 Music Sync Guide

| Music Section | Recommended Trajectory | Why |
|---------------|------------------------|-----|
| **Intro** | Linear (slow) | Easy to follow |
| **Verse** | Linear or Circular (small) | Predictable |
| **Pre-Chorus** | Circular (medium) | Building tension |
| **Chorus** | Circular (large) | Dynamic, engaging |
| **Bridge** | ZigZag Smooth | Variation |
| **Drop** | ZigZag Sharp | Intense |
| **Outro** | Linear (slow) | Wind down |

---

## ⚙️ Common Parameters

### Boundary Radius
- **Small** (50-80): Tight pattern, easier
- **Medium** (80-120): Standard
- **Large** (120-200): Wide pattern, harder

### Beat Count vs Trajectory
- **Few beats** (5-10): Simple trajectories (Linear, small Circular)
- **Medium** (10-20): Any trajectory
- **Many beats** (20+): Complex trajectories (ZigZag, full Circle)

---

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| Beats spawn outside screen | ↓ Decrease `Position Radius` |
| Pattern too small | ↑ Increase trajectory size (length/radius/amplitude) |
| Pattern too predictable | Use ZigZag or increase complexity |
| Pattern too hard | Use Linear or decrease amplitude |
| Want random spawning | Set `Trajectory Config = null` |

---

## 💡 Pro Tips

1. **Start Simple**: Test with Linear first
2. **Match Music**: Sync trajectory with song structure
3. **Vary Difficulty**: Don't use same trajectory for all phases
4. **Test Playability**: Make sure it's fun, not frustrating
5. **Visual Appeal**: Patterns should look good even when not playing

---

## 📋 Checklist

Before finalizing a phase:

- [ ] Trajectory created and configured
- [ ] Assigned to PhaseData
- [ ] Tested in game
- [ ] Beats don't spawn off-screen
- [ ] Difficulty matches phase intent
- [ ] Pattern syncs with music
- [ ] Fun to play!

---

## 🎯 Examples

### Example 1: Easy Tutorial Phase
```
Trajectory: Linear
  Use Angle: true
  Angle: 0° (horizontal)
  Length: 150

PhaseData:
  Beat Count: 8
  Beat Interval: 1.0s
  Position Radius: 100
```
**Result**: 8 beats in a straight line, easy to follow

---

### Example 2: Medium Chorus Phase
```
Trajectory: Circular
  Radius: 100
  Start Angle: 180°
  End Angle: 0°
  Clockwise: true

PhaseData:
  Beat Count: 12
  Beat Interval: 0.75s
  Position Radius: 120
```
**Result**: 12 beats in a smooth arc, engaging

---

### Example 3: Hard Drop Phase
```
Trajectory: ZigZag
  Direction: 90° (vertical)
  Amplitude: 50
  ZigZag Count: 4
  Pattern: Sharp

PhaseData:
  Beat Count: 16
  Beat Interval: 0.5s
  Position Radius: 150
```
**Result**: 16 beats in sharp zigzag, intense!

---

## 🚀 Quick Commands

**Create Linear**: `Create > Audition > Trajectories > Linear`  
**Create Circular**: `Create > Audition > Trajectories > Circular`  
**Create ZigZag**: `Create > Audition > Trajectories > ZigZag`

---

**Need Help?** Check `README.md` in `Configs/Trajectories/`
