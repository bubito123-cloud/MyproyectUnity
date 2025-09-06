# Test Scene Setup Guide

This guide explains how to set up a basic test scene in Unity to verify the functionality of the autonomous agent simulation.

## I. Essential Scene Objects

Your test scene must contain the following objects to function correctly.

1.  **EvolutionManager**:
    *   Create an empty GameObject and name it `_EvolutionManager`.
    *   Attach the `EvolutionManager.cs` script to it.
    *   **Crucially**, you must create an Agent Prefab and assign it to the `Agent Prefab` field in the Inspector.
    *   Assign a `spawnAreaCenter` transform if you want to control the spawn location, otherwise it will default to the manager's position.

2.  **Agent Prefab**:
    *   Create a 3D object (like a Capsule) to represent your agent.
    *   Attach the `ArtificialHumanAgent.cs` script.
    *   Attach all of its required components:
        *   `Rigidbody` (configure mass, drag, etc.)
        *   `EmotionalCore.cs`
        *   `DeliberativePlanner.cs`
        *   `PerceptionSystem.cs`
        *   `Conceptualizer.cs`
        *   `MemoryStore.cs`
        *   A `Collider` component (e.g., `Capsule Collider`).
    *   Save this configured GameObject as a prefab (e.g., drag it into your Project window).
    *   Assign this prefab to the `Agent Prefab` field on your `_EvolutionManager` object.

3.  **Goal Object**:
    *   Create a simple 3D object (like a Cube or Sphere) to act as the goal.
    *   Attach the `Goal.cs` script to it.
    *   The `ArtificialHumanAgent` will find this object automatically at the start of each episode.

4.  **Plane/Floor**:
    *   Create a large `Plane` or `Cube` for the agents to walk on.
    *   Ensure it has a `Collider` component.

5.  **EventSystem and UI (Optional but Recommended)**:
    *   To see the narrator's output, create a `UI > Canvas` and a `UI > Text` element.
    *   Create an empty GameObject named `_Narrator` and attach the `NarratorEngine.cs` script.
    *   On the `_Narrator` object's `On Narrate` event, drag your UI Text object and select the `Text.text` dynamic string function. This will display the agent's thoughts.

## II. Creating Concepts

To test the `Conceptualizer` and `DeliberativePlanner`, you need to add concepts to the scene.

1.  Create a 3D object (e.g., a red sphere for "FUEGO").
2.  Attach the `ConceptTag.cs` script to it.
3.  In the `Concept Name` field, enter the name of the concept exactly as it appears in `knowledge_base.json` (e.g., `FUEGO`).
4.  Make sure the object has a `Collider` set to be a trigger or a solid object.
5.  Place these objects where the agents can perceive them.

## III. How to Run and Verify

1.  **Run the Scene**: Press the Play button in Unity.
2.  **Check the Console Logs**: Open the Console window (`Ctrl+Shift+C`).

### What to Look For:

*   **KnowledgeBridge Loading**:
    *   At the very start, you should see a green message:
        `[KnowledgeBridge] Successfully loaded X concepts from JSON.`
    *   If you see an error, ensure `knowledge_base.json` is in a folder named `StreamingAssets` inside your `Assets` folder.

*   **Generation Start**:
    *   You should see a green message:
        `--- STARTING GENERATION 1 (Population: X) ---`
    *   You should see the agents being spawned in the scene.

*   **Planner Deliberation**:
    *   As agents move, you will see orange and cyan messages from the planner:
        `[Planner] Starting deliberation for agent...`
        `Chosen Goal: ...`
    *   If an agent sees a dangerous concept (like "FUEGO"), you should see a red critical override message.

*   **Fitness Reporting and Evolution**:
    *   When an agent reaches the goal, it will disappear and a new episode will start for it.
    *   Once all agents in the population have completed their episode, you will see a yellow message:
        `--- EVOLVING GENERATION 1 ---`
    *   This will be followed by a report of the top-performing agents.
    *   A new generation will then begin.

*   **Trauma Memory**:
    *   If an agent touches a dangerous object (and you implement the logic for it to take damage and record trauma), you should see a red message:
        `[Memory] Recording traumatic event... caused by 'FUEGO'.`

By following this guide, you can create a comprehensive test environment to see all the interconnected systems working together.
