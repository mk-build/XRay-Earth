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

        private async Task LoadFromBitMap(string filename)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
            _bitmap = BitmapFactory.DecodeStream(stream);

           _isLoaded = true;
        }

        public void UploadTexture()
        {

            int[] ids = new int[1];
            GLES20.GlGenTextures(1, ids, 0);
            _Id = ids[0];

            GLES20.GlBindTexture(GLES20.GlTexture2d, _Id);
            GLUtils.TexImage2D(GLES20.GlTexture2d, 0, _bitmap, 0);

            _bitmap.Recycle();

            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlRepeat);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlRepeat);

            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);

            _isUploaded = true;

        }
    }

}
