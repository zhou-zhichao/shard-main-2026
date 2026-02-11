/*
*
*   This class is the base class for all sound systems. It provides a common interface for playing sounds and updating the sound system.
*   @author Yuxi Guo
*   @version 1.1
*   
*/

namespace Shard
{
    abstract public class Sound
    {
        // similar with Unity AudioSource.volume(from 0.0 to 1.0)
        public float Volume { get; set; } = 1.0f;

        // add loop to support BGM
        abstract public void playSound(string file, bool loop = false);

        // add per frame update method to clean up resources that have finished playback
        abstract public void update();
    }
}
