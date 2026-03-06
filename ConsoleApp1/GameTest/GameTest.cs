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
            return global::GameTest.DemoSettings.GetTargetFrameRate();
        }

        public override void initialize()
        {
            global::GameTest.DemoRunState.StartNewRun();
            Bootstrap.getSceneManager().initialize(new global::GameTest.DemoLevelScene(global::GameTest.DemoLevelCatalog.GetLevel(1)));
        }

        public override bool isRunning()
        {
            return global::GameTest.DemoRunState.Paused == false;
        }

        public override bool useGlobalLauncherEscapeShortcut()
        {
            return false;
        }
    }
}

