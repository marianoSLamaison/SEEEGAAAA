using System;
using System.Collections.Generic;
using Escenografia;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Control
{
    class AdminUtileria
    {
        Escenografia.LimBox limites;
        private List<Escenografia.Escenografia3D> objetosFijos;
        public AdminUtileria(Vector3 minLims, Vector3 maxLims)
        {
            limites = new Escenografia.LimBox(minLims, maxLims);
            Vector3 dimenciones = maxLims - minLims;
            objetosFijos = new List<Escenografia.Escenografia3D>
            {
                new Escenografia.Plataforma(Convert.ToSingle(3*Math.PI / 2), minLims),
                new Escenografia.Plataforma(Convert.ToSingle(Math.PI), new Vector3(minLims.X + dimenciones.X,0f, minLims.Z)),
                new Escenografia.Plataforma(Convert.ToSingle(0),  new Vector3(minLims.X, 0f, minLims.Z + dimenciones.Z)),
                new Escenografia.Plataforma(Convert.ToSingle(Math.PI / 2), maxLims)
            };
        }
        public void loadPlataformas(string direcionModelo, string direccionEfecto, ContentManager contManager)
        {
            if (objetosFijos.Count > 4) throw new Exception("Esto era un metodo de prueba");
        
            foreach(Plataforma plataforma in objetosFijos)
            {
                plataforma.loadModel(direcionModelo,direccionEfecto,contManager);
            }
        }
        public void Dibujar(Camarografo camarografo)
        {
            foreach (Escenografia.Escenografia3D objeto in objetosFijos)
            {
                objeto.dibujar(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.Purple);
            }
        }
    }
}