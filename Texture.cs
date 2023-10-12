using StbImageSharp;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

namespace MirageDev.Mirage
{
	public class Texture
	{
		public int Handle;

		public Texture(Color4 color, int w, int h, PixelInternalFormat internalFormat=PixelInternalFormat.Rgba, PixelFormat format=PixelFormat.Rgba, PixelType pType=PixelType.UnsignedByte)
		{
			Handle = GL.GenTexture();
			Use();

			char[] colorChar = new char[] { (char)color.R, (char)color.G, (char)color.B, (char)color.A };
			char[] data = new char[4 * w * h * sizeof(char)];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = colorChar[i % 4];
			}
			GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, w, h, 0, format, pType, data);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}

		public Texture(string path)
		{
			Handle = GL.GenTexture();

			Use();

			StbImage.stbi_set_flip_vertically_on_load(1);
			ImageResult img = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}

		public void Use(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
		}
	}
}
