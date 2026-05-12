using Android.Graphics;
using Android.Opengl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XRay_Earth.Platforms.Android
{
    internal class Texture
    {
        private int _Id;
        private bool _isLoaded = false;
        private bool _isUploaded = false;

        private Bitmap _bitmap;

        public bool IsLoaded {  get { return _isLoaded; } }
        public bool IsUploaded { get { return _isUploaded; } }
        public int ID { get { return _Id; } }

        public Texture(string filename)
        {
            _ = LoadFromBitMap(filename);
        }

        //  Load texture from file.
        private async Task LoadFromBitMap(string filename)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
                _bitmap = BitmapFactory.DecodeStream(stream);

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFromBitMap error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        public void UploadTexture()
        {
            // Get a texture ID for later use
            int[] ids = new int[1];
            GLES20.GlGenTextures(1, ids, 0);
            _Id = ids[0];

            //  Sets that ID for whats to come
            GLES20.GlBindTexture(GLES20.GlTexture2d, _Id);
            //  Upload texture into the ID
            GLUtils.TexImage2D(GLES20.GlTexture2d, 0, _bitmap, 0);

            //  Delete the bitmap in C# cause the GPU has it now.
            _bitmap.Recycle();

            //  Sets the horizontal and vertical wrapping mode to repeat
            //  S is horizontal and T is vertical
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlRepeat);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlRepeat);

            //  Sets the minify and magnify filtering mode to linear.
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);

            _isUploaded = true;

        }
    }

}
