namespace HYT.MidiManager
{
    public class SpeedManager
    {
        private SpeedManager() { }
        public static readonly SpeedManager Instance = new SpeedManager();

        private float _speed=1f;
        public float Speed
        {
            get { return _speed; }
            set {
                if (value<=0)
                {
                    value = 0;
                }
                if (value >= 10f) { value = 10; }
                _speed = value;
            }
        }

    }
}
