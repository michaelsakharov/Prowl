using System;
using Silk.NET.OpenGL;

namespace Prowl.Runtime
{
    /// <summary>
    /// A <see cref="Texture"/> containing an array of two-dimensional images and support for multisampling
    /// </summary>
    public sealed class Texture2DArray : Texture
    {
        /// <summary>The width of this <see cref="Texture2DArray"/>.</summary>
        public uint Width { get; private set; }

        /// <summary>The height of this <see cref="Texture2DArray"/>.</summary>
        public uint Height { get; private set; }

        /// <summary>The amount of images or array length of this <see cref="Texture2DArray"/>.</summary>
        public uint Depth { get; private set; }

        public Texture2DArray(uint width, uint height, uint depth, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(TextureType.Texture2DArray, imageFormat)
        {
            RecreateImage(width, height, depth); //this also binds the texture

            Graphics.GL.TexParameter((TextureTarget)Type, TextureParameterName.TextureMinFilter, (int)DefaultMinFilter);
            Graphics.GL.TexParameter((TextureTarget)Type, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            Graphics.CheckGL();
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <param name="ptr">The pointer from which the pixel data will be read.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectZ">The Z coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="rectDepth">The depth of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetDataPtr(void* ptr, int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth, PixelFormat pixelFormat = 0)
        {
            ValidateRectOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            Graphics.GL.BindTexture((TextureTarget)Type, Handle);
            Graphics.GL.TexSubImage3D((TextureTarget)Type, 0, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
            Graphics.CheckGL();
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectZ">The Z coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="rectDepth">The depth of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetData<T>(ReadOnlySpan<T> data, int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            ValidateRectOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);
            if (data.Length < rectWidth * rectHeight * rectDepth)
                throw new ArgumentException("Not enough pixel data", nameof(data));

            Graphics.GL.BindTexture((TextureTarget)Type, Handle);
            fixed (void* ptr = data)
                Graphics.GL.TexSubImage3D((TextureTarget)Type, 0, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
            Graphics.CheckGL();
        }

        /// <summary>
        /// Sets the data of an entire array layer of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="depthLevel">The array layer to set the data for.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public void SetData<T>(ReadOnlySpan<T> data, int depthLevel, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            SetData(data, 0, 0, depthLevel, Width, Height, 1, pixelFormat);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2DArray"/>.
        /// </summary>
        /// <param name="ptr">The pointer to which the pixel data will be written.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetDataPtr(void* ptr, PixelFormat pixelFormat = 0)
        {
            Graphics.GL.BindTexture((TextureTarget)Type, Handle);
            Graphics.GL.GetTexImage((TextureTarget)Type, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
            Graphics.CheckGL();
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2DArray"/>.
        /// </summary>
        /// <param name="data">A <see cref="Span{T}"/> in which to write the pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetData<T>(Span<T> data, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            if (data.Length < Width * Height * Depth)
                throw new ArgumentException("Insufficient space to store the requested pixel data", nameof(data));

            Graphics.GL.BindTexture((TextureTarget)Type, Handle);
            fixed (void* ptr = data)
                Graphics.GL.GetTexImage((TextureTarget)Type, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
            Graphics.CheckGL();
        }

        /// <summary>
        /// Sets the coordinate wrapping modes for when the <see cref="Texture2DArray"/> is sampled outside the [0, 1] range.
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate.</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate.</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            Graphics.GL.BindTexture((TextureTarget)Type, Handle);
            Graphics.GL.TexParameter((TextureTarget)Type, TextureParameterName.TextureWrapS, (int)sWrapMode);
            Graphics.GL.TexParameter((TextureTarget)Type, TextureParameterName.TextureWrapT, (int)tWrapMode);
            Graphics.CheckGL();
        }

        /// <summary>
        /// Recreates this <see cref="Texture2DArray"/>'s images with a new size,
        /// resizing the <see cref="Texture2DArray"/> but losing the image data.
        /// </summary>
        /// <param name="width">The new width for the <see cref="Texture2DArray"/>.</param>
        /// <param name="height">The new height for the <see cref="Texture2DArray"/>.</param>
        /// <param name="depth">The new depth for the <see cref="Texture2DArray"/>.</param>
        public unsafe void RecreateImage(uint width, uint height, uint depth)
        {
            if (width <= 0 || width > Graphics.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be in the range (0, " + nameof(Graphics.MaxTextureSize) + "]");

            if (height <= 0 || height > Graphics.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(height), height, nameof(height) + " must be in the range (0, " + nameof(Graphics.MaxTextureSize) + "]");

            if (depth <= 0 || depth > Graphics.MaxArrayTextureLayers)
                throw new ArgumentOutOfRangeException(nameof(depth), depth, nameof(depth) + " must be in the range (0, " + nameof(Graphics.MaxArrayTextureLayers) + ")");

            Width = width;
            Height = height;
            Depth = depth;

            Graphics.GL.BindTexture((TextureTarget)Type, Handle);
            Graphics.GL.TexImage3D((TextureTarget)Type, 0, (int)PixelInternalFormat, width, height, depth, 0, PixelFormat, PixelType, (void*)0);
            Graphics.CheckGL();
        }

        private void ValidateRectOperation(int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth)
        {
            if (rectX < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectX), rectX, nameof(rectX) + " must be in the range [0, " + nameof(Width) + ")");

            if (rectY < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectY), rectY, nameof(rectY) + " must be in the range [0, " + nameof(Height) + ")");

            if (rectZ < 0 || rectZ >= Depth)
                throw new ArgumentOutOfRangeException(nameof(rectZ), rectZ, nameof(rectZ) + " must be in the range [0, " + nameof(Depth) + ")");

            if (rectWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, nameof(rectWidth) + " must be greater than 0");

            if (rectHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, nameof(rectHeight) + "must be greater than 0");

            if (rectDepth <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectDepth), rectDepth, nameof(rectDepth) + " must be greater than 0");

            if (rectWidth > Width - rectX || rectHeight > Height - rectY || rectDepth > Depth - rectZ)
                throw new ArgumentOutOfRangeException("Specified area is outside of the texture's storage");
        }
    }
}
