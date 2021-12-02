﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;


namespace Projet_IMA
{
    enum ModeAff { SLOW_MODE, FULL_SPEED};

    class BitmapEcran
    {
        const int refresh_every = 1000; // force l'affiche tous les xx pix
        static int nb_pix = 0;                 // comptage des pixels

        static private Bitmap B;
        static private ModeAff Mode;
        static private int Largeur;
        static private int Hauteur;
        static private int stride;
        static private BitmapData data;
        static private Couleur background;
        static private V3 CameraPosition;

        static public Bitmap Init(int largeur, int hauteur)
        {
            Largeur = largeur;
            Hauteur = hauteur;
            B = new Bitmap(largeur, hauteur);
            CameraPosition = new V3(GetWidth() / 2, -1.5f * GetWidth(), GetHeight() / 2);
            return B;
        }

        static void DrawFastPixel(int x, int y, Couleur c)
        {
            unsafe
            {
                byte RR, VV, BB;
                c.check();
                c.To255(out RR, out VV, out BB);

                byte* ptr = (byte*)data.Scan0;
                ptr[(x * 3) + y * stride] = BB;
                ptr[(x * 3) + y * stride + 1] = VV;
                ptr[(x * 3) + y * stride + 2] = RR;
            }
        }

        static void DrawSlowPixel(int x, int y, Couleur c)
        {
            Color cc = c.Convertion();
            B.SetPixel(x, y, cc);

            Program.MyForm.PictureBoxInvalidate();
            nb_pix++;
            if (nb_pix > refresh_every)  // force l'affichage à l'écran tous les 1000pix
            {
                Program.MyForm.PictureBoxRefresh();
                nb_pix = 0;
            }
        }

        /// /////////////////   public methods ///////////////////////

        static public void RefreshScreen()
        {
            Couleur c = background;
            if (Program.MyForm.Checked())
            {
                Mode = ModeAff.SLOW_MODE;
                Graphics g = Graphics.FromImage(B);
                Color cc = c.Convertion();
                g.Clear(cc);
            }
            else
            {
                Mode = ModeAff.FULL_SPEED;
                data = B.LockBits(new Rectangle(0, 0, B.Width, B.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                stride = data.Stride;
                for (int x = 0; x < Largeur; x++)
                    for (int y = 0; y < Hauteur; y++)
                        DrawFastPixel(x, y, c);
            }
        }


        public static void DrawPixel(int x, int y, Couleur c)
        {
            int x_ecran = x;
            int y_ecran = Hauteur - y;

            if ((x_ecran >= 0) && (x_ecran < Largeur) && (y_ecran >= 0) && (y_ecran < Hauteur))
                if (Mode == ModeAff.SLOW_MODE) DrawSlowPixel(x_ecran, y_ecran, c);
                else DrawFastPixel(x_ecran, y_ecran, c);
        }

        static public void Show()
        {
            if (Mode == ModeAff.FULL_SPEED)
                B.UnlockBits(data);

            Program.MyForm.PictureBoxInvalidate();
        }

        static public int GetWidth() { return Largeur; }
        static public int GetHeight() { return Hauteur; }
        static public V3 GetCameraPosition() { return CameraPosition; }

        static public void setBackground(Couleur c)
        {
            background = c;
        }

         static Couleur RayCast(V3 PosCamera, V3 DirRayon, ArrayList objets)
         {
            float maxT = float.MaxValue;
            Couleur finalColor = Couleur.m_Void;
            foreach(Objet3D objet in objets)
            {
                if (objet.testIntersection(PosCamera, DirRayon, out float t, out V3 PixelPosition, out float u, out float v))
                {
                    if (t>0 && t < maxT)
                    {
                        maxT = t;
                        finalColor = objet.getCouleur(PixelPosition, u, v);
                    }
                }
            }
            return finalColor;
         }
        static public void DrawAll(ArrayList objects)
        {
            for (int x_ecran = 0; x_ecran <= GetWidth(); x_ecran++)
            {
                for (int y_ecran = 0; y_ecran <= GetHeight(); y_ecran++)
                {
                    V3 PosPixScene = new V3(x_ecran, 0, y_ecran);
                    V3 DirRayon = PosPixScene - CameraPosition;
                    Couleur C = RayCast(CameraPosition, DirRayon, objects);
                    DrawPixel(x_ecran, y_ecran, C);
                }
            }
        }
    }

   
}
