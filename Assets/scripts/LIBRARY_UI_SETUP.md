# Library UI – Wiring in Unity

## 1. ScrollView hierarchy

1. Create **UI → Scroll View** (Canvas will be created if needed).
2. Under the Scroll View:
   - Use the default **Viewport** and **Content** (or add a child named `Content` under Viewport).
3. Select **Content**:
   - Add **Vertical Layout Group** (or ensure it’s present):
     - Child Alignment: Upper Left
     - Control Child Size: Width enabled, Height enabled (or Height only if you prefer flexible height)
     - Child Force Expand: Width enabled, Height disabled (so rows don’t stretch vertically unless you want)
   - Add **Content Size Fitter**:
     - Vertical Fit: Preferred Size (so the scroll area grows with the list)
   - Optional: add a **Layout Element** on Content and set Min Height so the list doesn’t collapse when empty.

## 2. Track item prefab

1. Create **UI → Panel** (or empty GameObject with RectTransform) and name it `TrackItemPrefab`.
2. Add the **TrackItemUI** script to it.
3. Add child **UI → Text - TextMeshPro** (or legacy Text) for the title:
   - Name: `TitleText`
   - Anchor to top-left of the row; set font size and color.
4. Add another Text (TMP) for the artist:
   - Name: `ArtistText`
   - Place under or next to the title.
5. Optional bundled icon:
   - Add **UI → Image** (or a child with an icon sprite).
   - Name: `BundledIcon`
   - Assign a small icon sprite; it will be shown only when `isBundled` is true.
6. Assign references on **TrackItemUI**:
   - **Title Text** → TitleText
   - **Artist Text** → ArtistText
   - **Bundled Icon** → BundledIcon (optional; leave empty to hide icon logic).
7. Set the prefab’s **RectTransform** height (e.g. 40–60) so the layout gives each row a consistent size.
8. Drag the GameObject into the Project window to create a **prefab**, then remove the instance from the scene if you don’t want it in the hierarchy.

## 3. Library UIController

1. Create an empty GameObject (e.g. `LibraryUI`) or use the Canvas.
2. Add the **LibraryUIController** script.
3. Assign:
   - **Library Manager** → The GameObject that has `LibraryManager` (or leave empty to use `FindObjectOfType`).
   - **Track Item Prefab** → Your `TrackItemPrefab` (with `TrackItemUI`).
   - **Content** → The **Content** RectTransform under the Scroll View (the one with Vertical Layout Group).

## 4. Execution order

- **LibraryManager** must run first (loads library in `Awake` and fires `OnLibraryChanged`).
- **DemoTrackInitializer** runs in `Start` and may add demo tracks (which triggers a refresh).
- **LibraryUIController** subscribes in `OnEnable` and calls `Refresh()`; it will refresh again when `OnLibraryChanged` fires.

No need to change script execution order if `LibraryManager` and `DemoTrackInitializer` are in the scene and `LibraryUIController` is on an active GameObject when the scene loads.

## 5. Optional: refresh button

- Add a Button and in On Click assign **LibraryUIController → Refresh()** if you want a manual refresh.

## 6. Optional: remove track

- On **TrackItemUI** you can add a “Remove” button and use `TrackItemUI.TrackId` to call `LibraryManager.RemoveTrack(trackId)`; the UI will refresh via `OnLibraryChanged`.
