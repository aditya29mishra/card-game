### Card Matching Game

A functional prototype of a card-matching memory game developed with Unity.

***

### About the Project

This project is a complete and functional prototype of a classic card-matching game, built from scratch using Unity. The core objective was to create a robust and scalable game loop that meets all the requirements of the project brief, with a focus on code quality, modular design, and essential features.

***

### Key Features

* **Dynamic Grid Layouts:** Supports multiple grid sizes (e.g., 4x4, 5x6) that can be selected dynamically at the start of a new game.
* **Continuous Card Flipping:** The system handles rapid player input, allowing for continuous card flipping without waiting for previous card comparisons to finish.
* **Scoring and Combos:** Includes a scoring system with points for matches and a streak counter for consecutive matches.
* **Persistent Save/Load System:** The game state is saved in `PlayerPrefs` after every significant action, allowing players to resume their progress from where they left off.
* **Basic Audio Integration:** Includes essential sound effects for card flips, matches, mismatches, and game over.
* **Clean UI/UX:** Features a simple and clean UI with an initial card reveal sequence and a basic menu for game flow.

***

### How to Play

1.  Select a grid size from the dropdown menu.
2.  Click "New Game" to start a new game or "Continue" to load a saved one.
3.  The cards will briefly reveal their faces before flipping back over.
4.  Click on any two cards to flip them.
5.  If the cards match, they stay face-up, and you score points. If they don't, they will flip back after a short delay.
6.  The game ends when all cards have been matched.

***

### Technical Breakdown

Here is an explanation of the major code components and core systems implemented in this project:

#### **Game Flow and Structure**
The entire game is managed by the **`GameManager`** script, which acts as a central control hub. This is a common design pattern in Unity. It uses the `public static GameManager Instance;` singleton pattern to ensure that other scripts (like `Card.cs`) can easily communicate with it from anywhere in the game.

#### **Dynamic Grid and Card Spawning**
The game grid is not a static object; it's generated dynamically based on your chosen rows and columns.
* The **`StartNewGame()`** method calculates the total number of cards needed (`rows * cols`).
* It then iterates through a nested loop to instantiate `cardUIPrefab` at calculated positions on the UI Canvas using `RectTransform`.
* A `Card` script is attached to each prefab, and a unique `pairID` and face sprite are assigned.

#### **Card Selection and Pairing**
This is handled entirely within the **`StartNewGame()`** method to ensure a random and fair game every time.
* **Card Pool:** The `allFaceSprites` list holds the full deck of 52 card sprites.
* **Random Selection:** The code creates a `choiceIndices` list by randomly selecting a subset of unique card sprites from the main pool. The number of unique cards selected is equal to `(rows * cols) / 2`.
* **Pairing and Shuffling:** Each chosen card index is added twice to a new list called `faces`, creating pairs. This list is then randomized using the **`ShuffleList()`** method, ensuring the card pairs are scattered across the grid.

#### **Continuous Flipping and Matching**
Your game allows the player to click cards rapidly without waiting for animations to finish.
* **`OnCardClicked(Card card)`:** This method is called by the `Card` script when it is clicked. It adds the clicked card to a `flippedUnmatched` list.
* **Match Check:** When the `flippedUnmatched` list contains two cards, the code compares their `pairID`s.
* **Match Logic:** If the `pairID`s are the same, the cards are marked as matched (`card.IsMatched = true`), score and streak are updated, and the `flippedUnmatched` list is cleared.
* **Mismatch Logic:** If the `pairID`s are different, the game starts a coroutine called **`FlipBackAfterDelay()`**. This coroutine waits for a short duration (`mismatchDelay`) and then flips both cards back over.
* **Input Blocking:** To prevent a third card from being flipped during the `mismatchDelay`, your `OnCardClicked` method first calls **`SetCardsInteractable(false)`** to disable all card buttons. After a match or mismatch is resolved, it calls **`SetCardsInteractable(true)`** to re-enable them.

#### **Scoring and Game Over**
The game's progression is tracked by two simple integers: `score` and `streak`.
* `score` is a cumulative total, incrementing for matches and decrementing for mismatches.
* `streak` increases for consecutive matches and resets to zero on a mismatch, allowing for a future combo system.
* The **`CheckGameOver()`** method is called after every successful match. It loops through all cards to check if they are all matched. If so, it triggers the game-over state.

#### **Save and Load System**
This system allows the player to save their progress and resume the game later.
* **`SaveData` Class:** A custom class is used as a data model to hold all relevant game state information, including the grid dimensions, card IDs, matched status, score, and streak.
* **`SaveGame()`:** This method creates an instance of `SaveData`, fills it with the current game's state, serializes it into a **JSON string** using `JsonUtility.ToJson()`, and saves the string to `PlayerPrefs` with a unique key.
* **`LoadGame()`:** This method retrieves the JSON string from `PlayerPrefs`, deserializes it back into a `SaveData` object, and then uses that data to reconstruct the entire game grid and state, correctly setting each card's `pairID` and `IsMatched` status. It's called by your "Continue" button, allowing the game to start from a saved point.