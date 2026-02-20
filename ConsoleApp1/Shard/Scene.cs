/*
*
*   Abstract base class for scenes. Any scene in the game (main menu, gameplay,
*       pause screen, etc.) should extend from this class.
*   @author Scene Management System
*   @version 1.0
*
*/

namespace Shard
{
    abstract class Scene
    {
        /// <summary>
        /// Called once when the scene is first loaded by the SceneManager.
        /// Use this to create game objects, register input listeners, etc.
        /// </summary>
        public abstract void initialize();

        /// <summary>
        /// Called every frame while this scene is the active scene.
        /// Use this for per-frame rendering commands (showText, addToDraw, etc.).
        /// </summary>
        public abstract void update();

        /// <summary>
        /// Called when the scene is about to be unloaded (before a scene transition).
        /// Override this to perform any custom cleanup beyond the automatic cleanup
        /// done by the SceneManager (which clears GameObjects, physics, and input listeners).
        /// </summary>
        public virtual void onExit()
        {
        }
    }
}
