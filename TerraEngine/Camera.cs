using OpenTK.Mathematics;

namespace TerraEngine
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _front;
        private Vector3 _up;
        private Vector3 _right;
        private Vector3 _worldUp;

        private float _yaw;
        private float _pitch;
        private float _speed;
        private float _sensitivity;

        public Camera(Vector3 position, Vector3 up, float yaw, float pitch)
        {
            _position = position;
            _worldUp = up;
            _yaw = yaw;
            _pitch = pitch;
            _front = new Vector3(0.0f, 0.0f, -1.0f);
            _speed = 2.5f;
            _sensitivity = 0.1f;

            UpdateCameraVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(_position, _position + _front, _up);
        }

        public void ProcessKeyboard(string direction, float deltaTime)
        {
            float velocity = _speed * deltaTime;
            if (direction == "FORWARD")
                _position += _front * velocity;
            if (direction == "BACKWARD")
                _position -= _front * velocity;
            if (direction == "LEFT")
                _position -= _right * velocity;
            if (direction == "RIGHT")
                _position += _right * velocity;
        }

        public void ProcessMouseMovement(float xoffset, float yoffset)
        {
            xoffset *= _sensitivity;
            yoffset *= _sensitivity;

            _yaw += xoffset;
            _pitch += yoffset;

            if (_pitch > 89.0f)
                _pitch = 89.0f;
            if (_pitch < -89.0f)
                _pitch = -89.0f;

            UpdateCameraVectors();
        }

        private void UpdateCameraVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            _front = Vector3.Normalize(front);
            _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
