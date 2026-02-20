/*
*   GameTest - Entry point game class.
*       Uses the SceneManager to load and manage scenes.
*   
*   Contributions to the code made by others:
*   @author Lisa te Braak (see Changelog for 1.3.0) 
*/

using GameTest;
using System;

namespace Shard
{
    class GameTest : Game
    {
        public override void update()
        {
            // Scenes handle their own per-frame updates via SceneManager.
        }

        public override int getTargetFrameRate()
        {
            return 100;
        }

        public override void initialize()
        {
            // Load the first scene
            Bootstrap.getSceneManager().initialize(new MainMenuScene());
        }
    }
}

