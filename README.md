# 💎 Crystal Caverns: Battle Run

A 2.5D action-platformer developed in Unity featuring physics-based movement, projectile combat, collectibles, platforming, and state-machine-driven enemy AI.

![Crystal Caverns: Battle Run Gameplay](![alt text](image.png))

---

## 🎮 Overview

**Crystal Caverns: Battle Run** is a 2.5D action-platformer in which the player navigates a crystal-filled environment, collects coins and crystals, moves across platforms, and battles robotic enemies using projectile-based combat.

The project was developed to explore several core areas of game development, including:

- Player movement and physics
- Projectile-based combat
- Enemy AI
- State machines
- Collision detection
- Level design
- UI and player feedback
- Audio and visual effects
- Gameplay balancing
- Playtesting and iteration

The game combines platforming with combat encounters that require the player to balance movement, positioning, shooting, and enemy avoidance.

---

## 🕹️ Gameplay

The core gameplay loop consists of four main elements:

1. **Explore** the platform-based level.
2. **Collect** coins and crystals placed throughout the environment.
3. **Fight** robotic enemies using projectile attacks.
4. **Progress** through the level while managing health and lives.

The game is designed around responsive movement and combat while giving enemies enough intelligence to make encounters less predictable.

---

## 🎯 How to Play

When the game starts, **click anywhere on the game screen with the mouse to begin**.

### Controls

| Action | Control |
|---|---|
| Start Game | Click anywhere on the game screen |
| Move Left | `A` or Left Arrow |
| Move Right | `D` or Right Arrow |
| Jump | `Space` |
| Shoot | `X` |

---

## ⚔️ Combat System

Combat is built around projectile-based attacks.

### Player Combat

- Projectiles are fired using the `X` key
- Approximately **0.2-second firing cooldown**
- Player bullets deal **20 damage**
- Ammunition is unlimited
- Projectile collisions use trigger-based detection
- Objects are automatically cleaned up after use

### Combat Balance

The prototype was tuned around the following values:

| Character | Health | Damage Per Shot | Hits Required |
|---|---:|---:|---:|
| Player | 100 HP | 20 | 5 shots to defeat enemy |
| Enemy | 100 HP | 34 | ~3 shots to defeat player |

The enemy has a slight damage advantage, encouraging the player to use movement, timing, and positioning rather than relying only on continuous shooting.

---

## 🤖 Enemy AI

One of the main technical components of the project is the robot enemy AI.

The enemy behavior is organized using a **five-state state machine**:

```text
Patrol
   ↓
Chase
   ↓
Attack
   ↓
Dodge
   ↓
Dead