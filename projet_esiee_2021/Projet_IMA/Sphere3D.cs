﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_IMA
{
    class Sphere3D : Objet3D
    {
        public float m_Rayon { get; set; }

        public Sphere3D(V3 centre, float rayon, Couleur couleur, Lumiere lumiere, float coefficient_diffus = 0.006f) : base(centre, couleur, lumiere, coefficient_diffus)
        {
            this.m_Rayon = rayon;
        }

        public override void Draw(float pas=.005f)
        {
            for (float u = 0; u < 2 * IMA.PI; u += pas)
            {  // echantillonage fnt paramétrique
                for (float v = -IMA.PI / 2; v < IMA.PI / 2; v += pas)
                {
                    // calcul des coordoonées dans la scène 3D
                    float x3D = m_Rayon * IMA.Cosf(v) * IMA.Cosf(u) + this.m_CentreObjet.x;
                    float y3D = m_Rayon * IMA.Cosf(v) * IMA.Sinf(u) + this.m_CentreObjet.y;
                    float z3D = m_Rayon * IMA.Sinf(v) + this.m_CentreObjet.z;
                    V3 normalizedPixelNormal = (new V3(x3D - this.m_CentreObjet.x, y3D - this.m_CentreObjet.y, z3D - this.m_CentreObjet.z));
                    normalizedPixelNormal.Normalize();

                    // projection orthographique => repère écran

                    int x_ecran = (int)(x3D);
                    int y_ecran = (int)(z3D);


                    float u1 = (u) / (2 * IMA.PI);
                    float v1 = (v) / (2 * IMA.PI);

                    BitmapEcran.DrawPixel(x_ecran, y_ecran, getCouleurDiffuse(normalizedPixelNormal, u1, -v1));// + getCouleurSpeculaire(x3D, y3D, z3D));//
                }
            }
        }
    }
}
