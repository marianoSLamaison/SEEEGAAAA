using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Escenografia
{
    public class Terreno : Escenografia3D
    {
        private VertexPositionNormalTexture[] vertices;
        private int[] indices;
        private Texture2D heightMapTexture;
        private Texture2D terrenoTextureDiffuse;
        private Texture2D terrenoTextureNormal;
        private Texture2D terrenoTextureHeight;

        private float[,] heightData;
        private int width, height;

        /// <summary>
        /// Constructor para inicializar el terreno con un heightmap.
        /// </summary>
        /// <param name="heightMapPath">La ruta de la imagen del heightmap.</param>
        /// <param name="content">El ContentManager del juego.</param>
        /// <param name="alturaMaxima">Altura máxima del terreno basado en el heightmap.</param>
        /// 
        public void CargarTerreno(string heightMapPath, ContentManager content, float alturaMaxima)
        {
            // Cargar el heightmap como textura
            heightMapTexture = content.Load<Texture2D>(heightMapPath);
            terrenoTextureDiffuse = content.Load<Texture2D>("Models/Terreno/"+"diffuseColor");
            terrenoTextureHeight = content.Load<Texture2D>("Models/Terreno/"+"OrangeRockTexture");
            terrenoTextureNormal = content.Load<Texture2D>("Models/Terreno/"+"normal");
            width = heightMapTexture.Width;
            height = heightMapTexture.Height;

            // Extraer datos de altura del heightmap
            Color[] heightMapColors = new Color[width * height];
            heightMapTexture.GetData(heightMapColors);

            heightData = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * width].R / 255.0f * alturaMaxima;
                }
            }

            // Crear el mesh (malla) del terreno
            GenerarVertices();
            GenerarIndices();
        }

        public void SetEffect (Effect effect){
            this.efecto = effect;
        }

              public void ApplyTexturesToShader()
        {
            efecto.Parameters["TerrenoTexture"].SetValue(heightMapTexture);
        }

        /// <summary>
        /// Generar los vértices del terreno basados en el heightmap.
        /// </summary>
        private void GenerarVertices()
        {
            vertices = new VertexPositionNormalTexture[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 posicion = new Vector3(x, heightData[x, y], y);
                    Vector2 texCoord = new Vector2((float)x / (width - 1), (float)y / (height - 1));
                    vertices[x + y * width] = new VertexPositionNormalTexture(posicion, Vector3.Up, texCoord);
                }
            }

            // Puedes calcular las normales más adelante si es necesario.
        }

        /// <summary>
        /// Generar los índices para los triángulos del terreno.
        /// </summary>
        private void GenerarIndices()
        {
            indices = new int[(width - 1) * (height - 1) * 6]; // 6 índices por cuadrado (2 triángulos por cuadrado)
            int indice = 0;

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    // Triángulo 1
                    indices[indice++] = x + y * width;
                    indices[indice++] = (x + 1) + y * width;
                    indices[indice++] = x + (y + 1) * width;

                    // Triángulo 2
                    indices[indice++] = (x + 1) + y * width;
                    indices[indice++] = (x + 1) + (y + 1) * width;
                    indices[indice++] = x + (y + 1) * width;
                }
            }
        }

        /// <summary>
        /// Devuelve la matriz de transformación mundial del terreno.
        /// </summary>
        public override Matrix getWorldMatrix()
        {
            return
                Matrix.CreateTranslation(posicion - new Vector3 (250f, 12.5f, 250f)) *
                Matrix.CreateScale(45f) *
                Matrix.CreateRotationX(rotacionX) *
                Matrix.CreateRotationY(rotacionY) *
                Matrix.CreateRotationZ(rotacionZ);
        }

        /// <summary>
        /// Sobreescribe el método para dibujar el terreno.
        /// </summary>
        public override void dibujar(Matrix view, Matrix projection, Color color)
        {
            efecto.Parameters["View"].SetValue(view);
            efecto.Parameters["Projection"].SetValue(projection);
            efecto.Parameters["DiffuseColor"]?.SetValue(color.ToVector3());

            efecto.Parameters["World"].SetValue(getWorldMatrix());

            foreach (var pass in efecto.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice device = efecto.GraphicsDevice;
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }
    }
}