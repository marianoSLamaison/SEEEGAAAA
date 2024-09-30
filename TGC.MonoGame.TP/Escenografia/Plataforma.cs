using System.Runtime.ConstrainedExecution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Escenografia
{
    class Plataforma : Escenografia3D
    {
        private Texture2D texture;
        static private float g_scale;
        public Plataforma(float rotacionY, Vector3 posicion)
        {
            this.rotacionY = rotacionY;
            this.posicion = posicion;
        }

        public void SetTexture(Texture2D unaTextura){
            this.texture = unaTextura;
        }
        public override Matrix getWorldMatrix()
        {
            return Matrix.CreateScale(g_scale*1.6f) * Matrix.CreateRotationY(rotacionY) * Matrix.CreateTranslation(posicion);
        }

        public static void setGScale(float scale)
        {
            g_scale = scale;
        }
        public override void loadModel(string direcionModelo, string direccionEfecto, ContentManager contManager) 
        {
            base.loadModel(direcionModelo, direccionEfecto, contManager);
                        foreach ( ModelMesh mesh in modelo.Meshes )
            {
                foreach ( ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efecto;
                }
            }
        }
    }
}