using System;
using System.Security.Cryptography.X509Certificates;
using Control;
using Escenografia;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Data;
using System.Collections.Generic;
using BepuPhysics.Collidables;
using BepuUtilities.Collections;

abstract class PowerUp
{
    public string tipoPowerUp;
    public float DuracionPowerUp; // Duración del power-up en segundos

    public abstract void ActivarPowerUp(AutoJugador auto);
    public abstract void DesactivarPowerUp(AutoJugador auto);
    public abstract void ActualizarPowerUp(GameTime gameTime);

    public void ActivarPowerUp(string tipoPowerUp)
    {
    
    }
}

class Turbo : PowerUp
{
    private float boostVelocidad;
    private AutoJugador auto;
    public Turbo()
    {
        tipoPowerUp = "Turbo";
        DuracionPowerUp = 5f;
        boostVelocidad = 150f;
    }

    public override void ActivarPowerUp(AutoJugador auto)
    {
        auto.velocidad += boostVelocidad;

        Console.WriteLine("Turbo activado");
    }

    public override void DesactivarPowerUp(AutoJugador auto)
    {
        auto.velocidad -= boostVelocidad;
        Console.WriteLine("Turbo desactivado");
    }

    public override void ActualizarPowerUp(GameTime gameTime)
    {
        DuracionPowerUp -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (DuracionPowerUp <= 0)
        {
            DesactivarPowerUp(auto);
        }
    }
}

class Misil : PowerUp
{

    private AutoJugador auto;
    private int MunicionMisiles = 0;

    public Misil()
    {
        tipoPowerUp = "Misil";
        MunicionMisiles += 5;

    }

    public override void ActivarPowerUp(AutoJugador auto)
    {
        Console.WriteLine("Cantidad de misiles : " + MunicionMisiles);
    }

    public override void DesactivarPowerUp(AutoJugador auto)
    {

        Console.WriteLine("Misiles desactivados");
        MunicionMisiles = 0;
    }

    public override void ActualizarPowerUp(GameTime gameTime)
    {
        DuracionPowerUp -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (DuracionPowerUp <= 0 || MunicionMisiles <= 0)
        {
          DesactivarPowerUp(auto);
        }
    }
}