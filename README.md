# Rush Platform

A 2.5D action platformer game developed in Unity, featuring advanced player movement mechanics, intelligent enemy AI, and dynamic combat systems.

## 🎮 Game Overview

Rush Platform is a challenging 3D platformer where players navigate through multiple levels, collect coins, defeat enemies, and overcome environmental obstacles. The game features responsive controls, sophisticated enemy AI, and a comprehensive save system.

## ✨ Key Features

### Player Mechanics

- **Advanced Movement System**: Physics-based movement with coyote time, jump buffering, and variable jump heights
- **Combat System**: Multiple projectile firing patterns (Single, Double, Triple, Spread) with object pooling
- **Health System**: Player health management with visual feedback and damage mechanics

### Enemy AI

- **State-Driven Behavior**: Finite state machines controlling Patrolling, Chasing, and Attacking states
- **Intelligent Detection**: Dual detection system combining distance zones with raycast line-of-sight
- **Environmental Awareness**: Enemies navigate obstacles, detect walls, and avoid falling off ledges
- **Dynamic Combat**: Attack cooldowns, damage dealing, and strategic positioning

### Game Systems

- **Save/Load System**: Persistent game progress with JSON serialization
- **Level Progression**: Multiple interconnected levels with increasing difficulty
- **Collectible System**: Coin collection with persistent tracking across sessions
- **Input Management**: Modern Unity Input System with cross-platform compatibility

### Technical Features

- **Object Pooling**: Optimized projectile and effect management
- **Physics Integration**: Rigidbody-based movement and collision detection
- **UI Toolkit**: Modern UI implementation using Unity's UI Toolkit (UXML/USS)
- **Audio System**: Dynamic sound effects and background music

## 🛠️ Technologies Used

- **Game Engine**: Unity 2022.3 LTS
- **Programming Language**: C#
- **Input System**: Unity New Input System
- **UI Framework**: Unity UI Toolkit
- **Physics**: Unity Physics Engine
- **Audio**: Unity Audio System
- **Version Control**: Git

## 🎯 Gameplay

### Controls

- **Movement**: WASD or Arrow Keys
- **Jump**: Spacebar
- **Run**: Hold Shift
- **Shoot**: Left Mouse Button
- **Pause**: Escape

### Objectives

1. Navigate through platforming challenges
2. Collect coins to increase your score
3. Defeat enemies using your projectile weapons
4. Avoid environmental hazards (spikes, spinning blades, press traps)
5. Reach the level exit to progress

### Enemy Types

- **Patrolling Enemies**: Basic enemies that patrol areas and chase when detected
- **Combat Enemies**: Enemies that can attack and deal damage to the player
- **Environmental Hazards**: Static and moving obstacles that damage the player

## 🚀 Installation & Setup

### Prerequisites

- Unity 2022.3 LTS or later
- Basic knowledge of Unity Editor

### Setup Instructions

1. Clone or download the project files
2. Open Unity Hub and add the project
3. Open the project in Unity
4. Navigate to `Assets/Scenes/MainMenu.unity`
5. Press Play to start the game

### Project Structure

```
Assets/
├── Scripts/           # C# game logic
│   ├── Player/       # Player movement and health
│   ├── Enemy/        # Enemy AI and behavior
│   ├── Shooting/     # Combat and projectile systems
│   ├── Input/        # Input management
│   └── Systems/      # Game management and save systems
├── Scenes/           # Game levels and menus
├── Prefabs/          # Reusable game objects
├── Materials/        # Visual materials and shaders
├── Models/           # 3D models and animations
├── Audio/            # Sound effects and music
└── UI Toolkit/       # User interface elements
```

## 🎨 Art Assets

This project uses various assets from the Unity Asset Store:

- **Character Models**: Robot Hero PBR HP Polyart, Meshtint Free Knight
- **Environment**: Fantasy Skybox FREE, Asian Style Trees, Obstacle Pack
- **Effects**: Warped Shooting FX, Particle Systems
- **UI**: 2D Simple UI Pack
- **Audio**: Shooting Sound effects

## 🔧 Development Features

### Code Architecture

- **Singleton Pattern**: Used for GameManager and InputManager
- **Component-Based Design**: Modular systems that can be easily modified
- **Event-Driven Systems**: Scene loading events and game state management
- **Data Persistence**: Comprehensive save/load system with serialization

### Performance Optimizations

- **Object Pooling**: Efficient memory management for projectiles and effects
- **Layer-Based Collision**: Optimized collision detection using Unity layers
- **Efficient Raycasting**: Smart use of raycasts for AI detection
- **Asset Management**: Proper asset organization and loading

## 🎮 Game Levels

1. **Main Menu**: Game start and level selection
2. **Level 1**: Introduction to basic mechanics
3. **Level 2**: Intermediate challenges with more enemies
4. **Level 3**: Advanced platforming and combat scenarios
