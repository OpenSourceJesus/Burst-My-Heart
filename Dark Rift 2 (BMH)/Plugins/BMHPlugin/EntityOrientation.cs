using System.Windows;

namespace BMHPlugin
{
    public class EntityOrientation
    {
        public Vector position;
        public float rotation;

        public EntityOrientation(Vector position, float rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}
