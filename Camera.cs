using OpenTK.Mathematics;

namespace MirageDev.Mirage
{
	public class Camera
	{
		public Vector3 position = new(0f, 0f, 3f);

		public Vector3 right;
		public Vector3 up;
		public Vector3 front;

		public float yaw = -MathHelper.PiOver2;
		public float pitch;

		public void Update()
		{
			front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
			front.Y = MathF.Sin(pitch);
			front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);
			front = Vector3.Normalize(front);

			right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
			up = Vector3.Normalize(Vector3.Cross(right, front));
		}

		public Matrix4 CreateViewMatrix()
		{
			return Matrix4.LookAt(position, position + front, up);
		}
	}
}
