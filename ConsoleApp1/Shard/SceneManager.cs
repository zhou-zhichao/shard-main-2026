/*
*
*   The SceneManager is a singleton responsible for managing scene lifecycle.
*       It holds the current active scene, handles transitions between scenes,
*       and ensures proper cleanup of game objects, physics bodies, and input
*       listeners during transitions.
*   @author Scene Management System
*   @version 1.0
*
*/

namespace Shard
{
    class SceneManager
    {
        private static SceneManager me;

        private Scene currentScene;
        private Scene pendingScene;

        private SceneManager()
        {
        }

        public static SceneManager getInstance()
        {
            if (me == null)
            {
                me = new SceneManager();
            }

            return me;
        }

        /// <summary>
        /// The currently active scene, or null if no scene has been loaded yet.
        /// </summary>
        public Scene CurrentScene
        {
            get => currentScene;
        }

        /// <summary>
        /// Initializes the SceneManager with the first scene. Call this once
        /// from Game.initialize().
        /// </summary>
        public void initialize(Scene firstScene)
        {
            currentScene = firstScene;
            currentScene.initialize();
        }

        /// <summary>
        /// Schedules a scene transition. The actual transition happens at the
        /// end of the current frame (in update()), so it is safe to call this
        /// from anywhere during the frame.
        /// </summary>
        public void loadScene(Scene nextScene)
        {
            pendingScene = nextScene;
        }

        /// <summary>
        /// Called every frame by Bootstrap. Updates the current scene and, if
        /// a transition is pending, performs the transition at the end of the frame.
        /// </summary>
        public void update()
        {
            if (currentScene != null)
            {
                currentScene.update();
            }

            // Process pending scene transition at end of frame
            if (pendingScene != null)
            {
                performTransition();
            }
        }

        /// <summary>
        /// Performs the actual scene transition: exits the old scene, clears all
        /// engine state, then initializes the new scene.
        /// </summary>
        private void performTransition()
        {
            // 1. Let the old scene do custom cleanup
            if (currentScene != null)
            {
                currentScene.onExit();
            }

            // 2. Clear all engine-managed state
            GameObjectManager.getInstance().clearAll();
            PhysicsManager.getInstance().clearAll();
            Bootstrap.getInput().clearListeners();

            // 3. Switch to the new scene
            currentScene = pendingScene;
            pendingScene = null;

            // 4. Initialize the new scene
            currentScene.initialize();
        }
    }
}
