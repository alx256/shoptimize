# Shoptimise

## Introduction

Shoptimise is a project created with the Unity game engine and C# that pits different agents against each other to see who can get the best deal in a shop sale.

More formally, this simulated environment replicates a variation of the [_unbounded knapsack problem_](https://en.wikipedia.org/wiki/Knapsack_problem) with the additional [_online_](https://en.wikipedia.org/wiki/Online_optimization) complexity of all agents starting with no knowledge of the deals on offer and having to explore the environment to discover them. This simulates the situation that robots would be in if they were to try and get the best deal in a real-life physical or virtual sale and has further (potentially more useful) applications in circumstances like automated logistics or warehouse management.

## Agents

To do this, agents participating in the competition have different combinations of both _exploration_ and _selection_ strategies. The exploration strategies are as follows:

- **Random Exploration** Agent rotates to face a random direction and walks in that direction until it hits a wall in which case it repeats.
- **ShelfExplore** Simple reactive/state-based algorithm that uses a wall-follower maze solving approach to locate and explore each shelf with items.
- **Reinforcement Learning** Reinforcement learning approach using Unity's [ML-Agents](https://github.com/Unity-Technologies/ml-agents) package.

The selection strategies are as follows:

- **Savings-Size Ratio** Only items that have a ratio between the amount saved and the size of the object above a specified threshold are selected.
- **Dynamic Programming** Agent regularly attempts to calculate the optimal combination of items using the [_UKP5_](https://link.springer.com/chapter/10.1007/978-3-319-38851-9_4) algorithm based on which items have been discovered.
- **Genetic Algorithm** Agent regularly finds a near-optimal or optimal combination of items based on a dynamically growing list of items that have been discovered.

## Future
The project is far from perfect. Agents still possess unexpected behaviour and many more selection and discovery strategies can still be implemented.