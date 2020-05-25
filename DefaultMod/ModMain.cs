using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Media;
// 1. Added the "GooseModdingAPI" project as a reference.
// 2. Compile this.
// 3. Create a folder with this DLL in the root, and *no GooseModdingAPI DLL*
using GooseShared;
using SamEngine;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace GooseKiller
{
    public class ModEntryPoint : IMod
    {
        // Gets called automatically, passes in a class that contains pointers to
        // useful functions we need to interface with the goose.
        void IMod.Init()
        {
            // Subscribe to whatever events we want
            InjectionPoints.PostTickEvent += PostTick;
            //InjectionPoints.PreRenderEvent += PreRenderEvent;
        }

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
        bool killTheGoose = false;
        int gooseKillTime = 1; //in seconds
        Color white = Color.FromName("White");
        Color red = Color.FromName("Red");
        Color goosecolor = Color.FromName("White");
        Stopwatch s = new Stopwatch();
        Stopwatch s1 = new Stopwatch();
        bool ExploderPlayed = false;
        Vector2 drawspot;
        Random rand = new Random();
        List<Vector2> positions = new List<Vector2> { };

        public void PostTick(GooseEntity g)
        {
            if (GetAsyncKeyState(Keys.ControlKey) != 0 && GetAsyncKeyState(Keys.Menu) != 0 && GetAsyncKeyState(Keys.MButton) != 0)
            {
                killTheGoose = true;
                s.Start();
            }

            if (killTheGoose == true)
            {
                if (goosecolor == white)
                {
                    goosecolor = red;
                }
                else
                {
                    goosecolor = white;
                }
                //API.Goose.playHonckSound();
                g.renderData.brushGooseWhite.Color = goosecolor;
                if (s.Elapsed >= TimeSpan.FromSeconds(gooseKillTime))
                {
                    if (!ExploderPlayed)
                    {
                        s1.Stop();
                        s1 = new Stopwatch();
                        s1.Start();
                        ExploderPlayed = true;
                        g.renderData.brushGooseWhite.Color = goosecolor;
                        new Thread(() =>
                        {
                            using (SoundPlayer exploder = new SoundPlayer(GooseExploder.Properties.Resources.explode3))
                            {
                                exploder.PlaySync();
                            }
                        }).Start();
                    }
                    if (s1.Elapsed >= TimeSpan.FromSeconds(1))
                    {
                        s.Stop();
                        s1.Stop();
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
        }

        static public float ease(float p) //Circular Ease In Function
        {
            p /= 5;
            float f = (p - 1);
            return f * f * f + 1;
        }

        private void PreRenderEvent(GooseEntity goose, Graphics g)
        {
            if (ExploderPlayed)
            {
                goosecolor = Color.FromArgb(0, 255, 255, 255);
                goose.renderData.brushGooseWhite.Color = goosecolor;
                goose.renderData.brushGooseOutline.Color = goosecolor;
                goose.renderData.brushGooseOrange.Color = goosecolor;
                goose.renderData.shadowPen = new Pen(goosecolor);
                Pen[] pens = {new Pen(Color.FromArgb(255, 183, 183, 183), 5), new Pen(Color.FromArgb(255, 183, 183, 183), 5), new Pen(Color.FromArgb(255, 137, 137, 137), 5), new Pen(Color.FromArgb(255, 70, 70, 70), 5) };
                Random rnd = new Random();
                foreach (Vector2 i in positions)
                {
                    g.DrawEllipse(pens[rand.Next(pens.Length)], new Rectangle(Convert.ToInt32(i.x - 2.5), -Convert.ToInt32(i.y - 2.5), 5, 5));
                }
                //drawspot = goose.position;
                drawspot.x = drawspot.x + rnd.Next(-15, +15);
                drawspot.y = drawspot.y + rnd.Next(-15, +15);
                g.DrawEllipse(pens[rand.Next(pens.Length)], new Rectangle(Convert.ToInt32(drawspot.x - 2.5), -Convert.ToInt32(drawspot.y - 2.5), 5, 5));
                positions.Add(drawspot);
            }
        }
    }
}
