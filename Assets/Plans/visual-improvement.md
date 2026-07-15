# Project Overview
- **Game Title**: Flying Burger (FlyingBurguer)
- **High-Level Concept**: A fast-paced, arcade-style cooking game where the player runs a mobile food stand, preparing delicious burgers, fries, and drinks, and serving a variety of animal customers in a vibrant, stylized setting.
- **Players**: Single player
- **Inspiration / Reference Games**: Overcooked, Cook, Serve, Delicious!
- **Tone / Art Direction**: Low-poly, cartoon, cute, vibrant, cozy pastel aesthetics.
- **Target Platform**: PC (StandaloneWindows64)
- **Screen Orientation / Resolution**: Landscape (1920x1080)
- **Render Pipeline**: URP (Universal Render Pipeline)

# Game Mechanics
## Core Gameplay Loop
The player takes orders from animal customers, grills meat patties, deep-fries potato chips, pours soda cups, and assembles custom burgers (adding buns, lettuce, cheese, tomatoes, etc.) at specialized food-prep stations. The completed orders are placed on a tray and served to customers before their patience timer runs out to score points and advance through levels.

## Controls and Input Methods
Smooth movement and interaction using keyboard/mouse or controller inputs integrated with Unity's New Input System, allowing players to navigate the kitchen, hold items, trigger cooking stations, and package items.

# UI
The game uses cartoonish head-up-display (HUD) overlays including order speech bubbles above animal customers, station timers, level timers, and current score metrics to maintain an immersive and clean arcade look.

# Key Asset & Context
- **Kitchen Master Prefab**: `Assets/Prefabs/Cenario_Rolote_Demo.prefab`
  - *Prefab Identification*: The kitchen environment in all three levels is a direct instance of this master prefab. Changes to materials and non-interactive decorations inside this prefab will propagate cleanly to `Level1`, `Level2`, and `Level3`.
- **Target Materials**:
  - `camião.mat`: Camião / Paredes (Pastel Sky Blue #BDE0FE)
  - `Balcão.mat`: Superfície do Balcão Principal (Soft Apricot Peach #FCD5CE)
  - `Balcao2.mat`: Estrutura do Balcão Secundário / Metal (Soft Pastel Slate Blue #D6E2E9)
  - `Balcao2.001.mat`: Estrutura do Suporte de Ingredientes / Metal (Soft Pastel Slate Blue #D6E2E9)
  - `Wood.001.mat`: Mesa de Montagem / Assembly Table (Pastel Rose Coral #FFB5A7)
  - `frigorifico.001.mat`: Frigorífico (Soft Vanilla Cream #FFF1E6)
  - `Fritadeira.001.mat`: Fritadeira (Pastel Lavender #DBCDF0)
  - `initialShadingGroup.001.mat`: Balde do Lixo / Trash Bin (Pastel Sage Green #C9E4DE)
- **Cameras**: `Player Camera` (one per scene).
- **Post-Processing**: `Assets/DefaultVolumeProfile.asset` or a dedicated new profile `Assets/Settings/CozyKitchenPostProcessing.asset`.

# Implementation Steps

### Step 1: Scope Boundary Verification
- **Description**: Confirm that all interactable objects, scripts, colliders, and level mechanics inside the scenes and prefabs remain completely untouched. Ensure that the kitchen is treated as a prefab instance (`Cenario_Rolote_Demo.prefab`) so edits apply globally.
- **Assigned role**: explorer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Material Visual Refinement (Smoothness & Metallic)
- **Description**: Refine the physically plausible material properties on the target URP Lit materials to create a premium, soft stylized cartoon look (removing harsh shininess or uniform flat shading):
  - `Balcão.mat` & `Wood.001.mat`: Smoothness = `0.15` (soft satin, matte clay feel).
  - `Balcao2.mat` & `Balcao2.001.mat` (Steel Shelves): Metallic = `0.7`, Smoothness = `0.35` (semi-matte satin steel look).
  - `frigorifico.001.mat` (Fridge): Metallic = `0.1`, Smoothness = `0.2` (smooth enameled ceramic look).
  - `Fritadeira.001.mat` (Fryer): Metallic = `0.8`, Smoothness = `0.4` (polished but stylized metal look).
  - `initialShadingGroup.001.mat` (Trash Bin): Smoothness = `0.12` (matte molded plastic).
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 3: Non-Interactive Decorative Props Placement
- **Description**: Enhance the visual interest of the kitchen by placing small, non-interactive decoration objects on empty shelves and empty counter corners inside the `Cenario_Rolote_Demo.prefab`.
  - Create a new parent GameObject `Decorations_NonInteractive` inside the prefab.
  - Instantiate decorative copies of existing models (like clean cups, decorative stacks of plates or bun models, pepper/tomato visuals) in background spots.
  - **Constraint Enforcement**: Remove all colliders, rigidbodies, scripts, and triggers from these added decorative models so they can never interfere with player movement or gameplay.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 4: Shadow & Ambient Occlusion Fine-Tuning
- **Description**: Eliminate low-poly visual artifacts and ground the objects:
  - Adjust the `Directional Light`'s `Shadow Bias` (`0.05` to `0.1`) and `Normal Bias` (`0.4` to `0.6`) in all three levels to fix low-poly shadow-acne artifacts.
  - Set the work area spot lights to cast subtle soft shadows if performance allows, grounding the kitchen appliances onto the floor.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 5: Subtle Post-Processing Integration
- **Description**: Add cinematic warmth and visual polish to the levels:
  - Create a new post-processing profile `Assets/Settings/CozyKitchenPostProcessing.asset` (or use `DefaultVolumeProfile`).
  - Configure subtle overrides:
    - **Bloom**: Intensity `0.15`, Threshold `0.9` (adds a gentle, warm glow around emissions and bright spots).
    - **Vignette**: Intensity `0.22`, Smoothness `0.8` (draws focus beautifully toward the center).
    - **Color Adjustments**: Saturation `+6`, Contrast `+5` (makes the cozy pastel palette pop).
    - **Tonemapping**: Mode `ACES` (cinematic color response).
  - For each scene (`Level1`, `Level2`, `Level3`), enable the `Post Processing` option on the `Player Camera`'s `UniversalAdditionalCameraData` component.
  - Create a Global Volume GameObject with a `Volume` component referencing our custom profile in each level.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

# Verification & Testing
- **Gameplay Integrity Check**: Play through Level1, Level2, and Level3. Verify that the player controller, stations (cooking, cutting, assembling), delivery system, and UI work exactly as they did before.
- **Visual Validation**: Take screenshots of Level3 kitchen from the player's view before and after. Verify that:
  - Highlights and reflections on metals (shelves, fryer) are soft and stylized.
  - Surfaces (wooden table, counter tops) have a clean matte clay/satin feel.
  - Added non-interactive props neatly populate the background without clipping.
  - Post-processing is subtle, warm, and does not darken the working areas.
- **Console Log Verification**: Ensure zero compiler errors, null-reference warnings, or physics warnings.
